using AutoMapper;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;
using RenatalPropertyManagement.Models.DTO;

namespace EazyRent.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {

            CreateMap<Property, PropertyDetailsDTO>().ReverseMap()
             .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<RegistrationDTO, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));
            CreateMap<Payment, PaymentDTO>().ReverseMap();
            CreateMap<MaintenanceRequest, MaintenanceRequestDto>().ReverseMap();

        }

    }
}
