using EazyRent.Data;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace EazyRent.Models.Repositories
{
    public class PropertyRepository : IProperty
    {
        private readonly RentalDBContext _dbContext;
        public PropertyRepository(RentalDBContext rentalDBContext)
        {
            _dbContext = rentalDBContext;
        }
        public PropertyDetailsDTO DisplayOwnerProperty(int ownerId)
        {
            var property = _dbContext.Properties
            .Where(t => t.OwnerId == ownerId)
            .Select(t => new PropertyDetailsDTO
            {
                Address = t.Address,
                RentAmount = t.RentAmount,
                AvailabilityStatus = t.AvailabilityStatus,
                PropertyImage= t.PropertyImage,
                PropertyDescription= t.PropertyDescription
            })
            .FirstOrDefault();

            return property;
        }

        public async Task<bool> AddPropertyAsync(string ownerEmail, PropertyDetailsDTO dto)
        {
            var owner = _dbContext.Users.FirstOrDefault(o => o.Email == ownerEmail);
            if (owner == null)
                return false;

            var property = new Property
            {
                OwnerId = owner.UserId,
                Address = dto.Address,
                RentAmount = dto.RentAmount,
                AvailabilityStatus = dto.AvailabilityStatus,
                PropertyImage = dto.PropertyImage,
                PropertyDescription=dto.PropertyDescription
            };

            _dbContext.Properties.Add(property);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<PropertyDetailsDTO> GetPropertyByIdAsync(int propertyId)
        {
            var property = await _dbContext.Properties
                .Where(p => p.PropertyId == propertyId) // Assuming your Property entity has a 'PropertyId' field
                .Select(p => new PropertyDetailsDTO
                {
                    Address = p.Address,
                    RentAmount = p.RentAmount,
                    AvailabilityStatus = p.AvailabilityStatus,
                    PropertyImage = p.PropertyImage,
                    PropertyDescription = p.PropertyDescription
                    // Include any other details you want in the DTO
                })
                .FirstOrDefaultAsync(); // Use FirstOrDefaultAsync to get a single item or null

            return property;
        }

        public async Task<IEnumerable<PropertyDetailsDTO>> GetAllPropertiesAsync()
        {
            var properties = await _dbContext.Properties
                .Select(p => new PropertyDetailsDTO
                {
                    Address = p.Address,
                    RentAmount = p.RentAmount,
                    AvailabilityStatus = p.AvailabilityStatus,
                    PropertyImage = p.PropertyImage,
                    PropertyDescription = p.PropertyDescription
                })
                .ToListAsync(); 

            return properties;
        }

        public async Task<IEnumerable<PropertyDetailsDTO>> GetPropertiesForOwnerAsync(int ownerId)
        {
            var properties = await _dbContext.Properties
                .Where(p => p.OwnerId == ownerId) // <-- This is the crucial filter!
                .Select(p => new PropertyDetailsDTO
                {
                    //PropertyId = p.PropertyId, // Assuming PropertyDetailsDTO has PropertyId
                    Address = p.Address,
                    RentAmount = p.RentAmount,
                    AvailabilityStatus = p.AvailabilityStatus,
                    PropertyImage = p.PropertyImage,
                    PropertyDescription = p.PropertyDescription
                    // You might want to include OwnerId here for clarity, though it's implied
                    // OwnerId = p.OwnerId
                })
                .ToListAsync();

            return properties;
        }



        public async Task<bool> UpdatePropertyAsync(int propertyId, int ownerId, PropertyDetailsDTO updatedPropertyDetails)
        {
            var existingProperty = await _dbContext.Properties
                                                   .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (existingProperty == null)
            {
                return false;
            }

            if (existingProperty.OwnerId != ownerId)
            {
                return false; // Unauthorized attempt
            }

            
            if (updatedPropertyDetails.Address != null)
            {
                existingProperty.Address = updatedPropertyDetails.Address; 
            }
           

            if (updatedPropertyDetails.AvailabilityStatus != null)
            {
                existingProperty.AvailabilityStatus = updatedPropertyDetails.AvailabilityStatus;
            }
            if (updatedPropertyDetails.PropertyDescription != null)
            {
                existingProperty.PropertyDescription = updatedPropertyDetails.PropertyDescription;
            }


            // For nullable decimals (decimal?): Check if it has a value
            if (updatedPropertyDetails.RentAmount.HasValue) // Check if a value was provided at all
            {
                existingProperty.RentAmount = updatedPropertyDetails.RentAmount; // Assign the value, even if it's 0
            }
         

            if (updatedPropertyDetails.PropertyImage != null)
            {
                existingProperty.PropertyImage = updatedPropertyDetails.PropertyImage;
            }
            
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePropertyAsync(int propertyId, int ownerId)
        {
            // 1. Find the property by ID
            var propertyToDelete = await _dbContext.Properties
                                                 .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

          
            if (propertyToDelete == null)
            {
                return false; 
            }

          
            if (propertyToDelete.OwnerId != ownerId)
            {
                
                return false; 
            }

            _dbContext.Properties.Remove(propertyToDelete);

            
            await _dbContext.SaveChangesAsync();

            return true; 
        }
    }
}
