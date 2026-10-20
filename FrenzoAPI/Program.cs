using System.Text;
using FrenZoAPI.Common.Middleware;
using FrenZoAPI.DataContext;
using FrenZoAPI.Hubs;
using FrenZoAPI.Services;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Serilog;

// Persists logs to daily rolling files (DB-independent, survives restarts); 30 files retained.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.File("logs/frenzoapi-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {CorrelationId} {SourceContext} {Message}{NewLine}{Exception}")
    .CreateLogger();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Routes all ILogger output (including the exception middleware) through Serilog.
builder.Host.UseSerilog();

// Registers controllers and the Dev-only OpenAPI document (Bearer scheme feeds Scalar's Auth button).
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi(options =>
{
    // Declares the JWT scheme once so locked endpoints show the global Authorize input.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        OpenApiSecurityScheme bearerScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the login access token. Scalar sends it as: Bearer <token>."
        };
        document.Components.SecuritySchemes["Bearer"] = bearerScheme;
        return Task.CompletedTask;
    });

    // Marks only [Authorize] operations as locked (login/signup stay open).
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        IList<object> endpointMetadata = context.Description.ActionDescriptor?.EndpointMetadata ?? new List<object>();
        bool requiresAuth = endpointMetadata.OfType<IAuthorizeData>().Any();
        if (requiresAuth)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();
            OpenApiSecurityScheme schemeReference = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            };
            OpenApiSecurityRequirement requirement = new OpenApiSecurityRequirement
            {
                [schemeReference] = new List<string>()
            };
            operation.Security.Add(requirement);
        }
        return Task.CompletedTask;
    });
});

// Binds AppDbContext to Postgres via the DefaultConnection string.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Exposes auth business rules to controllers.
builder.Services.AddScoped<IAuthService, AuthService>();

// Signs access tokens after login (claims: sub, unique_name, email).
builder.Services.AddScoped<ITokenService, TokenService>();

// Builds the chatlist rows for the signed-in caller.
builder.Services.AddScoped<IConversationService, ConversationService>();

// Persists chat messages for the realtime hub to broadcast.
builder.Services.AddScoped<IMessageService, MessageService>();

// Reads the shared Jwt settings once (R6: no config calls inside the options lambda conditions).
string jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
string jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "FrenZoAPI";
string jwtAudience = builder.Configuration["Jwt:Audience"] ?? "FrenZoAPI";

// Validates the Bearer token on every later request (signature + issuer + audience + expiry).
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // WebSockets cannot set headers, so the hub token arrives as ?access_token= (hub paths only).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                string? accessToken = context.Request.Query["access_token"];
                string? path = context.HttpContext.Request.Path.Value;
                bool isHubPath = path is not null && path.StartsWith("/hubs/chat");
                bool isMissingToken = string.IsNullOrWhiteSpace(accessToken);
                bool useQueryToken = isHubPath && !isMissingToken;
                if (useQueryToken)
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Assigns the correlation id first, then catches unhandled failures (R11/R12 ordering).
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

// Serves the OpenAPI JSON plus the Scalar browser UI in Development only.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Authenticates the Bearer token first, then enforces [Authorize] policies.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Realtime chat hub (JWT via Authorization header or ?access_token=, [Authorize] enforced).
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
