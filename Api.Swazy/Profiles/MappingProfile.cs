using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.DTOs.Services;
using Api.Swazy.Models.DTOs.Translations;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Entities;
using AutoMapper;
namespace Api.Swazy.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Booking
        CreateMap<CreateBookingDto, Booking>();
        CreateMap<UpdateBookingDto, Booking>();
        CreateMap<Booking, GetBookingDto>();

        // Business
        CreateMap<CreateBusinessDto, Business>();
        CreateMap<UpdateBusinessDto, Business>();
        CreateMap<Business, GetBusinessDto>();

        // Service
        CreateMap<CreateServiceDto, Service>();
        CreateMap<UpdateServiceDto, Service>();

        // User
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<User, GetUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.GetName(src.Role)));

        // Token DTO
        CreateMap<User, TokenDto>();
    }
}