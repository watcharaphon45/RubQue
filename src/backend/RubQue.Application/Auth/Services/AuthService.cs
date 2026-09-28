using Microsoft.EntityFrameworkCore;
using RubQue.Application.Auth.DTOs;
using RubQue.Application.Auth.Interfaces;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;
using RubQue.Domain.Enums;

namespace RubQue.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure("กรุณาระบุชื่อผู้ใช้งานและรหัสผ่าน");
        }

        var user = await _context.Users
            .Include(u => u.Shop)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.Trim().ToLower(), cancellationToken);

        if (user == null)
        {
            return Result<AuthResponse>.Failure("ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
        }

        if (user.Status != RecordStatus.Active)
        {
            return Result<AuthResponse>.Failure("บัญชีผู้ใช้นี้ถูกระงับการใช้งาน");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Result<AuthResponse>.Failure("ชื่อผู้ใช้งานหรือรหัสผ่านไม่ถูกต้อง");
        }

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        var response = new AuthResponse(
            UserId: user.UserId,
            ShopId: user.ShopId,
            BranchId: user.BranchId,
            Username: user.Username,
            FullName: user.FullName,
            Role: user.Role,
            Token: token,
            ExpiresAt: expiresAt
        );

        return Result<AuthResponse>.Success(response, "เข้าสู่ระบบสำเร็จ");
    }
}
