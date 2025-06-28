using AutoMapper;
using EazyRent.Models.Domains;
using EazyRent.Models.DTO;

namespace EazyRent.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {

            CreateMap<Property, GetPropertiesDTO>()
                .ForMember(dest => dest.PropertyImageBase64, opt => opt.MapFrom(src =>
                    src.PropertyImage != null
                        ? $"data:image/png;base64,{Convert.ToBase64String(src.PropertyImage)}"
                        : null))
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<RegistrationDTO, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)));
            CreateMap<Payment, PaymentDTO>().ReverseMap();
            CreateMap<MaintenanceRequest, MaintenanceRequestDto>()
                .ForMember(dest => dest.TenantFullName, opt => opt.MapFrom(src => src.Tenant != null ? src.Tenant.FullName : null))
                .ReverseMap();

            CreateMap<PropertyDetailsDTO, Property>()
             .ForMember(dest => dest.PropertyImage, opt => opt.MapFrom(src =>
             src.PropertyImage != null ? ConvertFormFileToByteArray(src.PropertyImage) : null
             ))
             .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Property, PropertyDetailsDTO>()
                .ForMember(dest => dest.PropertyImage, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Lease, GetLeaseDetailsDTO>()
                .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.FullName));

            CreateMap<CreateLeaseDTO, Lease>()
                .ForMember(dest => dest.DigitalSignature, opt => opt.MapFrom(src =>
                    src.DigitalSignature != null ? ConvertFormFileToByteArray(src.DigitalSignature) : null
                ))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }

        private static byte[] ConvertFormFileToByteArray(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }
    }




}
