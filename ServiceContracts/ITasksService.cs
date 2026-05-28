using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface ITasksService
    {
        Task<TaskResponse> AddAsync(Guid userId, TaskAddRequest taskAddRequest);
        Task<TaskResponse?> GetByIdAsync(Guid userId, Guid id);
        Task<TasksPaginatedResponse> GetPaginatedAsync(Guid userId, TasksPaginationParameters parameters);
        Task<TaskResponse> UpdateAsync(Guid userId, TaskUpdateRequest taskUpdateRequest);
        Task DeleteAsync(Guid userId, Guid id);
    }
}
