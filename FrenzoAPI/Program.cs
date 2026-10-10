using System.Text;
using FrenZoAPI.DataContext;
using FrenZoAPI.Services;
using FrenZoAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
