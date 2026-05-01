using Data.Entities;

namespace Data.Repositories.Interfaces
{
    public interface ITasksRepository : IRepository<TaskEntity>
    {
        Task<IEnumerable<TaskEntity>> GetAllAsync(int pageNumber, int count);
    }
}
