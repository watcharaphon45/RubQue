using RubQue.Domain.Enums;

namespace RubQue.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    long? ShopId { get; }
    long? BranchId { get; }
    string? Username { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
