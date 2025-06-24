using AutoMapper;
using EazyRent.Data;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace EazyRent.Models.Repositories
{
    public class PropertyRepository : IProperty
    {
        private readonly RentalDBContext _dbContext;
        private readonly IMapper _mapper;

        public PropertyRepository(RentalDBContext rentalDBContext, IMapper mapper)
        {
            _dbContext = rentalDBContext;
            _mapper = mapper;
        }

        public GetPropertiesDTO DisplayOwnerProperty(int ownerId)
        {
            var property = _dbContext.Properties
                .Where(t => t.OwnerId == ownerId)
                .FirstOrDefault();

            return _mapper.Map<GetPropertiesDTO>(property);
        }

        public async Task<bool> AddPropertyAsync(string ownerEmail, GetPropertiesDTO dto)
        {
            var owner = _dbContext.Users.FirstOrDefault(o => o.Email == ownerEmail);
            if (owner == null)
                return false;

            var property = _mapper.Map<Property>(dto);
            property.OwnerId = owner.UserId;

            _dbContext.Properties.Add(property);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<GetPropertiesDTO> GetPropertyByIdAsync(int propertyId)
        {
            var property = await _dbContext.Properties
                .Where(p => p.PropertyId == propertyId)
                .FirstOrDefaultAsync();

            return _mapper.Map<GetPropertiesDTO>(property);
        }

        //public async Task<IEnumerable<GetPropertiesDTO>> GetAllPropertiesAsync( string? filterOn = null, string? filterQuery = null)
        //{
        //    //var properties = await _dbContext.Properties.ToListAsync();
        //    var propertiesQuery =  _dbContext.Properties.AsQueryable();
        //    if (string.IsNullOrWhiteSpace(filterOn)== false || string.IsNullOrWhiteSpace(filterQuery) == false)
        //    {
        //       if (filterOn.Equals("Address", StringComparison.OrdinalIgnoreCase))
        //        {
        //            propertiesQuery = propertiesQuery.Where(p => p.Address.Contains(filterQuery));
        //        }

        //    }
        //    return _mapper.Map<IEnumerable<GetPropertiesDTO>>(propertiesQuery);
        //}

        public async Task<IEnumerable<GetPropertiesDTO>> GetAllPropertiesAsync(string? filterOn = null, string? filterQuery = null, decimal? filterRent = null)
        {
            var propertiesQuery = _dbContext.Properties.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("Address", StringComparison.OrdinalIgnoreCase))
                {
                    propertiesQuery = propertiesQuery.Where(p => p.Address.ToLower().Contains(filterQuery.ToLower()));
                }
            }

            if (filterRent.HasValue)
            {
                propertiesQuery = propertiesQuery.Where(p => p.RentAmount <= filterRent.Value);
            }

            var propertiesList = await propertiesQuery.ToListAsync();
            return _mapper.Map<IEnumerable<GetPropertiesDTO>>(propertiesList);
        }

        public async Task<IEnumerable<GetPropertiesDTO>> GetPropertiesForOwnerAsync(int ownerId)
        {
            var properties = await _dbContext.Properties
                .Where(p => p.OwnerId == ownerId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<GetPropertiesDTO>>(properties);
        }

        public async Task<bool> UpdatePropertyAsync(int propertyId, int ownerId, GetPropertiesDTO updatedPropertyDetails)
        {
            var existingProperty = await _dbContext.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (existingProperty == null || existingProperty.OwnerId != ownerId)
            {
                return false;
            }

            _mapper.Map(updatedPropertyDetails, existingProperty);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePropertyAsync(int propertyId, int ownerId)
        {
            var propertyToDelete = await _dbContext.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (propertyToDelete == null || propertyToDelete.OwnerId != ownerId)
            {
                return false;
            }

            _dbContext.Properties.Remove(propertyToDelete);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        PropertyDetailsDTO IProperty.DisplayOwnerProperty(int ownerId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AddPropertyAsync(string ownerEmail, PropertyDetailsDTO dto)
        {
            var owner = _dbContext.Users.FirstOrDefault(o => o.Email == ownerEmail);
            if (owner == null)
                return false;

            var property = _mapper.Map<Property>(dto);
            property.OwnerId = owner.UserId;

            _dbContext.Properties.Add(property);
            var result = await _dbContext.SaveChangesAsync();
            return result > 0;
        }

        Task<PropertyDetailsDTO> IProperty.GetPropertyByIdAsync(int propertyId)
        {
            throw new NotImplementedException();
        }

        Task<IEnumerable<PropertyDetailsDTO>> IProperty.GetPropertiesForOwnerAsync(int ownerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePropertyAsync(int propertyId, int ownerId, PropertyDetailsDTO updatedPropertyDetails)
        {
            throw new NotImplementedException();
        }
    }
}
