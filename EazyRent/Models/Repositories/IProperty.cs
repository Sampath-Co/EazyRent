using EazyRent.Models.DTO;

namespace EazyRent.Models.Repositories
{
    public interface IProperty
    {
        PropertyDetailsDTO DisplayOwnerProperty(int ownerId);
        Task<bool> AddPropertyAsync(string ownerEmail, PropertyDetailsDTO dto);
        Task<PropertyDetailsDTO> GetPropertyByIdAsync(int propertyId);
        Task<IEnumerable<PropertyDetailsDTO>> GetAllPropertiesAsync();
        Task<IEnumerable<PropertyDetailsDTO>> GetPropertiesForOwnerAsync(int ownerId);
        Task<bool> UpdatePropertyAsync(int propertyId, int ownerId, PropertyDetailsDTO updatedPropertyDetails);
        Task<bool> DeletePropertyAsync(int propertyId, int ownerId);
    }
}
