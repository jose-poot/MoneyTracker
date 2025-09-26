using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Infrastructure.Database;

namespace MoneyTracker.Infrastructure.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(MoneyTrackerContext context) : base(context) { }

    // Use no-tracking reads (optional, recommended for read-only scenarios)
    public override Task<User?> GetByIdAsync(int id) =>
        _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetActiveAsync() =>
        _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.IsActive);

    // Option A1: delegate to the base class (which persists internally)
    async Task IUserRepository.UpdateAsync(User user)
    {
        _ = await base.UpdateAsync(user);
    }

    // If the interface requires exposing SaveChangesAsync, implement it properly:
    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
