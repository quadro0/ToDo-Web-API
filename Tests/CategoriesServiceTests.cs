using AutoMapper;
using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace Tests
{
    public class CategoriesServiceTests
    {
        private readonly Mock<IUnitOfWork> repo;
        private readonly IMapper mapper;
        private readonly ICategoriesService categoriesService;
        public CategoriesServiceTests()
        {
            repo = new Mock<IUnitOfWork>();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<AutomapperProfile>(), new LoggerFactory());
            mapper = config.CreateMapper();

            categoriesService = new CategoriesService(repo.Object, mapper);
        }

        #region Add
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Add_ThrowsIfNullOrEmptyName(string? name)
        {
            // Arrange
            var categoryAddRequest = new CategoryAddRequest()
            {
                Name = name
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await categoriesService.Add(Guid.NewGuid(), categoryAddRequest);
            });

            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Add_ThrowsIfDuplicatedName()
        {
            // Arrange
            repo.Setup(r => r.Categories.GetByNameAsync(It.IsAny<Guid>(), "School")).ReturnsAsync(new CategoryEntity());
            var categoryAddRequest = new CategoryAddRequest()
            {
                Name = "School"
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await categoriesService.Add(Guid.NewGuid(), categoryAddRequest);
            });

            repo.Verify(r => r.Categories.GetByNameAsync(It.IsAny<Guid>(), "School"), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("Home")]
        [InlineData("Garden")]
        [InlineData("Work")]
        public async Task Add_AddsCategoryAndReturnsResponseIfValidName(string name)
        {
            // Arrange
            repo.Setup(r => r.Categories.GetByNameAsync(It.IsAny<Guid>(), It.IsAny<string>()));
            var categoryAddRequest = new CategoryAddRequest()
            {
                Name = name
            };

            var userId = Guid.NewGuid();

            // Action
            var categoryResponse = await categoriesService.Add(userId, categoryAddRequest);

            // Assert
            Assert.Equal(categoryResponse.Name, name);

            repo.Verify(r => r.Categories.GetByNameAsync(userId, name), Times.Once);
            repo.Verify(r => r.Categories.Add(It.Is<CategoryEntity>(e =>
                e.Name == name &&
                e.UserId == userId &&
                e.Id != Guid.Empty)), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(), Times.Once);
            repo.VerifyNoOtherCalls();
        }
        #endregion

        #region Delete
        [Fact]
        public async Task Delete_ThrowsIfInvalidCategoryId()
        {
            // Arrange
            var id = Guid.NewGuid();
            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(default(CategoryEntity));

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async() =>
            {
                // Action
                await categoriesService.Delete(Guid.NewGuid(), id);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Delete_ThrowsIfInvalidUserId()
        {
            // Arrange
            var id = Guid.NewGuid();

            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(new CategoryEntity(){
                Id = id,
                UserId = Guid.NewGuid()
            });

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.Delete(Guid.NewGuid(), id);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Delete_DeletesWithValidIdAndUserId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(new CategoryEntity()
            {
                Id = id,
                UserId = userId
            });

            // Action
            await categoriesService.Delete(userId, id);

            // Assert
            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.Verify(r => r.Categories.Delete(It.Is<CategoryEntity>(e =>
                e.Id == id)), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(), Times.Once);
            repo.VerifyNoOtherCalls();
        }
        #endregion

        #region GetAll
        [Fact]
        public async Task GetAll_ReturnsListOfCategories_IfCategoriesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categories = new List<CategoryEntity>
            {
                new CategoryEntity { Id = Guid.NewGuid(), Name = "Work", UserId = userId },
                new CategoryEntity { Id = Guid.NewGuid(), Name = "Home", UserId = userId }
            };

            repo.Setup(r => r.Categories.GetAllAsync(userId)).ReturnsAsync(categories);

            // Action
            var result = await categoriesService.GetAll(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categories.Count, result.Count);
            Assert.Equal(categories[0].Name, result[0].Name);
            Assert.Equal(categories[1].Name, result[1].Name);

            repo.Verify(r => r.Categories.GetAllAsync(userId), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAll_ReturnsEmptyList_IfNoCategoriesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            repo.Setup(r => r.Categories.GetAllAsync(userId)).ReturnsAsync(new List<CategoryEntity>());

            // Action
            var result = await categoriesService.GetAll(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            repo.Verify(r => r.Categories.GetAllAsync(userId), Times.Once);
            repo.VerifyNoOtherCalls();
        }
        #endregion

        #region GetById
        [Fact]
        public async Task GetById_ThrowsIfInvalidCategoryId()
        {
            // Arrange
            var id = Guid.NewGuid();
            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(default(CategoryEntity));

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.GetById(Guid.NewGuid(), id);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ThrowsIfInvalidUserId()
        {
            // Arrange
            var id = Guid.NewGuid();

            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(new CategoryEntity()
            {
                Id = id,
                UserId = Guid.NewGuid()
            });

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.GetById(Guid.NewGuid(), id);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("Home")]
        [InlineData("Garden")]
        [InlineData("Work")]
        public async Task GetById_ReturnsCategoryWithValidIdAndUserId(string name)
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();

            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(new CategoryEntity()
            {
                Id = id,
                UserId = userId,
                Name = name
            });

            // Action
            var result = await categoriesService.GetById(userId, id);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal(name, result.Name);

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }
        #endregion

        #region Update
        [Fact]
        public async Task Update_ThrowsIfNonExistingId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = id
            };

            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(default(CategoryEntity));

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.Update(Guid.NewGuid(), categoryUpdateRequest);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ThrowsIfInvalidUserId()
        {
            // Arrange
            var id = Guid.NewGuid();
            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = id
            };

            repo.Setup(r => r.Categories.GetByIdAsync(id)).ReturnsAsync(new CategoryEntity()
            {
                Id = id,
                UserId = Guid.NewGuid()
            });

            // Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            {
                // Action
                await categoriesService.Update(Guid.NewGuid(), categoryUpdateRequest);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Update_ThrowsIfNameIsNullOrEmpty(string? name)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = Guid.NewGuid(),
                Name = name,
                UserId = userId
            };

            repo.Setup(r => r.Categories.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new CategoryEntity()
            {
                UserId = userId
            });

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await categoriesService.Update(userId, categoryUpdateRequest);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ThrowsIfNameIsDuplicated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = Guid.NewGuid(),
                Name = "School",
                UserId = userId
            };

            repo.Setup(r => r.Categories.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new CategoryEntity()
            {
                UserId = userId
            });

            repo.Setup(r => r.Categories.GetByNameAsync(userId, "School")).ReturnsAsync(new CategoryEntity()
            {
                Name = "School",
                UserId = userId
            });

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                // Action
                await categoriesService.Update(userId, categoryUpdateRequest);
            });

            repo.Verify(r => r.Categories.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
            repo.Verify(r => r.Categories.GetByNameAsync(userId, "School"), Times.Once);
            repo.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData("Home")]
        [InlineData("Garden")]
        [InlineData("Work")]
        public async Task Update_UpdatesWithValidData(string name)
        {
            // Arrange
            var userId = Guid.NewGuid();
            var id = Guid.NewGuid();
            var categoryUpdateRequest = new CategoryUpdateRequest()
            {
                Id = id,
                Name = name,
                UserId = userId
            };

            repo.Setup(r => r.Categories.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new CategoryEntity()
            {
                Id = id,
                Name = "School",
                UserId = userId
            });

            // Action
            var result = await categoriesService.Update(userId, categoryUpdateRequest);

            // Assert
            Assert.Equal(categoryUpdateRequest.Name, result.Name);

            repo.Verify(r => r.Categories.GetByIdAsync(id), Times.Once);
            repo.Verify(r => r.Categories.GetByNameAsync(userId, name), Times.Once);
            repo.Verify(r => r.Categories.Update(It.Is<CategoryEntity>(e =>
                e.Name == name &&
                e.UserId == userId &&
                e.Id != Guid.Empty)), Times.Once);
            repo.Verify(r => r.SaveChangesAsync(), Times.Once);
            repo.VerifyNoOtherCalls();
        }
        #endregion
    }
}