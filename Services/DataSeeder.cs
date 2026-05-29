using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;

namespace Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            await context.Database.MigrateAsync();

            if (await context.Users.AnyAsync())
            {
                return;
            }

            var testUserId = Guid.NewGuid();
            var testUser = new UserEntity
            {
                Id = testUserId,
                Email = "test@example.com",
                PasswordHash = passwordService.Generate("password123")
            };

            await context.Users.AddAsync(testUser);

            var workCategoryId = Guid.NewGuid();
            var personalCategoryId = Guid.NewGuid();

            var categories = new List<CategoryEntity>
            {
                new() { Id = workCategoryId, UserId = testUserId, Name = "Work" },
                new() { Id = personalCategoryId, UserId = testUserId, Name = "Personal" }
            };

            await context.Categories.AddRangeAsync(categories);

            var tasks = new List<TaskEntity>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    CategoryId = workCategoryId,
                    Name = "Learn EF Core",
                    Description = "Review documentation about Seeding Data.",
                    IsCompleted = true,
                    DateCreated = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    CategoryId = workCategoryId,
                    Name = "Update Resume",
                    Description = "Add the To-Do API project to GitHub.",
                    IsCompleted = false,
                    DateCreated = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = testUserId,
                    CategoryId = personalCategoryId,
                    Name = "Buy groceries",
                    Description = "Milk, bread, and apples.",
                    IsCompleted = false,
                    DateCreated = DateTime.UtcNow
                }
            };

            await context.Tasks.AddRangeAsync(tasks);

            await context.SaveChangesAsync();
        }
    }
}
