using System.ComponentModel.DataAnnotations;

namespace EazyRent.Models.DTO
{
    public class CreateMaintenceRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public int? PropertyId { get; set; }

        [Required]
        public int? TenantId { get; set; }

        [Required]
        [MaxLength(500)]
        public string? IssueDescription { get; set; }

        [Required]
        [RegularExpression("^(Pending|Active|Terminated)$", ErrorMessage = "Status must be one of: Pending, Active, Terminated.")]
        public string? Status { get; set; }
    }
}
