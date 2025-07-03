namespace EazyRent.Models.DTO
{
    public class GetLeaseDetailsDTO
    {
        public int LeaseId { get; set; } // Added LeaseId
        public int TenantId { get; set; }
        public int PropertyId { get; set; }
        public string? TenantName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentAmount { get; set; }
        public string? Status { get; set; }
    }
}
