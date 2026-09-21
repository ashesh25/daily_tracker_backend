using Daily_Tracker.Api.Contracts;
using Daily_Tracker.Domain.Entities;
using Daily_Tracker.Infrastructure.Repositories;
using System.Security.Claims;

namespace Daily_Tracker.Api.Endpoints;

public static class WorkoutEndpoints
{
    public static void MapWorkoutEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tenants/{tenantId:guid}/workouts")
            .RequireAuthorization()
            .WithTags("Workouts");

        group.MapGet("", async (HttpContext httpContext, Guid tenantId, TenantRepository tenantRepository, WorkoutRepository workoutRepository) =>
        {
            if (!IsTenantAllowed(httpContext, tenantId))
            {
                return Results.Forbid();
            }

            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant is null)
            {
                return Results.NotFound();
            }

            var workouts = await workoutRepository.GetByTenantAsync(tenantId);
            return Results.Ok(workouts);
        });

        group.MapGet("{workoutId:guid}", async (HttpContext httpContext, Guid tenantId, Guid workoutId, TenantRepository tenantRepository, WorkoutRepository workoutRepository) =>
        {
            if (!IsTenantAllowed(httpContext, tenantId))
            {
                return Results.Forbid();
            }

            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant is null)
            {
                return Results.NotFound();
            }

            var workout = await workoutRepository.GetByIdAsync(tenantId, workoutId);
            return workout is null ? Results.NotFound() : Results.Ok(workout);
        });

        group.MapPost("", async (HttpContext httpContext, Guid tenantId, CreateWorkoutRequest request, TenantRepository tenantRepository, WorkoutRepository workoutRepository) =>
        {
            if (!IsTenantAllowed(httpContext, tenantId))
            {
                return Results.Forbid();
            }

            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant is null)
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest("Workout title is required.");
            }

            var workout = new WorkoutEntry
            {
                TenantId = tenantId,
                Title = request.Title.Trim(),
                Date = request.Date,
                Notes = request.Notes,
                DurationMinutes = request.DurationMinutes,
                CaloriesBurned = request.CaloriesBurned,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                Exercises = request.Exercises.Select(x => new WorkoutExercise
                {
                    Name = x.Name.Trim(),
                    Sets = x.Sets,
                    Reps = x.Reps,
                    WeightKg = x.WeightKg,
                    DurationMinutes = x.DurationMinutes,
                    Notes = x.Notes,
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow
                }).ToList()
            };

            var created = await workoutRepository.CreateAsync(workout);
            return Results.Created($"/api/tenants/{tenantId}/workouts/{created.Id}", created);
        });

        group.MapDelete("{workoutId:guid}", async (HttpContext httpContext, Guid tenantId, Guid workoutId, TenantRepository tenantRepository, WorkoutRepository workoutRepository) =>
        {
            if (!IsTenantAllowed(httpContext, tenantId))
            {
                return Results.Forbid();
            }

            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant is null)
            {
                return Results.NotFound();
            }

            var deleted = await workoutRepository.DeleteAsync(tenantId, workoutId);
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
