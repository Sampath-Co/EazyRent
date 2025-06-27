using EazyRent.Data;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace EazyRent.Models.Repositories
{
    public class LeaseRepository : ILease
    {
        private readonly RentalDBContext _dbContext;

        public LeaseRepository(RentalDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Lease?> CreateLeaseRequestAsync(int tenantId, CreateLeaseDTO createLeaseDto)
        {
            var property = await _dbContext.Properties
                                           .Include(p => p.Owner) // Include owner
                                           .FirstOrDefaultAsync(p => p.PropertyId == createLeaseDto.PropertyId);

            // Property not found
            if (property == null)
            {
                return null; // Controller will translate this to NotFound
            }

            // Tenant is the owner of this property (Unauthorized scenario)
            if (property.OwnerId == tenantId)
            {
                return null; // Controller will translate this to Forbid
            }

            // Property is not available for lease (Bad Request scenario)
            if (property.AvailabilityStatus != "Available") // Adjust status string as per your domain
            {
                return null; // Controller will translate this to BadRequest
            }

            // Map DTO to Lease entity
            var newLease = new Lease
            {
                PropertyId = createLeaseDto.PropertyId,
                TenantId = tenantId,
                StartDate = DateOnly.FromDateTime(createLeaseDto.StartDate),
                EndDate = DateOnly.FromDateTime(createLeaseDto.EndDate),
                RentAmount = property.RentAmount ?? createLeaseDto.ProposedRentAmount,
                Status = "Pending",
                DigitalSignature = createLeaseDto.DigitalSignature != null ? ConvertFormFileToByteArray(createLeaseDto.DigitalSignature) : null
            };

            _dbContext.Leases.Add(newLease);
            await _dbContext.SaveChangesAsync();

            return newLease;
        }

        public async Task<Lease?> GetLeaseByIdAsync(int leaseId)
        {
            return await _dbContext.Leases
                .Include(l => l.Property)
                .FirstOrDefaultAsync(l => l.LeaseId == leaseId);
        }

        public async Task<IEnumerable<GetLeaseDetailsDTO>> GetLeasesByTenantIdAsync(int tenantId)
        {
            var leases = await _dbContext.Leases
                .Include(l => l.Tenant)
                .Include(l => l.Property)
                .Where(l => l.TenantId == tenantId)
                .ToListAsync();

            return leases.Select(lease => new GetLeaseDetailsDTO
            {
                PropertyId = lease.PropertyId ?? 0,
                TenantName = lease.Tenant != null ? lease.Tenant.FullName : null, // <-- Use Tenant's name
                StartDate = lease.StartDate.HasValue ? lease.StartDate.Value.ToDateTime(TimeOnly.MinValue) : default(DateTime),
                EndDate = lease.EndDate.HasValue ? lease.EndDate.Value.ToDateTime(TimeOnly.MinValue) : default(DateTime),
                RentAmount = lease.RentAmount ?? 0,
                Status = lease.Status
            }).ToList();
        }

        public async Task<IEnumerable<LeaseDetailsDTO>> GetLeasesByOwnerIdAsync(int ownerId)
        {
            return await _dbContext.Leases
                .Include(l => l.Property) // Include the related Property
                .Include(l => l.Tenant) // Include the related Tenant (User)
                .Where(l => l.Property.OwnerId == ownerId) // Filter by the OwnerId on the Property
                .Select(l => new LeaseDetailsDTO
                {
                    LeaseId = l.LeaseId,
                    PropertyId = l.PropertyId,
                    TenantId = l.TenantId, // Keep TenantId for reference
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    RentAmount = l.RentAmount,
                    Status = l.Status,
                    DigitalSignature = l.DigitalSignature,
                    // Use Tenant's name instead of TenantId
                    TenantName = l.Tenant != null ? l.Tenant.FullName : null
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateLeaseAsync(Lease lease)
        {
            _dbContext.Leases.Update(lease);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLeaseByOwnerAsync(int leaseId, int ownerId)
        {
            var lease = await _dbContext.Leases.Include(l => l.Property).FirstOrDefaultAsync(l => l.LeaseId == leaseId);
            if (lease == null || lease.Property == null || lease.Property.OwnerId != ownerId)
            {
                return false;
            }
            _dbContext.Leases.Remove(lease);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        private static byte[] ConvertFormFileToByteArray(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
