using System.Security.Claims;
using EazyRent.Models.DTO;
using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EazyRent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly IProperty _property;
        public PropertyController(IProperty property)
        {
            _property = property;
        }

        [Authorize(Roles = "Owner")]
        [HttpGet("owner-properties")] 
        public IActionResult DisplayOwnerProperty()
        {
          
            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; 

          
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized("Owner ID claim not found or is invalid in token.");
            }

           
            var propertyDetails = _property.DisplayOwnerProperty(ownerId); 

          
            if (propertyDetails == null)
            {
                return NotFound("No property found for this owner."); 
            }

            return Ok(propertyDetails); 
        }




        [HttpGet("/Owner/Properties")]
        [Authorize(Roles = "Owner")] // Only users with the "Owner" role can access this
        public async Task<IActionResult> GetMyPropertiesAsOwner()
        {
            // Extract the OwnerId from the authenticated user's JWT token
            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized("Owner ID claim not found or is invalid in token.");
            }

            var properties = await _property.GetPropertiesForOwnerAsync(ownerId);

            if (properties == null || !properties.Any())
            {
                return NoContent(); // 204 No Content if no properties found for this owner
            }

            return Ok(properties); // Returns a list of PropertyDetailsDTO
        }



        [Authorize(Roles = "Owner")]
        [HttpPost("/Owner/AddProperty")]
        public async Task<IActionResult> AddProperty([FromBody] PropertyDetailsDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var success = await _property.AddPropertyAsync(email, dto);
            if (!success)
                return Unauthorized("Owner not found");

            return Ok("Property added successfully");
        }

        [Authorize(Roles = "Tenant")]
        [HttpGet("/Tenant/GetPropertyById{propertyId}")] // Route parameter for the property ID
        public async Task<IActionResult> GetPropertyById(int propertyId)
        {
            // Input validation for propertyId if needed (e.g., propertyId must be > 0)
            if (propertyId <= 0)
            {
                return BadRequest("Invalid property ID.");
            }

            var property = await _property.GetPropertyByIdAsync(propertyId);

            if (property == null)
            {
                return NotFound($"Property with ID {propertyId} not found.");
            }

            return Ok(property);
        }

        [HttpPut("/Owner/UpdateProperty{propertyId}")] // Use PUT for updating a resource by ID
        [Authorize(Roles = "Owner")] // Only owners can update properties
        public async Task<IActionResult> UpdateProperty(int propertyId, [FromBody] PropertyDetailsDTO updatedPropertyDetails)
        {
            
            if (propertyId <= 0)
            {
                return BadRequest("Invalid property ID.");
            }

            if (updatedPropertyDetails == null)
            {
                return BadRequest("Property details cannot be null.");
            }       
            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized("Owner ID claim not found or is invalid.");
            }            
            var success = await _property.UpdatePropertyAsync(propertyId, ownerId, updatedPropertyDetails);
            if (!success)
            {               
                var propertyExists = await _property.GetPropertyByIdAsync(propertyId) != null;
                if (!propertyExists)
                {
                    return NotFound($"Property with ID {propertyId} not found.");
                }
                else
                {
                    return Forbid("You are not authorized to update this property.");
                }
            }
            return Ok("Property updated successfully.");
        }
        [HttpDelete("/Owner/DeleteProperty{propertyId}")] // Use DELETE for deleting a resource by ID
        [Authorize(Roles = "Owner")] // Only owners can delete their properties
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            // 1. Basic validation
            if (propertyId <= 0)
            {
                return BadRequest("Invalid property ID.");
            }

            // 2. Extract owner ID from token
            var ownerIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdString) || !int.TryParse(ownerIdString, out int ownerId))
            {
                return Unauthorized("Owner ID claim not found or is invalid.");
            }

            // 3. Call the repository to delete the property
            var success = await _property.DeletePropertyAsync(propertyId, ownerId);

            if (!success)
            {
                // The repository returns false if property not found OR if owner doesn't match
                // We need to check if it was not found vs. forbidden.
                var propertyExists = await _property.GetPropertyByIdAsync(propertyId) != null;
                if (!propertyExists)
                {
                    return NotFound($"Property with ID {propertyId} not found.");
                }
                else
                {
                    // Property exists, but ownerId from token didn't match property's ownerId
                    return Forbid("You are not authorized to delete this property.");
                }
            }

            
            return Ok("Property deleted successfully."); 
        }


    }
}
