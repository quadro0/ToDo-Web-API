using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class UserEntity : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string? Email { get; set; }
        [Required]
        [MaxLength(100)]
        public string? PasswordHash { get; set; }
        public List<TaskEntity>? Tasks { get; set; }
    }
}
