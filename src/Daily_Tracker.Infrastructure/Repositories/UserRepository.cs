using Daily_Tracker.Domain.Entities;
using Daily_Tracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Daily_Tracker.Infrastructure.Repositories;

public class UserRepository
{
    private readonly DailyTrackerDbContext _dbContext;

    public UserRepository(DailyTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }
}
