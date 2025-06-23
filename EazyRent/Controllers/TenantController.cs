using EazyRent.Models.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EazyRent.Controllers
{
    [Authorize(Roles = "Tenant")]
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly IProperty _property;
        public TenantController(IProperty property)
        {
            _property = property;
        }
        [HttpGet("/Tenant/Properties")]
        public async Task<IActionResult> GetAllProperties([FromQuery] string? filterOn, [FromQuery] string? filterQuery, [FromQuery] decimal? filterRent)
        {
            try
            {
                var properties = await _property.GetAllPropertiesAsync(filterOn, filterQuery, filterRent);

                if (properties == null || !properties.Any())
                {
                    return NoContent();
                }

                // Map the list of Property to a list of GetPropertiesDTO
                var propertyDTOs = mapper.Map<IEnumerable<GetPropertiesDTO>>(properties);

                return Ok(propertyDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while fetching properties.", Details = ex.Message });
            }
        }
    }
}
