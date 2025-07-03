using EazyRent.Models.Domains;

// This partial class is not needed for tenant name in PaymentDTO; handled in DTOs and repository mapping.
public partial class MaintenanceRequestName
{
    public virtual User Tenant { get; set; } 
}