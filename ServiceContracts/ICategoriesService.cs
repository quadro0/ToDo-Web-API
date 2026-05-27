using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface ICategoriesService
    {
        /// <summary>
        /// Adds new task category for tasks for the current user, throws if fails
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="categoryAddRequest">Model of the category that is being added</param>
        /// <returns>Returns added category</returns>
        Task<CategoryResponse> AddAsync(Guid userId, CategoryAddRequest categoryAddRequest);

        /// <summary>
        /// Retrieves task category by provided Id if the current logged user is it's creator
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="id">Id of the desired category</param>
        /// <returns>Returns category by provided Id</returns>
        Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid id);

        /// <summary>
        /// Retrieves all task categories of the current logged user
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <returns>Returns all task categories</returns>
        Task<List<CategoryResponse>> GetAllAsync(Guid userId);

        /// <summary>
        /// Updates provided task category if it belongs to the current user, throws if fails
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="categoryUpdateRequest">Model of the category being updated</param>
        /// <returns>Returns updated category</returns>
        Task<CategoryResponse> UpdateAsync(Guid userId, CategoryUpdateRequest categoryUpdateRequest);

        /// <summary>
        /// Deletes task category by provided Id if it belongs to the current user, throws if fails
        /// </summary>
        /// <param name="userId">Id of the current logged user</param>
        /// <param name="id">Id of the category to be deleted</param>
        Task DeleteAsync(Guid userId, Guid id);
    }
}
