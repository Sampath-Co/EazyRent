using System;
using System.Collections.Generic;

namespace EazyRent.Models.Domains;

public partial class Lease
{
    public int LeaseId { get; set; }

    public int? PropertyId { get; set; }

    public int? TenantId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? RentAmount { get; set; }

    public byte[]? DigitalSignature { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Property? Property { get; set; }

    public virtual User? Tenant { get; set; }
}
