using System;
using System.Collections.Generic;

namespace EazyRent.Models.Domains;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? LeaseId { get; set; }

    public decimal? Amount { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public string? Status { get; set; }

    public virtual Lease? Lease { get; set; }
}
