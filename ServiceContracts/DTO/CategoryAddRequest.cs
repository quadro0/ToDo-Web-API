using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    public class CategoryAddRequest
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 30 characters.")]
        public string? Name { get; set; }
    }
}
