using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ToDoApp.Extensions;

namespace ToDoApp.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class CategoriesController(ICategoriesService categoriesService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
        {
            var userId = User.GetUserId();

            var categories = await categoriesService.GetAllAsync(userId);

            return Ok(categories);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryResponse>> Create(CategoryAddRequest request)
        {
            var userId = User.GetUserId();

            var result = await categoriesService.AddAsync(userId, request);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryResponse>> GetById(Guid id)
        {
            var userId = User.GetUserId();

            var result = await categoriesService.GetByIdAsync(userId, id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteById(Guid id)
        {
            var userId = User.GetUserId();

            await categoriesService.DeleteAsync(userId, id);

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryResponse>> Update(Guid id, CategoryUpdateRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("Id in the URL does not match the id in the request body.");
            }

            var userId = User.GetUserId();

            var result = await categoriesService.UpdateAsync(userId, request);

            return Ok(result);
        }
    } 
}
