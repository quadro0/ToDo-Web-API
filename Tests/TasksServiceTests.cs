using AutoMapper;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts.DTO;
using Services;

namespace Tests
{
    public sealed class TasksServiceTests : IDisposable
    {
        private readonly TodoDbContext context;
        private readonly Mock<ILogger<TasksService>> logger;
        private readonly IMapper mapper;
        private readonly TasksService tasksService;

        public TasksServiceTests()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = new TodoDbContext(options);
            logger = new Mock<ILogger<TasksService>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutomapperProfile>(), new LoggerFactory());
            mapper = config.CreateMapper();

            tasksService = new TasksService(context, mapper, logger.Object);
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
            GC.SuppressFinalize(this);
        }

        #region AddAsync
        [Theory]
        [InlineData("Task 1", "Description for task 1")]
        [InlineData("Task 2", "Description for task 2")]
        public async Task AddAsync_AddsTaskAndReturnsResponseIfValidData(string name, string description)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var taskAddRequest = new TaskAddRequest()
            {
                Name = name,
                Description = description,
                CategoryId = categoryId
            };

            // Action
            var taskResponse = await tasksService.AddAsync(userId, taskAddRequest);

            // Assert
            Assert.NotNull(taskResponse);
            Assert.Equal(name, taskResponse.Name);
            Assert.Equal(description, taskResponse.Description);
            Assert.Equal(categoryId, taskResponse.CategoryId);
            Assert.Equal(userId, taskResponse.UserId);
            Assert.NotEqual(Guid.Empty, taskResponse.Id);

            var dbTask = await context.Tasks.FindAsync([taskResponse.Id], TestContext.Current.CancellationToken);
            Assert.NotNull(dbTask);
            Assert.Equal(name, dbTask.Name);
            Assert.Equal(userId, dbTask.UserId);
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_ThrowsIfInvalidTaskId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidTaskId = Guid.NewGuid();

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await tasksService.DeleteAsync(userId, invalidTaskId);
            });
        }

        [Fact]
        public async Task DeleteAsync_ThrowsIfTaskBelongsToAnotherUser()
        {
            // Arrange
            var authorizedUserId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var task = new TaskEntity()
            {
                Id = taskId,
                UserId = wrongUserId,
                Name = "Secret Task",
                Description = "Some description"
            };
            context.Tasks.Add(task);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await tasksService.DeleteAsync(authorizedUserId, taskId);
            });
        }

        [Fact]
        public async Task DeleteAsync_DeletesWithValidId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var task = new TaskEntity()
            {
                Id = taskId,
                UserId = userId,
                Name = "To Delete",
                Description = "Some description"
            };

            context.Tasks.Add(task);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Action
            await tasksService.DeleteAsync(userId, taskId);

            // Assert
            var dbTask = await context.Tasks.FindAsync([taskId, TestContext.Current.CancellationToken], TestContext.Current.CancellationToken);
            Assert.Null(dbTask);
        }
        #endregion

        #region GetByIdAsync
        [Theory]
        [InlineData("Fix bugs", "Fix all critical issues")]
        public async Task GetByIdAsync_ReturnsTaskWithValidId(string name, string description)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var category = new CategoryEntity { Id = categoryId, UserId = userId, Name = "Tech Category" };
            var task = new TaskEntity()
            {
                Id = taskId,
                UserId = userId,
                CategoryId = categoryId,
                Category = category,
                Name = name,
                Description = description
            };

            context.Categories.Add(category);
            context.Tasks.Add(task);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Action
            var result = await tasksService.GetByIdAsync(userId, taskId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskId, result.Id);
            Assert.Equal(name, result.Name);
            Assert.Equal(description, result.Description);
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsIfTaskNotFoundOrWrongUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidTaskId = Guid.NewGuid();

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await tasksService.GetByIdAsync(userId, invalidTaskId);
            });
        }
        #endregion

        #region GetPaginatedAsync
        [Fact]
        public async Task GetPaginatedAsync_ReturnsAllTasksForUser_IfNoFiltersProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var category = new CategoryEntity { Id = categoryId, UserId = userId, Name = "General" };
            context.Categories.Add(category);

            var tasks = new List<TaskEntity>()
            {
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId, Category = category, Name = "Task 1", Description = "Desc 1" },
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId, Category = category, Name = "Task 2", Description = "Desc 2" },
                new() { Id = Guid.NewGuid(), UserId = anotherUserId, CategoryId = categoryId, Category = category, Name = "Task 3", Description = "Desc 3" }
            };

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var parameters = new TasksPaginationParameters();

            // Action
            var result = await tasksService.GetPaginatedAsync(userId, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
        }

        [Theory]
        [InlineData("Learn", 2)]
        [InlineData("EF Core", 1)]
        [InlineData("NonExisting", 0)]
        public async Task GetPaginatedAsync_FiltersByName_IfSearchNameIsProvided(string searchName, int expectedCount)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var category = new CategoryEntity { Id = categoryId, UserId = userId, Name = "Education" };
            context.Categories.Add(category);

            var tasks = new List<TaskEntity>()
            {
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId, Category = category, Name = "Learn C#", Description = "Desc" },
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId, Category = category, Name = "Learn EF Core", Description = "Desc" },
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId, Category = category, Name = "Buy Milk", Description = "Desc" }
            };

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var parameters = new TasksPaginationParameters { SearchName = searchName };

            // Action
            var result = await tasksService.GetPaginatedAsync(userId, parameters);

            // Assert
            Assert.Equal(expectedCount, result.TotalCount);
        }

        [Fact]
        public async Task GetPaginatedAsync_FiltersByCategory_IfCategoryIdIsProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var targetCategoryId = Guid.NewGuid();
            var otherCategoryId = Guid.NewGuid();

            var targetCategory = new CategoryEntity { Id = targetCategoryId, UserId = userId, Name = "Target" };
            var otherCategory = new CategoryEntity { Id = otherCategoryId, UserId = userId, Name = "Other" };
            context.Categories.AddRange(targetCategory, otherCategory);

            var tasks = new List<TaskEntity>()
            {
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = targetCategoryId, Category = targetCategory, Name = "Task 1", Description = "Desc" },
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = targetCategoryId, Category = targetCategory, Name = "Task 2", Description = "Desc" },
                new() { Id = Guid.NewGuid(), UserId = userId, CategoryId = otherCategoryId, Category = otherCategory, Name = "Task 3", Description = "Desc" }
            };

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var parameters = new TasksPaginationParameters { CategoryId = targetCategoryId };

            // Action
            var result = await tasksService.GetPaginatedAsync(userId, parameters);

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, t => Assert.Equal(targetCategoryId, t.CategoryId));
        }

        [Fact]
        public async Task GetPaginatedAsync_AppliesPagination_ReturnsCorrectPageAndSize()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var category = new CategoryEntity { Id = categoryId, UserId = userId, Name = "Pagination Category" };
            context.Categories.Add(category);

            var tasks = Enumerable.Range(1, 15).Select(i => new TaskEntity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = categoryId,
                Category = category,
                Name = $"Task {i}",
                Description = "Desc"
            }).ToList();

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var parameters = new TasksPaginationParameters { PageNumber = 2, PageSize = 5 };

            // Action
            var result = await tasksService.GetPaginatedAsync(userId, parameters);

            // Assert
            Assert.Equal(15, result.TotalCount);
            Assert.Equal(5, result.Items.Count());
            Assert.Equal(2, result.PageNumber);
            Assert.Equal(3, result.TotalPages);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_ThrowsIfNonExistingId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskUpdateRequest = new TaskUpdateRequest()
            {
                Id = Guid.NewGuid(),
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await tasksService.UpdateAsync(userId, taskUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdateAsync_ThrowsIfTaskBelongsToAnotherUser()
        {
            // Arrange
            var authorizedUserId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var existingTask = new TaskEntity() { Id = taskId, UserId = wrongUserId, Name = "Old Name", Description = "Old Desc" };
            context.Tasks.Add(existingTask);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var taskUpdateRequest = new TaskUpdateRequest()
            {
                Id = taskId,
                Name = "Updated Name",
                Description = "Updated Description"
            };

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await tasksService.UpdateAsync(authorizedUserId, taskUpdateRequest);
            });
        }

        [Theory]
        [InlineData("Brand New Task Name", "Brand New Description", true)]
        public async Task UpdateAsync_UpdatesWithValidData(string newName, string newDescription, bool isCompleted)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var existingEntity = new TaskEntity() { Id = taskId, UserId = userId, Name = "Old Name", Description = "Old Desc", IsCompleted = false };

            context.Tasks.Add(existingEntity);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var taskUpdateRequest = new TaskUpdateRequest()
            {
                Id = taskId,
                Name = newName,
                Description = newDescription,
                IsCompleted = isCompleted,
                CategoryId = categoryId
            };

            // Action
            var result = await tasksService.UpdateAsync(userId, taskUpdateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newName, result.Name);
            Assert.Equal(newDescription, result.Description);
            Assert.Equal(isCompleted, result.IsCompleted);
            Assert.Equal(categoryId, result.CategoryId);
            Assert.Equal(taskId, result.Id);

            var dbTask = await context.Tasks.FindAsync([taskId], TestContext.Current.CancellationToken);
            Assert.NotNull(dbTask);
            Assert.Equal(newName, dbTask.Name);
            Assert.Equal(newDescription, dbTask.Description);
            Assert.Equal(isCompleted, dbTask.IsCompleted);
        }
        #endregion
    }
}