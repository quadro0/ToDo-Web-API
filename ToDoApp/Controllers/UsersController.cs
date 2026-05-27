using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ToDoApp.Extensions;

namespace ToDoApp.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController(IUsersService usersService) : ControllerBase
    {
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(UserAddRequest request)
        {
            await usersService.RegisterAsync(request);

            return Ok();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult> Login(UserLoginRequest request)
        {
            var token = await usersService.LoginAsync(request);

            return Ok(new { Token = token });
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword(UserUpdateRequest resuest)
        {
            var userId = User.GetUserId();

            await usersService.UpdatePasswordAsync(userId, resuest);

            return Ok();
        }
    }
}
