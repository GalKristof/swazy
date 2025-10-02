namespace Api.Swazy.Models.DTOs.EmployeeSchedule;

public record UpdateEmployeeScheduleDto(
    Guid Id,
    int BufferTimeMinutes,
    DateTimeOffset? VacationFrom,
    DateTimeOffset? VacationTo,
    List<DayScheduleDto> DaySchedules
);
