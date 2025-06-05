using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Models.Results;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Bookings;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Api.Swazy.Tests.Services;

public class BookingServiceTests
{
    private readonly Mock<IRepository<Booking>> _mockBookingRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly BookingService _bookingService;

    public BookingServiceTests()
    {
        _mockBookingRepository = new Mock<IRepository<Booking>>();
        _mockMapper = new Mock<IMapper>(); // Even if not used directly by this method, BookingService constructor needs it.
        _bookingService = new BookingService(_mockBookingRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task GetBookingsByBusinessIdAsync_WhenBookingsExist_ReturnsCorrectDtoList()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var service1 = new Service { Id = Guid.NewGuid(), Value = "Test Service 1" };
        var businessService1 = new BusinessService { Id = Guid.NewGuid(), BusinessId = businessId, Service = service1, Duration = 60, ServiceId = service1.Id };

        // Note: Real EF Core queries would be async. Mocking GetAll() to return IQueryable which is then processed.
        // The ToListAsync() part happens inside the service method on this IQueryable.
        var bookingsData = new List<Booking>
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                BookingDate = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
                BusinessService = businessService1,
                BusinessServiceId = businessService1.Id,
                FirstName = "John", LastName = "Doe", Email = "john@test.com", PhoneNumber = "12345", Notes = "Note1"
            },
            new Booking
            {
                Id = Guid.NewGuid(),
                BookingDate = new DateTimeOffset(2024, 1, 1, 14, 0, 0, TimeSpan.Zero),
                BusinessService = businessService1,
                BusinessServiceId = businessService1.Id,
                FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", PhoneNumber = "67890", Notes = null
            }
        };

        // Setup mock to return the IQueryable version of bookingsData
        _mockBookingRepository.Setup(r => r.GetQueryable()).Returns(bookingsData.AsQueryable());

        // Act
        var result = await _bookingService.GetBookingsByBusinessIdAsync(businessId);

        // Assert
        Assert.Equal(CommonResult.Success, result.Result);
        Assert.NotNull(result.Value);
        var resultList = result.Value.ToList();
        Assert.Equal(2, resultList.Count);

        var firstBookingEntity = bookingsData.First();
        var firstBookingDto = resultList.First(dto => dto.Id == firstBookingEntity.Id);
        Assert.Equal(firstBookingEntity.BookingDate, firstBookingDto.StartTime);
        Assert.Equal(firstBookingEntity.BookingDate.AddMinutes(businessService1.Duration), firstBookingDto.EndTime);
        Assert.Equal(service1.Value, firstBookingDto.ServiceName);
        Assert.Equal("John", firstBookingDto.FirstName);
        Assert.Equal("Note1", firstBookingDto.Notes);

        var secondBookingEntity = bookingsData.Skip(1).First();
        var secondBookingDto = resultList.First(dto => dto.Id == secondBookingEntity.Id);
        Assert.Equal(secondBookingEntity.BookingDate, secondBookingDto.StartTime);
        Assert.Equal(secondBookingEntity.BookingDate.AddMinutes(businessService1.Duration), secondBookingDto.EndTime);
        Assert.Equal(service1.Value, secondBookingDto.ServiceName);
        Assert.Equal("Jane", secondBookingDto.FirstName);
        Assert.Null(secondBookingDto.Notes);
    }

    [Fact]
    public async Task GetBookingsByBusinessIdAsync_WhenNoBookingsExist_ReturnsEmptyList()
    {
        // Arrange
        var businessId = Guid.NewGuid();
        var bookingsData = new List<Booking>();

        _mockBookingRepository.Setup(r => r.GetQueryable()).Returns(bookingsData.AsQueryable());

        // Act
        var result = await _bookingService.GetBookingsByBusinessIdAsync(businessId);

        // Assert
        Assert.Equal(CommonResult.Success, result.Result);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetBookingsByBusinessIdAsync_WhenBookingsExistForOtherBusinesses_ReturnsEmptyList()
    {
        // Arrange
        var targetBusinessId = Guid.NewGuid();
        var otherBusinessId = Guid.NewGuid();
        var service1 = new Service { Id = Guid.NewGuid(), Value = "Test Service 1" };
        var businessService1 = new BusinessService { Id = Guid.NewGuid(), BusinessId = otherBusinessId, Service = service1, Duration = 60, ServiceId = service1.Id };

        var bookingsData = new List<Booking>
        {
            new Booking
            {
                Id = Guid.NewGuid(),
                BookingDate = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero),
                BusinessService = businessService1,
                BusinessServiceId = businessService1.Id,
                FirstName = "Other", LastName = "Biz", Email = "other@test.com", PhoneNumber = "00000"
            }
        };

        _mockBookingRepository.Setup(r => r.GetQueryable()).Returns(bookingsData.AsQueryable());

        // Act
        var result = await _bookingService.GetBookingsByBusinessIdAsync(targetBusinessId);

        // Assert
        Assert.Equal(CommonResult.Success, result.Result);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
