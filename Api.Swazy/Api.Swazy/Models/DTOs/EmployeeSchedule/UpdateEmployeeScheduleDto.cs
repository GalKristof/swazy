namespace Api.Swazy.Models.DTOs.EmployeeSchedule;

public record UpdateEmployeeScheduleDto(
    Guid Id,
    int BufferTimeMinutes,
    bool IsOnVacation,
    List<DayScheduleDto> DaySchedules
);
