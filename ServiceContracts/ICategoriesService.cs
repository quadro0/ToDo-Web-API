using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface ICategoriesService
    {
        Task<CategoryResponse> AddAsync(Guid userId, CategoryAddRequest categoryAddRequest);
        Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid id);
        Task<List<CategoryResponse>> GetAllAsync(Guid userId);
        Task<CategoryResponse> UpdateAsync(Guid userId, CategoryUpdateRequest categoryUpdateRequest);
        Task DeleteAsync(Guid userId, Guid id);
    }
}
