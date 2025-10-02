using Api.Swazy.Models.DTOs.EmployeeSchedule;

namespace Api.Swazy.Models.Responses;

public record EmployeeScheduleResponse(
    Guid Id,
    Guid UserId,
    Guid BusinessId,
    int BufferTimeMinutes,
    DateTimeOffset? VacationFrom,
    DateTimeOffset? VacationTo,
    List<DayScheduleDto> DaySchedules,
    DateTimeOffset CreatedAt
);
