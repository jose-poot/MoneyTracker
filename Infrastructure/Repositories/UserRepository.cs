using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Entities;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Infrastructure.Database;

namespace MoneyTracker.Infrastructure.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(MoneyTrackerContext context) : base(context) { }

    // Lecturas sin tracking (opcional, recomendado para solo-lectura)
    public override Task<User?> GetByIdAsync(int id) =>
        _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(string email) =>
        _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetActiveAsync() =>
        _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.IsActive);

    // Opción A1: delegar al base (que guarda internamente)
    public override Task<User> UpdateAsync(User user) => base.UpdateAsync(user); // base llama SaveChangesAsync

    // Si tu interfaz exige exponer SaveChangesAsync, impleméntalo correctamente:
    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
