using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ToDoApp.Extensions;

namespace ToDoApp.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class TasksController(ITasksService tasksService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TasksPaginatedResponse>> GetAll([FromQuery] TasksPaginationParameters parameters)
        {
            var userId = User.GetUserId();

            var result = await tasksService.GetPaginatedAsync(userId, parameters);

            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaskResponse>> Create(TaskAddRequest request)
        {
            var userId = User.GetUserId();

            var result = await tasksService.AddAsync(userId, request);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> GetById(Guid id)
        {
            var userId = User.GetUserId();

            var result = await tasksService.GetByIdAsync(userId, id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = User.GetUserId();

            await tasksService.DeleteAsync(userId, id);

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskResponse>> Update(Guid id, TaskUpdateRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("Id in the URL does not match the id in the request body.");
            }

            var userId = User.GetUserId();

            var result = await tasksService.UpdateAsync(userId, request);

            return Ok(result);
        }
    }
}
