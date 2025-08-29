namespace ProfileService.Repositories
{
    public interface ICrudRepository<T, U>
    {
        Task CreateAsync(T entity);
        Task<T?> GetByIdAsync(U id);
        Task SaveAsync();
    }
}