namespace Api.Swazy.Models.Responses;

public record BusinessResponse(
    Guid Id,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    string BusinessType,
    List<BusinessEmployeeResponse> Employees,
    List<BusinessServiceResponse> Services,
    string WebsiteUrl,
    DateTimeOffset CreatedAt
);