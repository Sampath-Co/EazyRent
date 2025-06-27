namespace EazyRent.Models.DTO
{
    public class LeaseDetailsDTO
    {
        public int LeaseId { get; set; } // LeaseId is not in CreateLeaseDTO
        public int? PropertyId { get; set; }
        public int? TenantId { get; set; }
        public string? TenantName { get; set; } // Added TenantName
        public DateOnly? StartDate { get; set; } // Nullable, matches domain
        public DateOnly? EndDate { get; set; }   // Nullable, matches domain
        public decimal? RentAmount { get; set; } // Matches domain
        public string? Status { get; set; }
        public byte[]? DigitalSignature { get; set; }
    } 
}
