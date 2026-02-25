using System.Security.Claims;
using HRMS.Application.DTOs.Auth;
using HRMS.Application.Interfaces;
using HRMS.Shared.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        var result = await _authService.RegisterAsync(dto, performedBy);
        return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Employee registered successfully."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(dto, ip);
        return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Login successful."));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return Ok(ApiResponse<TokenResponseDto>.Ok(result, "Token refreshed."));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        await _authService.LogoutAsync(userId, dto.RefreshToken);
        return Ok(ApiResponse<object>.Ok(null!, "Logged out successfully."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(ApiResponse<object>.Ok(null!, "Password changed successfully."));
    }

    [HttpPost("reset-password")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var performedBy = User.FindFirst(ClaimTypes.Email)?.Value ?? "system";
        await _authService.ResetPasswordAsync(dto, performedBy);
        return Ok(ApiResponse<object>.Ok(null!, "Password reset successfully."));
    }
}
