using Api.Swazy.Models.Base;

namespace Api.Swazy.Models.Entities;

public class EmployeeDaySchedule : BaseEntity
{
    public Guid EmployeeWeeklyScheduleId { get; set; }
    public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
    public bool IsWorkingDay { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public virtual EmployeeWeeklySchedule EmployeeWeeklySchedule { get; set; } = null!;
}
