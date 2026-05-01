using Data.Repositories.Interfaces;

namespace Data.Repositories
{
    public class UnitOfWork(TodoDbContext context, IUsersRepository users, ICategoriesRepository categories, ITasksRepository tasks) : IUnitOfWork
    {
        private readonly TodoDbContext context = context;
        public IUsersRepository Users { get; } = users;
        public ICategoriesRepository Categories { get; } = categories;
        public ITasksRepository Tasks { get; } = tasks;

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
