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
    public class TasksService(TodoDbContext context, IMapper mapper, ILogger<TasksService> logger) : ITasksService
    {
        public async Task<TaskResponse> AddAsync(Guid userId, TaskAddRequest taskAddRequest)
        {
            var TaskEntity = mapper.Map<TaskEntity>(taskAddRequest);
            TaskEntity.Id = Guid.NewGuid();
            TaskEntity.UserId = userId;

            context.Tasks.Add(TaskEntity);
            await context.SaveChangesAsync();

            return mapper.Map<TaskResponse>(TaskEntity);
        }

        public async Task DeleteAsync(Guid userId, Guid id)
        {
            var entity = await context.Tasks.FindAsync(id);
            if (entity == null || userId != entity.UserId)
            {
                logger.LogWarning("Access denied or Not Found: Task with Id {TaskId} doesn't exist for user with Id {UserId}", id, userId);
                throw new KeyNotFoundException("Task not found.");
            }

            context.Tasks.Remove(entity);

            await context.SaveChangesAsync();
        }

        public async Task<TaskResponse?> GetByIdAsync(Guid userId, Guid id)
        {
            var result = await context.Tasks.AsNoTracking().Where(t => t.UserId == userId && t.Id == id).ProjectTo<TaskResponse>(mapper.ConfigurationProvider).FirstOrDefaultAsync();
            if (result == null)
            {
                logger.LogWarning("Access denied or Not Found: Task with Id {TaskId} doesn't exist for user with Id {UserId}", id, userId);
                throw new KeyNotFoundException("Task not found.");
            }

            return result;
        }

        public async Task<TasksPaginatedResponse> GetPaginatedAsync(Guid userId, TasksPaginationParameters parameters)
        {
            var result = context.Tasks.AsNoTracking().Where(t => t.UserId == userId);

            if (!string.IsNullOrWhiteSpace(parameters.SearchName))
            {
                result = result.Where(t => t.Name!.Contains(parameters.SearchName));
            }

            if (parameters.CategoryId != null)
            {
                result = result.Where(t => t.CategoryId == parameters.CategoryId);
            }

            var totalCount = await result.CountAsync();

            var skipCount = (parameters.PageNumber - 1) * parameters.PageSize;

            var items = await result.Skip(Math.Max(0, skipCount)).Take(parameters.PageSize).ProjectTo<TaskResponse>(mapper.ConfigurationProvider).ToListAsync();

            return new TasksPaginatedResponse()
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public async Task<TaskResponse> UpdateAsync(Guid userId, TaskUpdateRequest taskUpdateRequest)
        {
            var existingEntity = await context.Tasks.FindAsync(taskUpdateRequest.Id);
            if (existingEntity == null || userId != existingEntity.UserId)
            {
                logger.LogWarning("Access denied or Not Found: Task with Id {TaskId} doesn't exist for user with Id {UserId}", taskUpdateRequest.Id, userId);
                throw new KeyNotFoundException("Task not found.");
            }

            mapper.Map(taskUpdateRequest, existingEntity);

            await context.SaveChangesAsync();

            return mapper.Map<TaskResponse>(existingEntity);
        }
    }
}
