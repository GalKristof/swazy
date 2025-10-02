namespace Api.Swazy.Models.DTOs.EmployeeSchedule;

public record CreateEmployeeScheduleDto(
    Guid UserId,
    Guid BusinessId,
    int BufferTimeMinutes,
    DateTimeOffset? VacationFrom,
    DateTimeOffset? VacationTo,
    List<DayScheduleDto> DaySchedules
);
