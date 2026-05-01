namespace Data.Repositories.Interfaces
{
    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Guid id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
