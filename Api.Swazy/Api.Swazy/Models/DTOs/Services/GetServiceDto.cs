using Api.Swazy.Models.Base;
using Api.Swazy.Types;

namespace Api.Swazy.Models.DTOs.Services;

public class GetServiceDto(Guid id, string tag, BusinessType businessType, string value)
    : BaseEntity
{
    public Guid Id { get; set; } = id;
    public string Tag { get; set; } = tag;
    public BusinessType BusinessType { get; set; } = businessType;
    public string Value { get; set; } = value;
}