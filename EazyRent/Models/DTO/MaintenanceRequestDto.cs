namespace EazyRent.Models.DTO
{
    public class MaintenanceRequestDto
    {
        public int RequestId { get; set; }
        public int? PropertyId { get; set; }

        public int? TenantId { get; set; }
        public string? TenantFullName { get; set; } // Added TenantName

        public string? IssueDescription { get; set; }
        public string? Status { get; set; }
    }
}
