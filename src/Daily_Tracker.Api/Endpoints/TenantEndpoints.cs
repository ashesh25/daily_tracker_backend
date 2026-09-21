using Daily_Tracker.Api.Contracts;
using Daily_Tracker.Domain.Entities;
using Daily_Tracker.Infrastructure.Repositories;
using System.Security.Claims;

namespace Daily_Tracker.Api.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants")
            .RequireAuthorization()
            .WithTags("Tenants");

        group.MapGet("", async (HttpContext httpContext, TenantRepository repository) =>
        {
            var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                httpContext.User.Identity?.IsAuthenticated == true)
            {
                var tenants = await repository.GetAllAsync();
                return Results.Ok(tenants);
            }

            return Results.Forbid();
        });

        group.MapGet("{tenantId:guid}", async (HttpContext httpContext, Guid tenantId, TenantRepository repository) =>
        {
            if (!IsTenantAllowed(httpContext, tenantId))
            {
                return Results.Forbid();
            }

            var tenant = await repository.GetByIdAsync(tenantId);
            return tenant is null ? Results.NotFound() : Results.Ok(tenant);
        });

        group.MapPost("", async (HttpContext httpContext, CreateTenantRequest request, TenantRepository repository) =>
        {
            if (!httpContext.User.Identity?.IsAuthenticated ?? false)
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Results.BadRequest("Tenant name is required.");
            }

            var slug = string.IsNullOrWhiteSpace(request.Slug)
                ? request.Name.Trim().ToLowerInvariant().Replace(" ", "-")
                : request.Slug.Trim();

            var existing = await repository.GetBySlugAsync(slug);
            if (existing is not null)
            {
                return Results.Conflict("A tenant with this slug already exists.");
            }

            var tenant = new Tenant
            {
                Name = request.Name.Trim(),
                Slug = slug,
                IsActive = true,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow
            };

            var created = await repository.CreateAsync(tenant);
            return Results.Created($"/api/tenants/{created.Id}", created);
        });

        group.MapDelete("{tenantId:guid}", async (HttpContext httpContext, Guid tenantId, TenantRepository repository) =>
        {
            if (!IsTenantAllowed(httpContext, tenantId))
            {
                return Results.Forbid();
            }

            var deleted = await repository.DeleteAsync(tenantId);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }

    private static bool IsTenantAllowed(HttpContext httpContext, Guid tenantId)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        var tenantClaim = httpContext.User.FindFirst("tenant_id");
        if (tenantClaim is null || !Guid.TryParse(tenantClaim.Value, out var currentTenantId))
        {
            return false;
        }

        return currentTenantId == tenantId ||
               string.Equals(httpContext.User.FindFirstValue(ClaimTypes.Role), "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
