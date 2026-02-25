using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRMS.Application.DTOs.Auth;
using HRMS.Application.Interfaces;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;
using HRMS.Domain.Interfaces;
using HRMS.Shared.Exceptions;
using HRMS.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HRMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IAuditService _auditService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IEmailService emailService,
        IAuditService auditService)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _emailService = emailService;
        _auditService = auditService;
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto dto, string performedBy)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new ConflictException("A user with this email already exists.");

        var year = dto.JoiningDate.Year;
        var seq = await _unitOfWork.Employees.GetNextSequenceForYearAsync(year);
        var code = StringHelper.GenerateEmployeeCode(year, seq);

        var dept = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId);
        if (dept == null || dept.IsDeleted) throw new NotFoundException("Department", dto.DepartmentId);

        var desig = await _unitOfWork.Designations.GetByIdAsync(dto.DesignationId);
        if (desig == null || desig.IsDeleted) throw new NotFoundException("Designation", dto.DesignationId);

        var user = new ApplicationUser
        {
            UserName = dto.Email, Email = dto.Email,
            EmailConfirmed = true, IsActive = true, CreatedAt = DateTime.UtcNow
        };

        var pwd = string.IsNullOrEmpty(dto.Password) ? StringHelper.GenerateRandomPassword() : dto.Password;
        var result = await _userManager.CreateAsync(user, pwd);
        if (!result.Succeeded)
            throw new Shared.Exceptions.ValidationException(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));

        await _userManager.AddToRoleAsync(user, dto.Role);

        var emp = new Employee
        {
            EmployeeCode = code, FirstName = dto.FirstName, LastName = dto.LastName,
            Email = dto.Email, Phone = dto.Phone, DateOfBirth = dto.DateOfBirth,
            Gender = (Gender)dto.Gender, Address = dto.Address,
            EmergencyContact = dto.EmergencyContact, DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId, EmploymentType = (EmploymentType)dto.EmploymentType,
            JoiningDate = dto.JoiningDate, IsActive = true, UserId = user.Id,
            CreatedBy = performedBy, UpdatedBy = performedBy
        };

        await _unitOfWork.Employees.AddAsync(emp);
        user.EmployeeId = emp.Id;
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync("Employee", emp.Id.ToString(), AuditAction.Created, performedBy);
        _ = _emailService.SendWelcomeEmailAsync(dto.Email, $"{dto.FirstName} {dto.LastName}", pwd);

        return await GenerateTokenResponseAsync(user);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto, string? ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedException("Invalid email or password.");

        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedException("Account is temporarily locked.");

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedException("Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync("User", user.Id, AuditAction.LoginSuccess, user.Id, ipAddress: ipAddress);

        return await GenerateTokenResponseAsync(user);
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var stored = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (stored == null) throw new UnauthorizedException("Invalid or expired refresh token.");

        if (stored.IsUsed)
        {
            var all = await _unitOfWork.RefreshTokens.GetByUserIdAsync(stored.UserId);
            foreach (var t in all) { t.IsRevoked = true; t.RevokedReason = "Token reuse detected"; }
            _unitOfWork.RefreshTokens.UpdateRange(all);
            await _unitOfWork.SaveChangesAsync();
            throw new UnauthorizedException("Token reuse detected. All sessions revoked.");
        }

        stored.IsUsed = true;
        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user == null || !user.IsActive) throw new UnauthorizedException("User not found or inactive.");

        var response = await GenerateTokenResponseAsync(user);
        stored.ReplacedByToken = response.RefreshToken;
        _unitOfWork.RefreshTokens.Update(stored);
        await _unitOfWork.SaveChangesAsync();
        return response;
    }

    public async Task LogoutAsync(string userId, string refreshToken)
    {
        var stored = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (stored != null && stored.UserId == userId)
        {
            stored.IsRevoked = true;
            stored.RevokedReason = "User logged out";
            _unitOfWork.RefreshTokens.Update(stored);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new NotFoundException("User", userId);

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new Shared.Exceptions.ValidationException(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));

        await _auditService.LogAsync("User", userId, AuditAction.PasswordChanged, userId);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto, string performedBy)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null) throw new NotFoundException("User", dto.UserId);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        if (!result.Succeeded)
            throw new Shared.Exceptions.ValidationException(
                result.Errors.ToDictionary(e => e.Code, e => new[] { e.Description }));

        await _auditService.LogAsync("User", dto.UserId, AuditAction.PasswordReset, performedBy);
        _ = _emailService.SendPasswordResetEmailAsync(user.Email!, user.UserName!, dto.NewPassword);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive) return; // Silent return to prevent email enumeration

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var newPwd = StringHelper.GenerateRandomPassword();
        var result = await _userManager.ResetPasswordAsync(user, token, newPwd);
        
        if (result.Succeeded)
        {
            await _auditService.LogAsync("User", user.Id, AuditAction.PasswordReset, "System-ForgotPassword");
            _ = _emailService.SendPasswordResetEmailAsync(user.Email!, user.UserName!, newPwd);
        }
    }

    private async Task<TokenResponseDto> GenerateTokenResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Employee";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (user.EmployeeId.HasValue) claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var mins = int.Parse(_configuration["JWT:AccessTokenExpiryMinutes"] ?? "30");

        var jwt = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"], audience: _configuration["JWT:Audience"],
            claims: claims, expires: DateTime.UtcNow.AddMinutes(mins), signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshStr = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var days = int.Parse(_configuration["JWT:RefreshTokenExpiryDays"] ?? "7");

        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(), Token = refreshStr, UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(days), CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.RefreshTokens.AddAsync(refreshEntity);
        await _unitOfWork.SaveChangesAsync();

        return new TokenResponseDto
        {
            AccessToken = accessToken, RefreshToken = refreshStr,
            AccessTokenExpiry = jwt.ValidTo, UserId = user.Id,
            Email = user.Email!, Role = role, EmployeeId = user.EmployeeId
        };
    }
}
