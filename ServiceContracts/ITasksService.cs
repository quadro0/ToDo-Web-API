using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface ITasksService
    {
        /// <summary>
        /// Adds new task for the current user, throws if fails
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="taskAddRequest">Model of the task that is being added</param>
        /// <returns>Returns added task</returns>
        Task<TaskResponse> AddAsync(Guid userId, TaskAddRequest taskAddRequest);

        /// <summary>
        /// Retrieves task by provided Id if the current logged user is it's creator
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="id">Id of the desired task</param>
        /// <returns>Returns task by provided Id</returns>
        Task<TaskResponse?> GetByIdAsync(Guid userId, Guid id);

        /// <summary>
        /// Retrieves all tasks filtered and paginated by provided parameters of the current logged user
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="parameters">Parameters to filter and paginate tasks</param>
        /// <returns>Returns filtered and paginated tasks</returns>
        Task<TasksPaginatedResponse> GetPaginatedAsync(Guid userId, TasksPaginationParameters parameters);

        /// <summary>
        /// Updates provided task if it belongs to the current user, throws if fails
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="taskUpdateRequest">Model of the task being updated</param>
        /// <returns>Returns updated task</returns>
        Task<TaskResponse> UpdateAsync(Guid userId, TaskUpdateRequest taskUpdateRequest);

        /// <summary>
        /// Deletes task by provided Id if it belongs to the current user, throws if fails
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="id">Id of the task to be deleted</param>
        Task DeleteAsync(Guid userId, Guid id);
    }
}
