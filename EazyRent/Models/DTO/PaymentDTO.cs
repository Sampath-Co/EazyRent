namespace EazyRent.Models.DTO
{
    public class PaymentDTO
    {
        public int? LeaseId { get; set; }
        public decimal? Amount { get; set; }
        public DateOnly? PaymentDate { get; set; }
        public string? Status { get; set; }
    }
}