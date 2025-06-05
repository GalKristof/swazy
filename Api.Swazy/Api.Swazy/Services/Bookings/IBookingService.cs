using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Services.Generic;
using Api.Swazy.Models.Results;

namespace Api.Swazy.Services.Bookings;

public interface IBookingService : IGenericService<Booking, CreateBookingDto, UpdateBookingDto>
{
    Task<CommonResponse<IEnumerable<BookingDetailsDto>>> GetBookingsByBusinessIdAsync(Guid businessId);
}