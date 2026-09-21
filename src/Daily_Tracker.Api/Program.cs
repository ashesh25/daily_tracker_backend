
using Daily_Tracker.Api.Endpoints;
using Daily_Tracker.Infrastructure.Persistence;
using Daily_Tracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Daily_Tracker.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });

        builder.Services.AddAuthorization();

        var frontendOrigins = builder.Configuration["Frontend:Origins"] ?? builder.Configuration["Frontend:Origin"];
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                var origins = new List<string> { "http://localhost:5173" };
                if (!string.IsNullOrWhiteSpace(frontendOrigins))
                {
                    origins.AddRange(frontendOrigins
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(origin => origin.TrimEnd('/')));
                }

                policy.WithOrigins(origins.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddDbContext<DailyTrackerDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<TenantRepository>();
        builder.Services.AddScoped<WorkoutRepository>();
        builder.Services.AddScoped<UserRepository>();

        var app = builder.Build();

        if (!app.Environment.IsProduction())
        {
            app.UseHttpsRedirection();
        }

        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapAuthEndpoints();
        app.MapTenantEndpoints();
        app.MapWorkoutEndpoints();

        app.Run();
    }
}
