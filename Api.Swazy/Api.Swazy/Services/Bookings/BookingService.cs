using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Generic;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Api.Swazy.Persistence.UoW; // Added for unitOfWork
using Serilog; // Added for logging

namespace Api.Swazy.Services.Bookings;

public class BookingService : GenericService<Booking, CreateBookingDto, UpdateBookingDto>, IBookingService
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IMapper _mapper;
    private readonly IRepository<BusinessEmployee> _businessEmployeeRepository; // Added
    private readonly IRepository<BusinessService> _businessServiceRepository; // Added

    public BookingService(
        IRepository<Booking> repository,
        IMapper mapper,
        IRepository<BusinessEmployee> businessEmployeeRepository, // Added
        IRepository<BusinessService> businessServiceRepository) // Added
        : base(repository, mapper)
    {
        _bookingRepository = repository;
        _mapper = mapper;
        _businessEmployeeRepository = businessEmployeeRepository; // Added
        _businessServiceRepository = businessServiceRepository; // Added
    }

    public override async Task<CommonResponse<Booking?>> CreateEntityAsync(CreateBookingDto dto, IUnitOfWork? unitOfWork = null)
    {
        Log.Verbose("[BookingService - {MethodName}] Attempting to create booking.", nameof(CreateEntityAsync));
        var response = new CommonResponse<Booking?>();

        if (dto.EmployeeId.HasValue)
        {
            var businessService = await _businessServiceRepository.GetByIdAsync(dto.BusinessServiceId);
            if (businessService == null)
            {
                response.Result = CommonResult.RequirementNotFound;
                Log.Warning("[BookingService - {MethodName}] BusinessService with ID {BusinessServiceId} not found.", nameof(CreateEntityAsync), dto.BusinessServiceId);
                return response;
            }

            bool isEmployeeOfBusiness = await _businessEmployeeRepository.GetQueryable()
                .AnyAsync(be => be.BusinessId == businessService.BusinessId && be.UserId == dto.EmployeeId.Value && !be.IsDeleted);

            if (!isEmployeeOfBusiness)
            {
                response.Result = CommonResult.ValidationError;
                // It's good practice to add a message, but CommonResponse doesn't have it.
                // For now, log will be the primary source of detailed error info.
                Log.Warning("[BookingService - {MethodName}] Employee {EmployeeId} is not a valid employee of Business {BusinessId}.",
                    nameof(CreateEntityAsync), dto.EmployeeId.Value, businessService.BusinessId);
                return response;
            }
        }

        Log.Verbose("[BookingService - {MethodName}] Employee validation passed or not applicable. Proceeding with base creation.", nameof(CreateEntityAsync));
        return await base.CreateEntityAsync(dto, unitOfWork);
    }

    public override async Task<CommonResponse<Booking?>> UpdateEntityAsync(UpdateBookingDto dto, IUnitOfWork? unitOfWork = null)
    {
        Log.Verbose("[BookingService - {MethodName}] Attempting to update booking with ID {BookingId}.", nameof(UpdateEntityAsync), dto.Id);
        var response = new CommonResponse<Booking?>();

        // Fetch existing entity to ensure it exists before validation
        var existingBooking = await _bookingRepository.GetByIdAsync(dto.Id);
        if (existingBooking == null)
        {
            response.Result = CommonResult.NotFound;
            Log.Warning("[BookingService - {MethodName}] Booking with ID {BookingId} not found for update.", nameof(UpdateEntityAsync), dto.Id);
            return response;
        }

        if (dto.EmployeeId.HasValue)
        {
            var businessService = await _businessServiceRepository.GetByIdAsync(dto.BusinessServiceId);
            if (businessService == null)
            {
                response.Result = CommonResult.RequirementNotFound;
                Log.Warning("[BookingService - {MethodName}] BusinessService with ID {BusinessServiceId} not found.", nameof(UpdateEntityAsync), dto.BusinessServiceId);
                return response;
            }

            bool isEmployeeOfBusiness = await _businessEmployeeRepository.GetQueryable()
                .AnyAsync(be => be.BusinessId == businessService.BusinessId && be.UserId == dto.EmployeeId.Value && !be.IsDeleted);

            if (!isEmployeeOfBusiness)
            {
                response.Result = CommonResult.ValidationError;
                Log.Warning("[BookingService - {MethodName}] Employee {EmployeeId} is not a valid employee of Business {BusinessId} for booking update.",
                    nameof(UpdateEntityAsync), dto.EmployeeId.Value, businessService.BusinessId);
                return response;
            }
        }

        Log.Verbose("[BookingService - {MethodName}] Employee validation passed or not applicable for booking update. Proceeding with base update.", nameof(UpdateEntityAsync));
        return await base.UpdateEntityAsync(dto, unitOfWork);
    }

    public async Task<CommonResponse<IEnumerable<BookingDetailsDto>>> GetBookingsByBusinessIdAsync(Guid businessId)
    {
        var query = _bookingRepository.GetQueryable();

        // Using ToList() instead of ToListAsync() as a pragmatic fix for testing with Moq's AsQueryable()
        // which doesn't support IAsyncEnumerable. This might have performance implications in a real scenario.
        var bookings = query
            .Include(b => b.BusinessService)
                .ThenInclude(bs => bs!.Service)
            .Where(b => b.BusinessService != null && b.BusinessService.BusinessId == businessId);

        var bookingEntities = await bookings.ToListAsync(); // Keep it async

        var bookingDetailsDtos = bookingEntities.Select(b => new BookingDetailsDto
        {
            Id = b.Id,
            StartTime = b.BookingDate,
            EndTime = b.BookingDate.AddMinutes(b.BusinessService!.Duration), // Assuming Duration is on BusinessService
            ServiceName = b.BusinessService!.Service!.Value, // Assuming Service has a Value property for name
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