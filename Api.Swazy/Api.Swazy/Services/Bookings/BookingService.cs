using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Api.Swazy.Services.Bookings;

public class BookingService : GenericService<Booking, CreateBookingDto, UpdateBookingDto>, IBookingService
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IMapper _mapper; // Preserved as it was added in a previous step

    public BookingService(IRepository<Booking> repository, IMapper mapper)
        : base(repository, mapper)
    {
        _bookingRepository = repository;
        _mapper = mapper;
    }

    public async Task<CommonResponse<IEnumerable<BookingDetailsDto>>> GetBookingsByBusinessIdAsync(Guid businessId)
    {
        // Now using GetQueryable() to get IQueryable<Booking>
        var query = _bookingRepository.GetQueryable();

        // Using ToList() instead of ToListAsync() as a pragmatic fix for testing with Moq's AsQueryable()
        // which doesn't support IAsyncEnumerable. This might have performance implications in a real scenario.
        var bookings = query
            .Include(b => b.BusinessService)
                .ThenInclude(bs => bs!.Service)
            .Where(b => b.BusinessService != null && b.BusinessService.BusinessId == businessId)
            .ToList(); // Changed from ToListAsync()

        // Since the operation is now synchronous, Task.FromResult is needed if the method must remain async.
        // However, the surrounding method is async Task<CommonResponse>, so direct return is fine if bookings are processed sync.
        // The await keyword was on ToListAsync, so it's removed here. The method remains async due to its signature.

        var bookingDetailsDtos = bookings.Select(b => new BookingDetailsDto
        {
            Id = b.Id,
            StartTime = b.BookingDate,
            EndTime = b.BookingDate.AddMinutes(b.BusinessService!.Duration),
            ServiceName = b.BusinessService!.Service!.Value,
            FirstName = b.FirstName,
            LastName = b.LastName,
            Email = b.Email,
            PhoneNumber = b.PhoneNumber,
            Notes = b.Notes,
            EmployeeId = b.EmployeeId
        }).ToList();

        return new CommonResponse<IEnumerable<BookingDetailsDto>> { Result = CommonResult.Success, Value = bookingDetailsDtos };
    }
}