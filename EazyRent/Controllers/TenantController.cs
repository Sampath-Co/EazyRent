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
        public async Task<IActionResult> GetAllProperties()
        {
            var properties = await _property.GetAllPropertiesAsync();

            if (properties == null || !properties.Any()) // Check if the list is null or empty
            {
                return NoContent(); // 204 No Content - indicates success but no resources to return
                                    // or return NotFound("No properties found."); depending on your API design philosophy
            }

            return Ok(properties);
        }
    }
}
