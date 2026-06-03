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
    public sealed class CategoriesServiceTests : IDisposable
    {
        private readonly TodoDbContext context;
        private readonly Mock<ILogger<CategoriesService>> logger;
        private readonly IMapper mapper;
        private readonly CategoriesService categoriesService;

        public CategoriesServiceTests()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            context = new TodoDbContext(options);
            logger = new Mock<ILogger<CategoriesService>>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutomapperProfile>(), new LoggerFactory());
            mapper = config.CreateMapper();

            categoriesService = new CategoriesService(context, mapper, logger.Object);
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
            GC.SuppressFinalize(this);
        }

        #region AddAsync
        [Fact]
        public async Task AddAsync_ThrowsIfCategoryNameAlreadyExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingCategory = new CategoryEntity()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Work"
            };
            context.Categories.Add(existingCategory);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var categoryAddRequest = new CategoryAddRequest()
            {
                Name = "Work"
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await categoriesService.AddAsync(userId, categoryAddRequest);
            });
        }

        [Theory]
        [InlineData("Work")]
        [InlineData("Personal")]
        [InlineData("Health and Sport")]
        public async Task AddAsync_AddsCategoryAndReturnsResponseIfValidData(string name)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryAddRequest = new CategoryAddRequest()
            {
                Name = name
            };

            // Action
            var categoryResponse = await categoriesService.AddAsync(userId, categoryAddRequest);

            // Assert
            Assert.NotNull(categoryResponse);
            Assert.Equal(name, categoryResponse.Name);
            Assert.NotEqual(Guid.Empty, categoryResponse.Id);

            var dbCategory = await context.Categories.FindAsync([categoryResponse.Id], TestContext.Current.CancellationToken);
            Assert.NotNull(dbCategory);
            Assert.Equal(name, dbCategory.Name);
            Assert.Equal(userId, dbCategory.UserId);
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_ThrowsIfInvalidCategoryId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidCategoryId = Guid.NewGuid();

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.DeleteAsync(userId, invalidCategoryId);
            });
        }

        [Fact]
        public async Task DeleteAsync_ThrowsIfCategoryBelongsToAnotherUser()
        {
            // Arrange
            var authorizedUserId = Guid.NewGuid();
            var wrongUserId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();

            var category = new CategoryEntity() { Id = categoryId, UserId = wrongUserId, Name = "Private Tasks" };
            context.Categories.Add(category);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.DeleteAsync(authorizedUserId, categoryId);
            });
        }

        [Fact]
        public async Task DeleteAsync_DeletesWithValidId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var category = new CategoryEntity() { Id = categoryId, UserId = userId, Name = "To Delete" };

            context.Categories.Add(category);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Action
            await categoriesService.DeleteAsync(userId, categoryId);

            // Assert
            var dbCategory = await context.Categories.FindAsync([categoryId], TestContext.Current.CancellationToken);
            Assert.Null(dbCategory);
        }
        #endregion

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_ReturnsListOfCategories_IfCategoriesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();

            var categories = new List<CategoryEntity>()
            {
                new() { Id = Guid.NewGuid(), UserId = userId, Name = "Work" },
                new() { Id = Guid.NewGuid(), UserId = userId, Name = "Personal" },
                new() { Id = Guid.NewGuid(), UserId = anotherUserId, Name = "Stranger Things" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Action
            var result = await categoriesService.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Work");
            Assert.Contains(result, c => c.Name == "Personal");
            Assert.DoesNotContain(result, c => c.Name == "Stranger Things");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_IfNoCategoriesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Action
            var result = await categoriesService.GetAllAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_ThrowsIfCategoryNotFoundOrWrongUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var invalidCategoryId = Guid.NewGuid();

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.GetByIdAsync(userId, invalidCategoryId);
            });
        }

        [Theory]
        [InlineData("Study")]
        [InlineData("Home")]
        public async Task GetByIdAsync_ReturnsCategoryWithValidId(string name)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var category = new CategoryEntity() { Id = categoryId, UserId = userId, Name = name };

            context.Categories.Add(category);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Action
            var result = await categoriesService.GetByIdAsync(userId, categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryId, result.Id);
            Assert.Equal(name, result.Name);
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_ThrowsIfNonExistingId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = Guid.NewGuid(),
                Name = "New Name"
            };

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.UpdateAsync(userId, categoryUpdateRequest);
            });
        }

        [Fact]
        public async Task UpdateAsync_ThrowsIfNameIsWithDuplicatedNameForThisUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var category1Id = Guid.NewGuid();
            var category2Id = Guid.NewGuid();

            var category1 = new CategoryEntity() { Id = category1Id, UserId = userId, Name = "Existing Work" };
            var category2 = new CategoryEntity() { Id = category2Id, UserId = userId, Name = "Existing Home" };

            context.Categories.AddRange(category1, category2);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = category2Id,
                Name = "Existing Work"
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await categoriesService.UpdateAsync(userId, categoryUpdateRequest);
            });
        }

        [Theory]
        [InlineData("Brand New Name")]
        public async Task UpdateAsync_UpdatesWithValidData(string newName)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var existingEntity = new CategoryEntity() { Id = categoryId, UserId = userId, Name = "Old Name" };

            context.Categories.Add(existingEntity);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = categoryId,
                Name = newName
            };

            // Action
            var result = await categoriesService.UpdateAsync(userId, categoryUpdateRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newName, result.Name);
            Assert.Equal(categoryId, result.Id);

            var dbCategory = await context.Categories.FindAsync([categoryId], TestContext.Current.CancellationToken);
            Assert.NotNull(dbCategory);
            Assert.Equal(newName, dbCategory.Name);
        }
        #endregion
    }
}