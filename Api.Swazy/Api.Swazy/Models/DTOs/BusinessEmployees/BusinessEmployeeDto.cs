using Api.Swazy.Types;
using System;
using System.Collections.Generic;

namespace Api.Swazy.Models.DTOs.BusinessEmployees;

public class BusinessEmployeeDto
{
    public Guid BusinessId { get; set; }
    public Guid UserId { get; set; }
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public BusinessRole Role { get; set; }
    public DateTimeOffset HiredDate { get; set; }
    public Guid HiredById { get; set; }
    public string HiredByFirstName { get; set; } = string.Empty;
    public string HiredByLastName { get; set; } = string.Empty;
}
