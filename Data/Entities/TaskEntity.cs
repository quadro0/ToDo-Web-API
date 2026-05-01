using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class TaskEntity : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public bool IsCompleted { get; set; }
        [Required]
        public DateTime DateCreated { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public UserEntity? User { get; set; }
        public CategoryEntity? Category { get; set; }
    }
}
