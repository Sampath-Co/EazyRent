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
                StartDate = createLeaseDto.StartDate,
                EndDate = createLeaseDto.EndDate,
                RentAmount = createLeaseDto.ProposedRentAmount ?? property.RentAmount,
                Status = "Pending",
                DigitalSignature = createLeaseDto.DigitalSignatureBase64 != null ? Convert.FromBase64String(createLeaseDto.DigitalSignatureBase64) : null
            };

            _dbContext.Leases.Add(newLease);
            await _dbContext.SaveChangesAsync();

            return newLease;
        }
        //public async Task<LeaseDetailsDTO?> GetLeaseByIdAsync(int leaseId)
        //{
        //    var lease = await _dbContext.Leases
        //                                .Include(l => l.Property) // Include related property if needed for DTO
        //                                .Include(l => l.Tenant)   // Include related tenant if needed for DTO
        //                                .FirstOrDefaultAsync(l => l.LeaseId == leaseId);

        //    if (lease == null)
        //    {
        //        return null; // Lease not found
        //    }

        //    // Map the Lease entity to LeaseDetailsDTO
        //    var leaseDetailsDto = new LeaseDetailsDTO
        //    {
        //        LeaseId = lease.LeaseId,
        //        PropertyId = lease.PropertyId,
        //        TenantId = lease.TenantId,
        //        StartDate = lease.StartDate,
        //        EndDate = lease.EndDate,
        //        RentAmount = lease.RentAmount,
        //        Status = lease.Status,
        //        DigitalSignature = lease.DigitalSignature,
        //        // Add any other properties you want to include from the Lease entity or its related entities
        //        // Example:
        //        // PropertyAddress = lease.Property?.Address, // Use ?. for null-conditional if Property can be null
        //        // TenantEmail = lease.Tenant?.Email,
        //    };

        //    return leaseDetailsDto;
        //}
        public async Task<IEnumerable<LeaseDetailsDTO>> GetLeasesByTenantIdAsync(int tenantId)
        {
            var leases = await _dbContext.Leases
                                         .Where(l => l.TenantId == tenantId) // Crucial filter for the specific tenant
                                         .Include(l => l.Property)           // Eager load Property details
                                         .Include(l => l.Tenant)             // Eager load Tenant details
                                         .ToListAsync(); // Get all matching leases

            if (!leases.Any()) // If no leases found, return an empty collection
            {
                return Enumerable.Empty<LeaseDetailsDTO>();
            }

            // Map the list of Lease entities to a list of LeaseDetailsDTOs
            var leaseDetailsDtos = leases.Select(lease => new LeaseDetailsDTO
            {
                LeaseId = lease.LeaseId,
                PropertyId = lease.PropertyId,
                TenantId = lease.TenantId,
                StartDate = lease.StartDate,
                EndDate = lease.EndDate,
                RentAmount = lease.RentAmount,
                Status = lease.Status,
                DigitalSignature = lease.DigitalSignature
            }).ToList();

            return leaseDetailsDtos;
        }
    }
}
