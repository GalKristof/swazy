﻿namespace Api.Swazy.Models.DTOs.Bookings;

public class GetBookingDto(
    Guid Id,
    DateTimeOffset BookingDate,
    string? Notes,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid BusinessServiceId,
    Guid? EmployeeId);