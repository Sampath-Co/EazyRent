using EazyRent.Models.DTO;
using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace EazyRent.Models.Repositories
{
    public interface IProperty
    {
        PropertyDetailsDTO DisplayOwnerProperty(int ownerId);
        Task<bool> AddPropertyAsync(string ownerEmail, PropertyDetailsDTO dto);
        Task<GetPropertiesDTO> GetPropertyByIdAsync(int propertyId);
        Task<GetPropertiesDTO> GetPropertyByIdAndOwnerIdAsync(int propertyId, int ownerId);
        Task<IEnumerable<GetPropertiesDTO>> GetAllPropertiesAsync( string? filterOn = null, string? filterQuery = null, decimal? filterRent = null);
        Task<IEnumerable<GetPropertiesDTO>> GetPropertiesForOwnerAsync(int ownerId);
        Task<bool> UpdatePropertyAsync(int propertyId, int ownerId, PropertyDetailsDTO updatedPropertyDetails);
        Task<(bool Success, bool HasLeases)> DeletePropertyAsync(int propertyId, int ownerId);
    }
}
