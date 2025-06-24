namespace EazyRent.Models.DTO
{
    public class MaintenanceRequestDto
    {
        public int RequestId { get; set; }
        public int? PropertyId { get; set; }
        public string? TenantFullName { get; set; } // <-- Add this

        public string? IssueDescription { get; set; }
        public string? Status { get; set; }
    }
}
