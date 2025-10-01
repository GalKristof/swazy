namespace Api.Swazy.Models.Responses;

public record AvailableTimeSlotsResponse(
    DateTimeOffset Date,
    List<DateTimeOffset> AvailableSlots
);
