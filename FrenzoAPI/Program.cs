using FrenZoAPI.DataContext;
using FrenZoAPI.Services;
using FrenZoAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
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

WebApplication app = builder.Build();

// Serves the OpenAPI JSON plus the Scalar browser UI in Development only.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
