using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Services.Generic;

namespace Api.Swazy.Services.Bookings;

public interface IBookingService : IGenericService<Booking, CreateBookingDto, UpdateBookingDto>;