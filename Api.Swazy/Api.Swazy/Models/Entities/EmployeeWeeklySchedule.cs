using Api.Swazy.Models.Base;

namespace Api.Swazy.Models.Entities;

public class EmployeeWeeklySchedule : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid BusinessId { get; set; }
    public int BufferTimeMinutes { get; set; } = 15;
    public bool IsOnVacation { get; set; } = false;

    public virtual User User { get; set; } = null!;
    public virtual Business Business { get; set; } = null!;
    public virtual List<EmployeeDaySchedule> DaySchedules { get; set; } = new();
}
