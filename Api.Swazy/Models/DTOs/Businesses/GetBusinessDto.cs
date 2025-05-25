using Api.Swazy.Services.Businesses;
using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record GetBusinessDto(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    BusinessType BusinessType,
    Dictionary<Guid, BusinessRole> Employees,
    string WebsiteUrl,
    List<BusinessService> Services);