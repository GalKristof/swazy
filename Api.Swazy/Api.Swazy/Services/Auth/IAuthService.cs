using Api.Swazy.Models.DTOs.Users;
using Api.Swazy.Models.Results;

namespace Api.Swazy.Services.Auth;

public interface IAuthService
{
    Task<CommonResponse<string>> LoginUserAsync(LoginUserDto dto);
    Task<CommonResponse<string>> RegisterUserAsync(RegisterUserDto dto);
}