using System.Security.Claims;

namespace ToDoApp.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var userIdString = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing User ID in token.");
            }

            return userId;
        }
    }
}
