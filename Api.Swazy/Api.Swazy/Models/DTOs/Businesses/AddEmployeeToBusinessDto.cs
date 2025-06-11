using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record AddEmployeeToBusinessDto(Guid BusinessId, string UserEmail, BusinessRole Role);
