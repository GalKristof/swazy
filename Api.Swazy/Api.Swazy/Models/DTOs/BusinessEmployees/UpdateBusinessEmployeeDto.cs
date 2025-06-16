using Api.Swazy.Types;
using System.ComponentModel.DataAnnotations;

namespace Api.Swazy.Models.DTOs.BusinessEmployees;

public class UpdateBusinessEmployeeDto
{
    [Required]
    public BusinessRole Role { get; set; }
}
