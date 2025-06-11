using System;
using System.Collections.Generic;

namespace EazyRent.Models.Domains;

public partial class MaintenanceRequest
{
    public int RequestId { get; set; }

    public int? PropertyId { get; set; }

    public int? TenantId { get; set; }

    public string? IssueDescription { get; set; }

    public string? Status { get; set; }


    public virtual Property? Property { get; set; }

    public virtual User? Tenant { get; set; }

}
