using RubQue.Domain.Enums;

namespace RubQue.Application.Auth.DTOs;

public record LoginRequest(string Username, string Password);

public record AuthResponse(
    long UserId,
    long ShopId,
    long? BranchId,
    string Username,
    string FullName,
    UserRole Role,
    string Token,
    DateTime ExpiresAt
);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
