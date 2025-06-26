using EazyRent.Models.Domains;
using EazyRent.Models.DTO;

namespace EazyRent.Models.Repositories
{
    public interface ILease
    {
        Task<Lease?> CreateLeaseRequestAsync(int tenantId, CreateLeaseDTO createLeaseDto);
        Task<IEnumerable<GetLeaseDetailsDTO>> GetLeasesByTenantIdAsync(int tenantId); // <-- Changed here
        Task<IEnumerable<LeaseDetailsDTO>> GetLeasesByOwnerIdAsync(int ownerId);
        Task<Lease> GetLeaseByIdAsync(int leaseId);
        Task<bool> UpdateLeaseAsync(Lease lease);
        Task<bool> DeleteLeaseByOwnerAsync(int leaseId, int ownerId);
    }
}
