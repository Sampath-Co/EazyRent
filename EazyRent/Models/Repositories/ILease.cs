using EazyRent.Models.Domains;
using EazyRent.Models.DTO;

namespace EazyRent.Models.Repositories
{
    public interface ILease
    {
        Task<Lease?> CreateLeaseRequestAsync(int tenantId, CreateLeaseDTO createLeaseDto);
        //Task<LeaseDetailsDTO?> GetLeaseByIdAsync(int leaseId);
        Task<IEnumerable<LeaseDetailsDTO>> GetLeasesByTenantIdAsync(int tenantId);
    }
}
