using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EazyRent.Models.DTO;
using EazyRent.Models.Domains;
using EazyRent.Models.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using RenatalPropertyManagement.Models.DTO;

namespace EazyRent.Controllers
{
    [Route("/[controller]")]
    [ApiController]
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
        [HttpGet]
        public async Task<IActionResult> GetAllRequests()
        {
            try
            {
                var requests = await _maintenanceRequestRepository.GetAllRequest();
                var result = _mapper.Map<List<MaintenanceRequestDto>>(requests);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching maintenance requests.", Details = ex.Message });
            }
        }

        // To get a maintenance request by its ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRequestById([FromRoute] int id)
        {
            try
            {
                var request = await _maintenanceRequestRepository.GetRequestById(id);
                if (request == null)
                {
                    return NotFound(new { Message = "Maintenance request not found" });
                }
                var result = _mapper.Map<MaintenanceRequestDto>(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching the maintenance request.", Details = ex.Message });
            }
        }

        // To add a new maintenance request
        [Authorize(Roles = "Tenant")]
        [HttpPost("/Tenant/CreateMaintenanceRequest/")]
        public async Task<IActionResult> AddRequest([FromBody] MaintenanceRequestDto requestDto)
        {
            try
            {
                // Extract the UserId from the JWT claims
                var loggedInTenantId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(loggedInTenantId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                // Validate that the tenant has a lease on the property
                if (!requestDto.PropertyId.HasValue)
                {
                    return BadRequest("Property ID is required.");
                }
                var lease = await _maintenanceRequestRepository.GetLeaseByPropertyIdAndTenantId(requestDto.PropertyId.Value, loggedInTenantId);
                if (lease == null || string.IsNullOrEmpty(lease.Status) || lease.Status.ToLower() != "active")
                {
                    return BadRequest("Tenant does not have an active lease on the specified property.");
                }

                var request = _mapper.Map<MaintenanceRequest>(requestDto);
                await _maintenanceRequestRepository.AddRequest(request);
                return CreatedAtAction(nameof(GetRequestById), new { id = request.RequestId }, requestDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while adding the maintenance request.", Details = ex.Message });
            }
        }

        // Adjusting the method to update maintenance status using PropertyId
        [Authorize(Roles = "Owner")]
        [HttpPut("update-status/{requestId:int}")]
        public async Task<IActionResult> UpdateMaintenanceStatus([FromRoute] int requestId, [FromBody] string status)
        {
            try
            {
                // Extract the UserId from the JWT claims
                var loggedInOwnerId = User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(loggedInOwnerId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                // Validate that the owner owns the property linked to the maintenance request
                var maintenanceRequest = await _maintenanceRequestRepository.GetRequestById(requestId);
                if (maintenanceRequest == null || maintenanceRequest.Property == null || maintenanceRequest.Property.OwnerId.ToString() != loggedInOwnerId)
                {
                    return BadRequest("Owner does not own the property linked to the specified maintenance request.");
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
    }
}
