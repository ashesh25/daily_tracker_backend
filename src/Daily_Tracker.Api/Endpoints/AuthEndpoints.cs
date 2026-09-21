using Daily_Tracker.Api.Contracts;
using Daily_Tracker.Domain.Entities;
using Daily_Tracker.Infrastructure.Persistence;
using Daily_Tracker.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Daily_Tracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register-tenant", async (RegisterTenantRequest request, DailyTrackerDbContext dbContext, UserRepository userRepository, TenantRepository tenantRepository, IConfiguration configuration) =>
        {
            if (string.IsNullOrWhiteSpace(request.TenantName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Tenant name, email, and password are required.");
            }

            if (request.Password.Length < 8)
            {
                return Results.BadRequest("Password must be at least 8 characters long.");
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var slug = string.IsNullOrWhiteSpace(request.TenantSlug)
                ? request.TenantName.Trim().ToLowerInvariant().Replace(" ", "-")
                : request.TenantSlug.Trim().ToLowerInvariant();

            if (await tenantRepository.GetBySlugAsync(slug) is not null)
            {
                return Results.Conflict("A tenant with this slug already exists.");
            }

            if (await userRepository.GetByEmailAsync(email) is not null)
            {
                return Results.Conflict("User already exists for this email.");
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var tenant = new Tenant
            {
                Name = request.TenantName.Trim(),
                Slug = slug,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();

            var user = new ApplicationUser
            {
                Email = email,
                TenantId = tenant.Id,
                Role = "Owner",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok(new AuthResponse
            {
                Token = GenerateJwtToken(user, configuration),
                TenantId = tenant.Id,
                Email = user.Email,
                Role = user.Role
            });
        });

        group.MapPost("/register", async (HttpContext httpContext, RegisterRequest request, UserRepository userRepository, TenantRepository tenantRepository, IConfiguration configuration) =>
        {
            var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
            var tenantClaim = httpContext.User.FindFirst("tenant_id")?.Value;
            var canRegisterMember = string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(role, "Owner", StringComparison.OrdinalIgnoreCase);

            if (!canRegisterMember ||
                !Guid.TryParse(tenantClaim, out var authenticatedTenantId) ||
                authenticatedTenantId != request.TenantId)
            {
                return Results.Forbid();
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Email and password are required.");
            }

            var tenant = await tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant is null)
            {
                return Results.BadRequest("Tenant does not exist.");
            }

            if (await userRepository.GetByEmailAsync(request.Email.Trim()) is not null)
            {
                return Results.Conflict("User already exists for this email.");
            }

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var user = new ApplicationUser
            {
                Email = request.Email.Trim(),
                TenantId = request.TenantId,
                Role = "Member",
                PasswordHash = string.Empty,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            var created = await userRepository.CreateAsync(user);

            return Results.Ok(new AuthResponse
            {
                Token = GenerateJwtToken(created, configuration),
                TenantId = created.TenantId,
                Email = created.Email,
                Role = created.Role
            });
        }).RequireAuthorization();

        group.MapPost("/login", async (LoginRequest request, UserRepository userRepository, IConfiguration configuration) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Email and password are required.");
            }

            var user = await userRepository.GetByEmailAsync(request.Email.Trim());
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new AuthResponse
            {
                Token = GenerateJwtToken(user, configuration),
                TenantId = user.TenantId,
                Email = user.Email,
                Role = user.Role
            });
        });
    }

    private static string GenerateJwtToken(ApplicationUser user, IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "DailyTrackerApi";
        var audience = configuration["Jwt:Audience"] ?? "DailyTrackerClients";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("tenant_id", user.TenantId.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
