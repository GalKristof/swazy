namespace Api.Swazy.Models.DTOs.EmployeeSchedule;

public record CreateEmployeeScheduleDto(
    Guid UserId,
    Guid BusinessId,
    int BufferTimeMinutes,
    bool IsOnVacation,
    List<DayScheduleDto> DaySchedules
);
