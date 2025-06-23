namespace EazyRent.Models.DTO
{
    public class MaintenanceRequestDto
    {
        public int? PropertyId { get; set; }
        public int? TenantId { get; set; }
        public string? IssueDescription { get; set; }
        public string? Status { get; set; }
    }
}
