// EazyRent.Controllers/LeaseController.cs
using System.Security.Claims;
using EazyRent.Models.Repositories;
using EazyRent.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EazyRent.Models.Domains;
using EazyRent.Data; // If you want to return the full Lease object or use for internal checks

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseController : ControllerBase
    {
        private readonly ILease _leaseRepository;

        public IPayment _paymentRepository { get; }

        // private readonly RentalDBContext _dbContext; // <-- DO NOT inject DbContext here if you use a repository

        public LeaseController(ILease leaseRepository ,IPayment paymentRepository) // ONLY inject the repository
        {
            _leaseRepository = leaseRepository;
            _paymentRepository = paymentRepository;
            // _dbContext = dbContext; // <-- REMOVE this line if it existed
        }

        [Authorize(Roles = "Tenant")]
        [HttpPost("Tenant/RequestLease")]
        public async Task<IActionResult> RequestLease([FromBody] CreateLeaseDTO createLeaseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
                {
                    throw new UnauthorizedAccessException("Tenant ID claim not found or is invalid in token.");
                }

                var newLease = await _leaseRepository.CreateLeaseRequestAsync(tenantId, createLeaseDto);

                if (newLease == null)
                {
                    throw new Exception("Could not create lease request.");
                }

                // Create initial payment
                var payment = new Payment
                {
                    LeaseId = newLease.LeaseId,
                    Amount = createLeaseDto.ProposedRentAmount, // or any logic you use
                    //PaymentDate = DateTime.Now(), // example due date
                    Status = "Pending"
                }; 

                await _paymentRepository.AddPaymentAsync(payment);

                return Ok("Lease and initial payment created.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred.", Details = ex.Message });
            }
        }


        //[Authorize(Roles = "Tenant")]
        //[HttpPost("/Tenant/RequestLease")]
        //public async Task<IActionResult> RequestLease([FromBody] CreateLeaseDTO createLeaseDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
        //        {
        //            throw new UnauthorizedAccessException("Tenant ID claim not found or is invalid in token.");
        //        }

        //        var newLease = await _leaseRepository.CreateLeaseRequestAsync(tenantId, createLeaseDto);

        //        if (newLease == null)
        //        {
        //            throw new Exception("Could not create lease request. Please check property details or contact support.");
        //        }

        //        // 201 Created is appropriate for successful resource creation
        //        return Ok("Lease Created");
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(new { Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while creating the lease.", Details = ex.Message });
        //    }
        //}
        //[HttpGet("my-leases/tenant")]
        //[Authorize(Roles = "Tenant")] // Only Tenants can access this endpoint
        //public async Task<IActionResult> GetMyLeasesAsTenant()
        //{
        //    var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
        //    {
        //        return Unauthorized("Tenant ID claim not found or is invalid in token.");
        //    }

        //    var leases = await _leaseRepository.GetLeaseByIdAsync(tenantId); 

        //    if (leases == null || !leases.Any())
        //    {
        //        return NoContent(); // 204 No Content if no leases found for this tenant
        //    }

        //    return Ok(leases); // Returns a list of LeaseDetailsDTO
        //}

        [HttpGet("/Tenant/Leases")]
        [Authorize(Roles = "Tenant")] // Only Tenants can access this endpoint
        public async Task<IActionResult> GetMyLeasesAsTenant()
        {
            // 1. Get the authenticated Tenant's ID from their claims
            var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
            {
                // This means the user is authenticated but their tenant ID claim is missing or invalid.
                // This shouldn't happen with a well-configured authentication system.
                return Unauthorized("Tenant ID claim not found or is invalid in token.");
            }

            // 2. Call the repository to get all leases for this specific tenant
            var leases = await _leaseRepository.GetLeasesByTenantIdAsync(tenantId);

            // 3. Handle the response
            if (!leases.Any()) // Check if the collection is empty
            {
                return NoContent(); // 204 No Content if no leases found for this tenant
            }

            // Return the list of LeaseDetailsDTOs
            return Ok(leases); // 200 OK with the list of leases
        }

        //Controller:
        [HttpGet("/Owner/Leases")]
        [Authorize(Roles = "Owner")] // Only Owners can access this endpoint
        public async Task<IActionResult> GetMyLeasesAsOwner()
        {
            // 1. Get the authenticated Owner's ID from their claims
            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                // This means the user is authenticated but their owner ID claim is missing or invalid.
                return Unauthorized("Owner ID claim not found or is invalid in token.");
            }

            // 2. Call the repository to get all leases for properties owned by this owner
            var leases = await _leaseRepository.GetLeasesByOwnerIdAsync(ownerId);

            if (!leases.Any())
            {
                return NoContent(); // 204 No Content if no leases found for this owner's properties
            }

            return Ok(leases); // Returns a list of LeaseDetailsDTO
        }


    }
}