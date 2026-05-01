using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class TasksRepository(TodoDbContext context) : ITasksRepository
    {
        private readonly TodoDbContext context = context;

        public async Task<TaskEntity?> GetByIdAsync(Guid id)
        {
            return await context.Tasks.AsNoTracking().Include(t => t.User).Include(t => t.Category).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<TaskEntity>> GetAllAsync()
        {
            return await context.Tasks.AsNoTracking().Include(t => t.User).Include(t => t.Category).ToListAsync();
        }

        public async Task<IEnumerable<TaskEntity>> GetAllAsync(int pageNumber, int count)
        {
            var skipCount = Math.Max(0, (pageNumber - 1) * count);

            return await context.Tasks.AsNoTracking().Include(t => t.User).Include(t => t.Category).Skip(skipCount).Take(count).ToListAsync();
        }

        public void Add(TaskEntity task)
        {
            context.Tasks.Add(task);
        }

        public void Delete(TaskEntity task)
        {
            context.Tasks.Remove(task);
        }

        public void Update(TaskEntity task)
        {
            context.Tasks.Update(task);
        }
    }
}
