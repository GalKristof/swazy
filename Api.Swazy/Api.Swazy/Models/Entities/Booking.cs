using Api.Swazy.Models.Base;

namespace Api.Swazy.Models.Entities;

public class Booking : BaseEntity
{
    public string ConfirmationCode { get; set; } = string.Empty;
    public DateTimeOffset BookingDate { get; set; }
    public string? Notes { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public Guid BusinessServiceId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? BookedByUserId { get; set; }

    public virtual BusinessService BusinessService { get; set; }
    public virtual User? Employee { get; set; }
    public virtual User? BookedByUser { get; set; }
}
