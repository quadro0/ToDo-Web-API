using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    public class UserUpdateRequest
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 50 characters.")]
        public string? CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "New password must be between 5 and 50 characters.")]
        public string? NewPassword { get; set; }
    }
}
