using HRMS.Application.DTOs.Auth;

namespace HRMS.Application.Interfaces;

public interface IAuthService
{
    Task<TokenResponseDto> RegisterAsync(RegisterDto dto, string performedBy);
    Task<TokenResponseDto> LoginAsync(LoginDto dto, string? ipAddress);
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task LogoutAsync(string userId, string refreshToken);
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
}
