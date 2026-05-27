using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }
    }
}
