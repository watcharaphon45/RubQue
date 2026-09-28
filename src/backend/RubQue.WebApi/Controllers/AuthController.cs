using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubQue.Application.Auth.DTOs;
using RubQue.Application.Auth.Interfaces;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;

namespace RubQue.WebApi.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _context;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        IApplicationDbContext context)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _context = context;
    }

    /// <summary>
    /// เข้าสู่ระบบสำหรับ Admin / Staff
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// ตรวจสอบข้อมูลผู้ใช้งานปัจจุบันจาก JWT Token
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            return Unauthorized(Result.Failure("ไม่พบข้อมูลผู้ใช้งาน"));
        }

        var user = await _context.Users
            .Include(u => u.Shop)
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.UserId == userId.Value, cancellationToken);

        if (user == null)
        {
            return NotFound(Result.Failure("ไม่พบผู้ใช้งานในระบบ"));
        }

        var data = new
        {
            user.UserId,
            user.ShopId,
            ShopName = user.Shop?.ShopName,
            user.BranchId,
            BranchName = user.Branch?.BranchName,
            user.Username,
            user.FullName,
            user.Email,
            user.Phone,
            Role = user.Role.ToString()
        };

        return Ok(Result<object>.Success(data));
    }
}
