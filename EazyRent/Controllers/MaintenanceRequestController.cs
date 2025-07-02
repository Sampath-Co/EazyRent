using AutoMapper;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EazyRent.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class MaintenanceRequestController : ControllerBase
    {
        private readonly IMaintenanceRequestRepository _maintenanceRequestRepository;
        private readonly IMapper _mapper;

        public MaintenanceRequestController(IMaintenanceRequestRepository maintenanceRequestRepository, IMapper mapper)
        {
            _maintenanceRequestRepository = maintenanceRequestRepository;
            _mapper = mapper;
        }

        // To get all maintenance requests
        [Authorize(Roles = "Owner")]
        [HttpGet("/Owner/GetAllMaintenance/")]
        public async Task<IActionResult> GetAllRequests()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            int ownerId = int.Parse(userId);

            var requests = await _maintenanceRequestRepository.GetAllRequest(ownerId);
            var result = _mapper.Map<List<MaintenanceRequestDto>>(requests);
            return Ok(result);
        }

        //To get a maintenance request by its ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRequestById([FromRoute] int id)
        {
            var request = await _maintenanceRequestRepository.GetRequestById(id);
            if (request == null)
            {
                return NotFound(new { Message = "Maintenance request not found" });
            }
            var result = _mapper.Map<MaintenanceRequestDto>(request);
            return Ok(result);
        }

        [Authorize(Roles = "Tenant")]
        [HttpGet("/Tenant/GetAllMaintenance/")]
        public async Task<IActionResult> GetAllRequestsByTenant()
        {
            // Extract the UserId from the JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token.");
            int tenantId = int.Parse(userId);
            var requests = await _maintenanceRequestRepository.GetAllRequestsByTenant(tenantId);
            var result = _mapper.Map<List<MaintenanceRequestDto>>(requests);
            return Ok(result);
        }

        [Authorize(Roles = "Tenant")]
        [HttpPost("/Tenant/CreateMaintenanceRequest/")]
        public async Task<IActionResult> AddRequest([FromBody] MaintenanceRequestDto requestDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "User ID not found in token." });
            }

            int loggedInTenantId = int.Parse(userId); // ✅ Use this instead of requestDto.TenantId

            // ✅ Validate that the tenant has a lease on the property
            if (!requestDto.PropertyId.HasValue)
            {
                return BadRequest(new { Message = "Property ID is required." });
            }

            var lease = await _maintenanceRequestRepository
                .GetLeaseByPropertyIdAndTenantId(requestDto.PropertyId.Value, loggedInTenantId);

            if (lease == null || string.IsNullOrEmpty(lease.Status) || lease.Status.ToLower() != "active")
            {
                return BadRequest(new { Message = "Tenant does not have an active lease on the specified property." });
            }

            // ✅ Map and override tenantId to ensure security
            var request = _mapper.Map<MaintenanceRequest>(requestDto);
            request.TenantId = loggedInTenantId;

            await _maintenanceRequestRepository.AddRequest(request);

            return CreatedAtAction(nameof(GetRequestById), new { id = request.RequestId }, requestDto);
        }

        // Adjusting the method to update maintenance status using PropertyId
        [Authorize(Roles = "Owner")]
        [HttpPut("Owner/Update/{requestId:int}")]
        public async Task<IActionResult> UpdateMaintenanceStatus([FromRoute] int requestId, [FromBody] string status)
        {
            try
            {
                // Extract the UserId from the JWT claims
                var loggedInOwnerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(loggedInOwnerId))
                {
                    return Unauthorized(new { Message = "User ID not found in token." });
                }

                // Validate that the owner owns the property linked to the maintenance request
                var maintenanceRequest = await _maintenanceRequestRepository.GetRequestById(requestId);
                if (maintenanceRequest == null || maintenanceRequest.Property == null || maintenanceRequest.Property.OwnerId.ToString() != loggedInOwnerId)
                {
                    return BadRequest(new { Message = "Owner does not own the property linked to the specified maintenance request." });
                }

                // Update the maintenance status
                maintenanceRequest.Status = status;
                await _maintenanceRequestRepository.UpdateRequest(requestId, maintenanceRequest);
                return Ok(new { Message = "Maintenance status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while updating the maintenance status.", Details = ex.Message });
            }
        }

        [Authorize(Roles = "Owner")]
        [HttpDelete("/Owner/DeleteMaintenance/{requestId:int}")]
        public async Task<IActionResult> DeleteMaintenanceRequest([FromRoute] int requestId)
        {
            if (requestId <= 0)
            {
                return BadRequest(new { message = "Invalid maintenance request ID." });
            }

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { message = "Owner ID claim not found or is invalid." });
            }

            // Retrieve the maintenance request.
            MaintenanceRequest maintenanceRequest;
            try
            {
                maintenanceRequest = await _maintenanceRequestRepository.GetRequestById(requestId);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }

            // Check if maintenance request belongs to the owner.
            if (maintenanceRequest.Property == null || maintenanceRequest.Property.OwnerId != ownerId)
            {
                return Unauthorized(new { message = "You are not authorized to delete this maintenance request." });
            }

            // Only allow deletion if the maintenance status is "terminated"
            if (!maintenanceRequest.Status.Equals("terminated", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Maintenance request cannot be deleted. Its status must be terminated first." });
            }

            await _maintenanceRequestRepository.DeleteRequest(requestId);
            return Ok(new { message = "Maintenance request deleted successfully." });
        }
    }
}
