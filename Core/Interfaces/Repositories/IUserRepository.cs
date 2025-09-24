using MoneyTracker.Core.Entities;

namespace MoneyTracker.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetActiveAsync(); 
    Task UpdateAsync(User user);
    Task<int> SaveChangesAsync();
}