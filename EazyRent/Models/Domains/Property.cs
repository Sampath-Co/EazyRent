using System;
using System.Collections.Generic;

namespace EazyRent.Models.Domains;

public partial class Property
{
    public int PropertyId { get; set; }

    public int? OwnerId { get; set; }

    public string? Address { get; set; }

    public decimal? RentAmount { get; set; }

    public string? AvailabilityStatus { get; set; }

    public byte[]? PropertyImage { get; set; }

    public string? PropertyDescription { get; set; }

    public virtual ICollection<Lease> Leases { get; set; } = new List<Lease>();

    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    public virtual User? Owner { get; set; }
}
