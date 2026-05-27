using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CategoriesService(TodoDbContext context, IMapper mapper, ILogger<CategoriesService> logger) : ICategoriesService
    {
        public async Task<CategoryResponse> AddAsync(Guid userId, CategoryAddRequest categoryAddRequest)
        {
            if (await context.Categories.AnyAsync(c => c.UserId == userId && c.Name == categoryAddRequest.Name))
            {
                logger.LogWarning("Category add failed: Name {CategoryName} already exists for user {UserId}.", categoryAddRequest.Name, userId);
                throw new ArgumentException("Category with given name already exists.");
            }

            var categoryEntity = mapper.Map<CategoryEntity>(categoryAddRequest);
            categoryEntity.Id = Guid.NewGuid();
            categoryEntity.UserId = userId;

            context.Categories.Add(categoryEntity);
            await context.SaveChangesAsync();

            return mapper.Map<CategoryResponse>(categoryEntity);
        }

        public async Task DeleteAsync(Guid userId, Guid id)
        {
            var entity = await context.Categories.FindAsync(id);
            if (entity == null || userId != entity.UserId)
            {
                logger.LogWarning("Access denied or Not Found: Category with Id {CategoryId} doesn't exist for user with Id {UserId}", id, userId);
                throw new KeyNotFoundException("Category not found.");
            }

            context.Categories.Remove(entity);

            await context.SaveChangesAsync();
        }

        public async Task<List<CategoryResponse>> GetAllAsync(Guid userId)
        {
            return await context.Categories.AsNoTracking().Where(c => c.UserId == userId).ProjectTo<CategoryResponse>(mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid id)
        {
            var result = await context.Categories.AsNoTracking().Where(c => c.UserId == userId && c.Id == id).ProjectTo<CategoryResponse>(mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null)
            {
                logger.LogWarning("Access denied or Not Found: Category with Id {CategoryId} doesn't exist for user with Id {UserId}", id, userId);
                throw new KeyNotFoundException("Category not found.");
            }

            return result;
        }

        public async Task<CategoryResponse> UpdateAsync(Guid userId, CategoryUpdateRequest categoryUpdateRequest)
        {
            var existingEntity = await context.Categories.FindAsync(categoryUpdateRequest.Id);
            if (existingEntity == null || userId != existingEntity.UserId)
            {
                logger.LogWarning("Access denied or Not Found: Category with Id {CategoryId} doesn't exist for user with Id {UserId}", categoryUpdateRequest.Id, userId);
                throw new KeyNotFoundException("Category not found.");
            }

            if (existingEntity.Name != categoryUpdateRequest.Name && await context.Categories.AnyAsync(c => c.UserId == userId && c.Name == categoryUpdateRequest.Name))
            {
                logger.LogWarning("Category update failed: Name {CategoryName} already exists for user {UserId}.", categoryUpdateRequest.Name, userId);
                throw new ArgumentException("Category with provided name already exists.");
            }

            mapper.Map(categoryUpdateRequest, existingEntity);

            await context.SaveChangesAsync();

            return mapper.Map<CategoryResponse>(existingEntity);
        }
    }
}
