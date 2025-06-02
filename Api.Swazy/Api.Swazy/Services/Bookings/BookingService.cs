using Api.Swazy.Models.DTOs.Bookings;
using Api.Swazy.Models.Entities;
using Api.Swazy.Persistence.Repositories;
using Api.Swazy.Services.Generic;
using AutoMapper;

namespace Api.Swazy.Services.Bookings;

public class BookingService(
    IRepository<Booking> repository,
    IMapper mapper)
    : GenericService<Booking, CreateBookingDto, UpdateBookingDto>(repository, mapper), IBookingService;