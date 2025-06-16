using Api.Swazy.Types;
using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Swazy.Models.DTOs.BusinessEmployees;

public class CreateBusinessEmployeeDto
{
    [Required]
    public Guid BusinessId { get; set; }

    [Required]
    [EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public BusinessRole Role { get; set; }
}
