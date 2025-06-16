using Api.Swazy.Models.DTOs.BusinessEmployees;
using Api.Swazy.Services.Businesses;
using Api.Swazy.Types;
using System.Collections.Generic;

namespace Api.Swazy.Models.DTOs.Businesses;

public record GetBusinessDto(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    BusinessType BusinessType,
    List<BusinessEmployeeDto> BusinessEmployees,
    string WebsiteUrl,
    List<BusinessService> Services);