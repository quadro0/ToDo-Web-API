using Data.Entities;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class CategoriesRepository(TodoDbContext context) : ICategoriesRepository
    {
        private readonly TodoDbContext context = context;

        public async Task<CategoryEntity?> GetByIdAsync(Guid id)
        {
            return await context.Categories.AsNoTracking().Include(c => c.Tasks).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<CategoryEntity>> GetAllAsync()
        {
            return await context.Categories.AsNoTracking().Include(c => c.Tasks).ToListAsync();
        }

        public void Add(CategoryEntity category)
        {
            context.Categories.Add(category);
        }

        public void Delete(CategoryEntity category)
        {
            context.Categories.Remove(category);
        }

        public void Update(CategoryEntity category)
        {
            context.Categories.Update(category);
        }
    }
}
