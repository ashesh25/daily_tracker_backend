using Daily_Tracker.Domain.Entities;
using Daily_Tracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daily_Tracker.Infrastructure.Repositories;

public class TenantRepository
{
    private readonly DailyTrackerDbContext _dbContext;

    public TenantRepository(DailyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        return await _dbContext.Tenants
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Tenant?> GetByIdAsync(Guid tenantId)
    {
        return await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == tenantId);
    }

    public async Task<Tenant?> GetBySlugAsync(string slug)
    {
        return await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();
        return tenant;
    }

    public async Task<bool> DeleteAsync(Guid tenantId)
    {
        var tenant = await _dbContext.Tenants
            .FirstOrDefaultAsync(x => x.Id == tenantId);

        if (tenant is null)
        {
            return false;
        }

        _dbContext.Tenants.Remove(tenant);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
