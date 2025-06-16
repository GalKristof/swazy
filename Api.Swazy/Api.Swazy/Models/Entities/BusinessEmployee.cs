using Api.Swazy.Models.Base;
using Api.Swazy.Types;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Swazy.Models.Entities;

public class BusinessEmployee : BaseEntity
{
    public Guid BusinessId { get; set; }
    public virtual Business? Business { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public BusinessRole Role { get; set; }

    public DateTimeOffset HiredDate { get; set; }

    public Guid HiredBy { get; set; } // UserId of the user who hired this employee

    [ForeignKey("HiredBy")]
    public virtual User? HiredByUser { get; set; }
}
