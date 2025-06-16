using Api.Swazy.Models.DTOs.Authentication;
using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.DTOs.Businesses;
using Api.Swazy.Models.DTOs.BusinessEmployees; // Added
using Api.Swazy.Models.DTOs.Services;
using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.DTOs.BusinessServices;
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
        CreateMap<Business, GetBusinessDto>(); // This should now correctly map BusinessEmployees due to the below mapping

        // BusinessEmployee
        CreateMap<BusinessEmployee, BusinessEmployeeDto>()
            .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.User.FirstName))
            .ForMember(dest => dest.UserLastName, opt => opt.MapFrom(src => src.User.LastName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.HiredByFirstName, opt => opt.MapFrom(src => src.HiredByUser.FirstName))
            .ForMember(dest => dest.HiredByLastName, opt => opt.MapFrom(src => src.HiredByUser.LastName));

        // Service
        CreateMap<CreateServiceDto, Service>();
        CreateMap<UpdateServiceDto, Service>();

        // BusinessService
        CreateMap<BusinessService, BusinessServiceDto>().ReverseMap();
        CreateMap<BusinessService, CreateBusinessServiceDto>().ReverseMap();
        CreateMap<BusinessService, UpdateBusinessServiceDto>().ReverseMap();

        // User
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();
        CreateMap<User, GetUserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.GetName(src.Role)));

        // Token DTO
        CreateMap<User, TokenDto>();
    }
}