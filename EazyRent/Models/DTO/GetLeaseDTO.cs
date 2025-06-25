namespace EazyRent.Models.DTO
{
    public class GetLeaseDetailsDTO
    {
        public int PropertyId { get; set; }
        public string? TenantName { get; set; } // <-- Add this
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentAmount { get; set; }
        public string? Status { get; set; }
    }
}
