using Microsoft.EntityFrameworkCore;
using ProfileService.Data;

namespace ProfileService.Repositories
{
    public class CrudRepository<T, U>(ProfileDbContext context) : ICrudRepository<T, U> where T : class
    {
        private readonly ProfileDbContext context = context;
        private readonly DbSet<T> dbSet = context.Set<T>();
        public async Task CreateAsync(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public async Task<T?> GetByIdAsync(U id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task SaveAsync() => await context.SaveChangesAsync();
    }
}