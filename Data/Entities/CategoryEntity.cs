using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class CategoryEntity : BaseEntity
    {
        [Required]
        [MaxLength(30)]
        public string? Name { get; set; }
        public List<TaskEntity>? Tasks { get; set; }
    }
}
