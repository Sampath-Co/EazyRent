// EazyRent.Controllers/LeaseController.cs
using System.Security.Claims;
using EazyRent.Models.Repositories;
using EazyRent.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EazyRent.Models.Domains;
using EazyRent.Data; 
using System.IO;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaseController : ControllerBase
    {
        private readonly ILease _leaseRepository;
        private readonly IPayment _paymentRepository;

        public LeaseController(ILease leaseRepository, IPayment paymentRepository)
        {
            _leaseRepository = leaseRepository;
            _paymentRepository = paymentRepository;
        }

        // Tenant: Request a new lease
        [Authorize(Roles = "Tenant")]
        [HttpPost("Tenant/RequestLease")]
        public async Task<IActionResult> RequestLease([FromForm] CreateLeaseDTO createLeaseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Errors = ModelState });
            }

            var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
            {
                return Unauthorized(new { Message = "Tenant ID claim not found or is invalid." });
            }

            var newLease = await _leaseRepository.CreateLeaseRequestAsync(tenantId, createLeaseDto);

            if (newLease == null)
            {
                return BadRequest(new { Message = "Could not create lease request. Please check property details or contact support." });
            }

            // Create an initial payment for the lease
            var payment = new Payment
            {
                LeaseId = newLease.LeaseId, 
                Amount = createLeaseDto.ProposedRentAmount,
                PaymentDate = DateOnly.FromDateTime(DateTime.Now), // Set the current date as PaymentDate
                Status = "Pending"
            };

            await _paymentRepository.AddPaymentAsync(payment);

            return Ok(new { Message = "Lease request created successfully.", LeaseId = newLease.LeaseId });
        }

        // Tenant: Get all leases for the tenant
        [Authorize(Roles = "Tenant")]
        [HttpGet("Tenant/Leases")]
        public async Task<IActionResult> GetMyLeasesAsTenant()
        {
            var tenantIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tenantIdString) || !int.TryParse(tenantIdString, out int tenantId))
            {
                return Unauthorized(new { Message = "Tenant ID claim not found or is invalid." });
            }

            var leases = await _leaseRepository.GetLeasesByTenantIdAsync(tenantId);

            if (!leases.Any())
            {
                return NoContent();
            }

            return Ok(leases); // Returns IEnumerable<GetLeaseDetailsDTO>
        }

        // Owner: Get all leases for the owner
        [Authorize(Roles = "Owner")]
        [HttpGet("Owner/Leases")]
        public async Task<IActionResult> GetMyLeasesAsOwner()
        {
            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { Message = "Owner ID claim not found or is invalid." });
            }

            var leases = await _leaseRepository.GetLeasesByOwnerIdAsync(ownerId);

            if (!leases.Any())
            {
                return NoContent();
            }

            return Ok(leases); // Returns IEnumerable<LeaseDetailsDTO>
        }

        // Owner: Delete a lease
        [Authorize(Roles = "Owner")]
        [HttpDelete("Owner/DeleteLease/{leaseId:int}")]
        public async Task<IActionResult> DeleteLease(int leaseId)
        {
            if (leaseId <= 0)
            {
                return BadRequest(new { Message = "Invalid lease ID." });
            }

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { Message = "Owner ID claim not found or is invalid." });
            }

            var payments = await _paymentRepository.GetPaymentsByLeaseIdAsync(leaseId);
            if (payments != null && payments.Any())
            {
                bool allPaid = payments.All(p => p.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase));
                if (!allPaid)
                {
                    return BadRequest(new { Message = "Cannot delete lease. All payments must be completed first." });
                }

                await _paymentRepository.DeletePaymentsByLeaseIdAsync(leaseId);
            }

            var success = await _leaseRepository.DeleteLeaseByOwnerAsync(leaseId, ownerId);
            if (!success)
            {
                return NotFound(new { Message = "Lease not found or you are not authorized to delete this lease." });
            }

            return Ok(new { Message = "Lease and associated payments deleted successfully." });
        }
    }
}