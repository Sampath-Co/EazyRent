using System.ComponentModel.DataAnnotations;

namespace EazyRent.Models.DTO
{
    public class CreateLeaseDTO
    {
        [Required]
        public int PropertyId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; } // Non-nullable, tenant must provide

        [Required]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; } // Non-nullable, tenant must provide

        // RentAmount could be optional if you pull it from the property itself,
        // but if tenant can propose rent, keep it.
        public decimal? ProposedRentAmount { get; set; }

        // DigitalSignature is likely something added *after* agreement,
        // so it's often not part of the initial creation request.
        // If it's part of the initial request, it would also be a Base64 string.
        public string? DigitalSignatureBase64 { get; set; } // For initial request, if applicable
    }
}

