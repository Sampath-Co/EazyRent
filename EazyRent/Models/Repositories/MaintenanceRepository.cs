using EazyRent.Data;
using EazyRent.Models.Domains;
using EazyRent.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EazyRent.Models.Repositories
{
    public class MaintenanceRepository : IMaintenanceRequestRepository
    {
        private readonly RentalDBContext _context;

        public MaintenanceRepository(RentalDBContext context)
        {
            _context = context;
        }

        public async Task<List<MaintenanceRequest>> GetAllRequest(int ownerId)
        {
            return await _context.MaintenanceRequests
                .Include(m => m.Tenant) // Eager load Tenant
                .Include(m => m.Property)
                .Where(m => m.Property != null && m.Property.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<MaintenanceRequest?> GetRequestById(int id)
        {
            var maintenanceRequest = await _context.MaintenanceRequests
                .Include(m => m.Property)
                .FirstOrDefaultAsync(request => request.RequestId == id);

            if (maintenanceRequest == null)
            {
                throw new KeyNotFoundException($"Maintenance request with RequestId {id} not found.");
            }
            return maintenanceRequest;
        }

        public async Task AddRequest(MaintenanceRequest request)
        {
            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRequest(int id, MaintenanceRequest request)
        {
            var existingRequest = await _context.MaintenanceRequests.FindAsync(id);
            if (existingRequest == null)
            {
                throw new KeyNotFoundException($"Maintenance request with RequestId {id} not found.");
            }

            existingRequest.Status = request.Status;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRequest(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null)
            {
                throw new KeyNotFoundException($"Maintenance request with RequestId {id} not found.");
            }

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();
        }

        public async Task<Property?> GetPropertyById(int propertyId)
        {
            var property = await _context.Properties.Include(p => p.Leases).FirstOrDefaultAsync(p => p.PropertyId == propertyId);
            if (property == null)
            {
                throw new KeyNotFoundException($"Property with PropertyId {propertyId} not found.");
            }
            return property;
        }

        public async Task<Lease?> GetLeaseByPropertyIdAndTenantId(int propertyId, int tenantId)
        {
            return await _context.Leases
                .FirstOrDefaultAsync(l => l.PropertyId == propertyId && l.TenantId == tenantId && l.Status == "Active");
        }

        // Adjusting the method to update maintenance status using PropertyId
        public async Task UpdateMaintenanceStatus(int propertyId, string status)
        {
            var maintenanceRequest = await _context.MaintenanceRequests.FirstOrDefaultAsync(mr => mr.PropertyId == propertyId);
            if (maintenanceRequest == null)
            {
                throw new KeyNotFoundException($"Maintenance request with PropertyId {propertyId} not found.");
            }

            maintenanceRequest.Status = status;
            await _context.SaveChangesAsync();
        }

        public async Task<Lease?> GetLeaseById(int leaseId)
        {
            return await _context.Leases.Include(l => l.Property).FirstOrDefaultAsync(l => l.LeaseId == leaseId);
        }

        public Task<List<MaintenanceRequest>> GetAllRequestsByTenant(int tenantId)
        {
            var requests = _context.MaintenanceRequests
                .Include(mr => mr.Property)
                .Where(mr => mr.Property.Leases.Any(l => l.TenantId == tenantId))
                .ToListAsync();
            return requests;
        }
    }
}