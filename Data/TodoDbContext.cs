using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<TaskEntity> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserEntity>().ToTable("Users");
            modelBuilder.Entity<CategoryEntity>().ToTable("Categories");
            modelBuilder.Entity<TaskEntity>().ToTable("Tasks");

            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<TaskEntity>()
                .HasOne(t => t.Category)
                .WithMany(u => u.Tasks)
                .HasForeignKey(p => p.CategoryId);
        }
    }
}
