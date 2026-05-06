using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;

namespace ToDoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController(ICategoriesService categoriesService) : ControllerBase
    {
        private readonly ICategoriesService categoriesService = categoriesService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllCategories()
        {
            //var categories = await categoriesService.GetAll();

            return this.Ok();//categories);
        }
    } 
}
