using AutoMapper;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CategoriesService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoriesService> logger) : ICategoriesService
    {
        private readonly IUnitOfWork unitOfWork = unitOfWork;
        private readonly IMapper mapper = mapper;
        private readonly ILogger<CategoriesService> logger = logger;

        public async Task<CategoryResponse> Add(Guid userId, CategoryAddRequest categoryAddRequest)
        {
            logger.LogInformation("Attempting to add category {CategoryName} for user {UserId}.", categoryAddRequest.Name, userId);

            if (string.IsNullOrWhiteSpace(categoryAddRequest.Name))
            {
                logger.LogWarning("Category add failed: Name is null or whitespace for user {UserId}.", userId);
                throw new ArgumentException("Invalid name.");
            }

            var existingCategory = await unitOfWork.Categories.GetByNameAsync(userId, categoryAddRequest.Name);

            if (existingCategory != null)
            {
                logger.LogWarning("Category add failed: Name {CategoryName} already exists for user {UserId}.", categoryAddRequest.Name, userId);
                throw new ArgumentException("Category with given name already exists.");
            }

            var categoryEntity = mapper.Map<CategoryEntity>(categoryAddRequest);
            categoryEntity.Id = Guid.NewGuid();
            categoryEntity.UserId = userId;

            unitOfWork.Categories.Add(categoryEntity);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Category {CategoryId} successfully added for user {UserId}.", categoryEntity.Id, userId);

            return mapper.Map<CategoryResponse>(categoryEntity);
        }

        public async Task Delete(Guid userId, Guid id)
        {
            logger.LogInformation("Attempting to delete category {CategoryId} for user {UserId}.", id, userId);

            var entity = await CheckIfCategoryExists(userId, id);

            unitOfWork.Categories.Delete(entity);

            logger.LogInformation("Category {CategoryId} successfully deleted for user {UserId}.", id, userId);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<List<CategoryResponse>> GetAll(Guid userId)
        {
            logger.LogInformation("Retrieving all categories for user {UserId}.", userId);

            var resultEntity = await unitOfWork.Categories.GetAllAsync(userId);
            var result = mapper.Map<List<CategoryResponse>>(resultEntity);

            return result;
        }

        public async Task<CategoryResponse?> GetById(Guid userId, Guid id)
        {
            logger.LogInformation("Attempting to retrieve category {CategoryId} for user {UserId}.", id, userId);

            var resultEntity = await CheckIfCategoryExists(userId, id);
            var result = mapper.Map<CategoryResponse>(resultEntity);

            logger.LogInformation("Category {CategoryId} successfully retrieved for user {UserId}.", id, userId);

            return result;
        }

        public async Task<CategoryResponse> Update(Guid userId, CategoryUpdateRequest categoryUpdateRequest)
        {
            logger.LogInformation("Attempting to update category {CategoryId} for user {UserId}.", categoryUpdateRequest.Id, userId);

            var existingEntity = await CheckIfCategoryExists(userId, categoryUpdateRequest.Id);

            if (string.IsNullOrWhiteSpace(categoryUpdateRequest.Name))
            {
                logger.LogWarning("Category update failed: Name is null or whitespace for user {UserId}.", userId);
                throw new ArgumentException("Invalid name.");
            }

            if (existingEntity.Name != categoryUpdateRequest.Name)
            {
                var duplicate = await unitOfWork.Categories.GetByNameAsync(userId, categoryUpdateRequest.Name);
                
                if (duplicate != null)
                {
                    logger.LogWarning("Category update failed: Name {CategoryName} already exists for user {UserId}.", categoryUpdateRequest.Name, userId);
                    throw new ArgumentException("Category with provided name already exists.");
                }
            }

            mapper.Map(categoryUpdateRequest, existingEntity);

            unitOfWork.Categories.Update(existingEntity);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Category {CategoryId} successfully updated for user {UserId}.", categoryUpdateRequest.Id, userId);

            return mapper.Map<CategoryResponse>(existingEntity);
        }

        private async Task<CategoryEntity> CheckIfCategoryExists(Guid userId, Guid id)
        {
            var resultEntity = await unitOfWork.Categories.GetByIdAsync(id);
            if (resultEntity == null || userId != resultEntity.UserId)
            {
                logger.LogWarning("Access denied or Not Found: Category with Id {CategoryId} doesn't exist for user with Id {UserId}", id, userId);
                throw new KeyNotFoundException("Category not found.");
            }
            return resultEntity;
        }
    }
}
