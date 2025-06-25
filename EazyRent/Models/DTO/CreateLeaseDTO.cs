using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace EazyRent.Models.DTO
{
public class CreateLeaseDTO
{
    public int PropertyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal ProposedRentAmount { get; set; }
    public IFormFile DigitalSignature { get; set; }
}
}

