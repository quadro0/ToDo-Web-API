using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    public class TaskAddRequest
    {
        [Required(ErrorMessage = "Task name is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 20 characters.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Task description is required.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 100 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Task category is required.")]
        public Guid CategoryId { get; set; }
    }
}
