using Daily_Tracker.Domain.Entities;
using Daily_Tracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daily_Tracker.Infrastructure.Repositories;

public class WorkoutRepository
{
    private readonly DailyTrackerDbContext _dbContext;

    public WorkoutRepository(DailyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<WorkoutEntry>> GetByTenantAsync(Guid tenantId)
    {
        return await _dbContext.WorkoutEntries
            .Include(x => x.Exercises)
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.CreatedUtc)
            .ToListAsync();
    }

    public async Task<WorkoutEntry?> GetByIdAsync(Guid tenantId, Guid workoutId)
    {
        return await _dbContext.WorkoutEntries
            .Include(x => x.Exercises)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workoutId);
    }

    public async Task<WorkoutEntry> CreateAsync(WorkoutEntry workout)
    {
        _dbContext.WorkoutEntries.Add(workout);
        await _dbContext.SaveChangesAsync();
        return workout;
    }

    public async Task<bool> DeleteAsync(Guid tenantId, Guid workoutId)
    {
        var workout = await _dbContext.WorkoutEntries
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == workoutId);

        if (workout is null)
        {
            return false;
        }

        _dbContext.WorkoutEntries.Remove(workout);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
