namespace Api.Swazy.Models.DTOs.BusinessServices
{
    public record UpdateBusinessServiceDto(
        Guid Id,
        decimal? Price,
        int? Duration
    ) : BaseUpdateDto(Id);
}
