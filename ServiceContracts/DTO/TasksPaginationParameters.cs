using System.ComponentModel.DataAnnotations;

namespace ServiceContracts.DTO
{
    public class TasksPaginationParameters
    {
        public string? SearchName { get; set; }
        public Guid? CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;

        [Range(1, 50)]
        public int PageSize { get; set; } = 10;
    }
}
