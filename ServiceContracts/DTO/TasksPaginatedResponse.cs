namespace ServiceContracts.DTO
{
    public class TasksPaginatedResponse
    {
        public IEnumerable<TaskResponse> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int) Math.Ceiling(TotalCount / (double) PageSize);
    }
}
