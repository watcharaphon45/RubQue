using System.Security.Claims;
using RubQue.Application.Common.Interfaces;
using RubQue.Domain.Enums;

namespace RubQue.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var idClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public long? ShopId
    {
        get
        {
            var shopClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("shop_id");
            return long.TryParse(shopClaim, out var id) ? id : null;
        }
    }

    public long? BranchId
    {
        get
        {
            var branchClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("branch_id");
            return long.TryParse(branchClaim, out var id) ? id : null;
        }
    }

    public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public UserRole? Role
    {
        get
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
