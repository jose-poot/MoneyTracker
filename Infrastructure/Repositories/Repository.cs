using Microsoft.EntityFrameworkCore;
using MoneyTracker.Core.Interfaces.Repositories;
using MoneyTracker.Infrastructure.Database;
using System.Linq.Expressions;

namespace MoneyTracker.Infrastructure.Repositories;

/// <summary>
/// Implementación base del patrón Repository usando Entity Framework
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly MoneyTrackerContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(MoneyTrackerContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error getting entity by id {id}: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting all entities: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            var entry = await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error adding entity: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating entity: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting entity with id {id}: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error finding entities: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting first entity: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        try
        {
            return predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error counting entities: {ex.Message}");
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return await _dbSet.AnyAsync(predicate);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking if entity exists: {ex.Message}");
            throw;
        }
    }
}