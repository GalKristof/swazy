using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Businesses;

public record AddEmployeeToBusinessDto(string UserEmail, BusinessRole Role);
