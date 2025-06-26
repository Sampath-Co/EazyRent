using EazyRent.Models.DTO;
using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly ILease _leaseRepository;
        public OwnerController(ILease leaseRepository)
        {
            _leaseRepository = leaseRepository;
        }

        //public LeaseRepository LeaseRepository { get; }

        [Authorize(Roles = "Owner")]
        [HttpPost("/Owner/ApproveRejectLease")]
        public async Task<IActionResult> ApproveRejectLease([FromBody] ApproveRejectLeaseDto approveRejectLeaseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized("Owner ID claim not found or is invalid in token.");
            }

            try
            {
                var lease = await _leaseRepository.GetLeaseByIdAsync(approveRejectLeaseDto.LeaseId);
                if (lease == null || lease.Property?.OwnerId != ownerId)
                {
                    return NotFound("Lease request not found or you are not the owner.");
                }

                lease.Status = approveRejectLeaseDto.Status;

                var result = await _leaseRepository.UpdateLeaseAsync(lease);

                if (!result)
                {
                    return BadRequest("Could not update lease request. Please try again or contact support.");
                }

                return Ok($"Lease request has been {approveRejectLeaseDto.Status.ToLower()}.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while processing the lease request.", Details = ex.Message });
            }
        }

        [Authorize(Roles = "Owner")]
        [HttpDelete("/Owner/DeleteLease/{leaseId}")]
        public async Task<IActionResult> DeleteLease(int leaseId)
        {
            if (leaseId <= 0)
            {
                return BadRequest(new { message = "Invalid lease ID." });
            }

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { message = "Owner ID claim not found or is invalid." });
            }

            var success = await _leaseRepository.DeleteLeaseByOwnerAsync(leaseId, ownerId);
            if (!success)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You are not authorized to delete this lease or lease not found." });
            }

            return Ok(new { message = "Lease deleted successfully." });
        }
    }
}
