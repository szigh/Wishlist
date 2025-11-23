using AutoMapper;
using Contracts.DTOs;
using Models;

namespace WishlistWeb
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserReadDto>();
            CreateMap<User, UserWishlistReadDto>();
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(_ => "user"))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // handled manually
            CreateMap<UserUpdateDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // never overwrite password
                .ForMember(dest => dest.Id, opt => opt.Ignore());          // Id comes from route, not DTO

            // Gift mappings
            CreateMap<Gift, GiftReadDto>();
            CreateMap<GiftCreateDto, Gift>();
            CreateMap<GiftUpdateDto, Gift>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore());

            // Volunteer mappings
            CreateMap<Volunteer, VolunteerReadDto>();
            CreateMap<VolunteerCreateDto, Volunteer>();
        }
    }
}
