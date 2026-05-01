namespace Data.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IUsersRepository Users { get; }
        ICategoriesRepository Categories { get; }
        ITasksRepository Tasks { get; }
        Task SaveChangesAsync();
    }
}
