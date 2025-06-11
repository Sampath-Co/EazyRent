using EazyRent.Models.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EazyRent.Models.Repositories
{
    public interface IMaintenanceRequestRepository
    {
        Task<List<MaintenanceRequest>> GetAllRequest();
        Task<MaintenanceRequest?> GetRequestById(int id);
        Task AddRequest(MaintenanceRequest request);
        Task UpdateRequest(int id, MaintenanceRequest request);
        Task DeleteRequest(int id);
        Task<Property?> GetPropertyById(int propertyId);
        Task<Lease?> GetLeaseByPropertyIdAndTenantId(int propertyId, string tenantId);
        Task UpdateMaintenanceStatus(int leaseId, string status);
        Task<Lease?> GetLeaseById(int leaseId);
    }
}
