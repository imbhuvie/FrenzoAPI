using System.Text;
using FrenZoAPI.Common.Middleware;
using FrenZoAPI.DataContext;
using FrenZoAPI.Services;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

// Registers controllers and the Dev-only OpenAPI document.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Binds AppDbContext to Postgres via the DefaultConnection string.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Exposes auth business rules to controllers.
builder.Services.AddScoped<IAuthService, AuthService>();

// Signs access tokens after login (claims: sub, unique_name, email).
builder.Services.AddScoped<ITokenService, TokenService>();

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

app.Run();
