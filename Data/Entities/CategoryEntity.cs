using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class CategoryEntity : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string? Name { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserEntity? User { get; set; }
        public List<TaskEntity> Tasks { get; set; } = [];
    }
}
