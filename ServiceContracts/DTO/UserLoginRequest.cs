using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    public class UserLoginRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email has to be valid.")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Email must be between 10 and 50 characters.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Password must be between 5 and 50 characters.")]
        public string? Password { get; set; }
    }
}
