﻿using System.Security.Claims;
using EazyRent.Models.DTO;
using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PropertyController : ControllerBase
    {
        private readonly IProperty _property;
        public PropertyController(IProperty property)
        {
            _property = property;
        }

        //[Authorize(Roles = "Owner")]
        //[HttpGet("owner-properties")] 
        //public IActionResult DisplayOwnerProperty()
        //{

        //    var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 


        //    if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
        //    {
        //        return Unauthorized("Owner ID claim not found or is invalid in token.");
        //    }


        //    var propertyDetails = _property.DisplayOwnerProperty(ownerId); 


        //    if (propertyDetails == null)
        //    {
        //        return NotFound("No property found for this owner."); 
        //    }

        //    return Ok(propertyDetails); 
        //}


        [HttpGet("/Owner/Properties")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetMyPropertiesAsOwner()
        {

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { Message = "Owner ID claim not found or is invalid in token." });
            }

            try
            {
                var properties = await _property.GetPropertiesForOwnerAsync(ownerId);

                if (properties == null || !properties.Any())
                {
                    return NotFound(new { Message = "No property found for this owner." });
                }

                return Ok(properties);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while fetching owner's properties.", Details = ex.Message });
            }
        }



        [Authorize(Roles = "Owner")]
        [HttpPost("/Owner/AddProperty")]
        public async Task<IActionResult> AddProperty([FromForm] PropertyDetailsDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            try
            {
                // Handle image upload
                if (dto.PropertyImage != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");



                    // Create folder if it doesn't exist
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique file name
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.PropertyImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.PropertyImage.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    return BadRequest(new { Message = "No image uploaded." });
                }

                // Save property
                var success = await _property.AddPropertyAsync(email, dto);
                if (!success)
                {
                    return Unauthorized(new { Message = "Owner not found" });
                }

                return Ok(new
                {
                    Message = "Property added successfully",
                    ImagePath = dto.PropertyDescription // For debugging
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while uploading the property." });
            }
        }


        [Authorize(Roles = "Tenant")]
        [HttpGet("/Tenant/GetPropertyById/{propertyId}")]
        public async Task<IActionResult> GetPropertyById(int propertyId)
        {
            if (propertyId <= 0)
            {
            return BadRequest(new { Message = "Invalid property ID." });
            }

            try
            {
            var property = await _property.GetPropertyByIdAsync(propertyId);

            if (property == null)
            {
                return NotFound(new { Message = $"Property with ID {propertyId} not found." });
            }

            return Ok(property);
            }
            catch (Exception ex)
            {
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while fetching the property.", Details = ex.Message });
            }
        }

        [Authorize(Roles = "Owner")]
        [HttpGet("/Owner/GetPropertyById/{propertyId}")]
        public async Task<IActionResult> GetPropertyByIdAsync(int propertyId)
        {
            // 1. Basic validation for property ID
            if (propertyId <= 0)
            {
                return BadRequest("Invalid property ID.");
            }

            // 2. Get the current authenticated owner's ID
            var ownerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (ownerIdClaim == null)
            {
                return Unauthorized("Owner ID not found in claims.");
            }

            if (!int.TryParse(ownerIdClaim.Value, out int ownerId))
            {
                return Unauthorized("Invalid Owner ID format.");
            }

            // 3. Retrieve the property and check ownership
            var property = await _property.GetPropertyByIdAndOwnerIdAsync(propertyId, ownerId);

            if (property == null)
            {
                // Return NotFound if the property doesn't exist OR if it exists but doesn't belong to the current owner.
                return NotFound($"Property with ID {propertyId} not found or you do not have access to it.");
            }

            // 4. Return the property if found and owned by the current user
            return Ok(property);
        }




        [HttpPut("/Owner/UpdateProperty/{propertyId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UpdateProperty(int propertyId, [FromForm] PropertyDetailsDTO updatedPropertyDetails)
        {
            if (propertyId <= 0)
            {
                return BadRequest(new { message = "Invalid property ID." });
            }

            if (updatedPropertyDetails == null)
            {
                return BadRequest(new { message = "Property details cannot be null." });
            }

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { message = "Owner ID claim not found or is invalid." });
            }

            var success = await _property.UpdatePropertyAsync(propertyId, ownerId, updatedPropertyDetails);
            if (!success)
            {
                var propertyExists = await _property.GetPropertyByIdAsync(propertyId) != null;
                if (!propertyExists)
                {
                    return NotFound(new { message = $"Property with ID {propertyId} not found." });
                }
                else
                {
                    return Forbid("You are not authorized to update this property.");
                }
            }

            return Ok(new { message = "Property updated successfully." });
        }


        [HttpDelete("/Owner/DeleteProperty/{propertyId}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            if (propertyId <= 0)
            {
                return BadRequest(new { Message = "Invalid property ID." });
            }

            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized(new { Message = "Owner ID claim not found or is invalid." });
            }

            var (success, hasLeases) = await _property.DeletePropertyAsync(propertyId, ownerId);

            if (!success)
            {
                if (hasLeases)
                {
                    return Conflict(new { Message = "Cannot delete property. There are active leases for this property. Please delete all leases first." });
                }

                var propertyExists = await _property.GetPropertyByIdAsync(propertyId) != null;
                if (!propertyExists)
                {
                    return NotFound(new { Message = $"Property with ID {propertyId} not found." });
                }
                else
                {
                    return Forbid("You are not authorized to delete this property.");
                }
            }

            return Ok(new { Message = "Property deleted successfully." });
        }
    }

}
