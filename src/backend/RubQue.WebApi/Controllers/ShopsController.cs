using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;
using RubQue.Domain.Enums;

namespace RubQue.WebApi.Controllers;

public class ShopsController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public ShopsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ดึงข้อมูลร้านและสาขา
    /// </summary>
    [HttpGet("info")]
    public async Task<IActionResult> GetShopInfo(CancellationToken cancellationToken)
    {
        var shop = await _context.Shops
            .Include(s => s.Branches)
                .ThenInclude(b => b.BusinessHours)
            .FirstOrDefaultAsync(s => s.Status == RecordStatus.Active, cancellationToken);

        if (shop == null)
        {
            return NotFound(Result.Failure("ไม่พบข้อมูลร้าน"));
        }

        var data = new
        {
            shop.ShopId,
            shop.ShopName,
            shop.Logo,
            shop.Description,
            shop.Phone,
            shop.Email,
            shop.Address,
            Branches = shop.Branches.Select(b => new
            {
                b.BranchId,
                b.BranchName,
                b.Phone,
                b.Address,
                b.Latitude,
                b.Longitude,
                BusinessHours = b.BusinessHours.OrderBy(bh => bh.DayOfWeek).Select(bh => new
                {
                    bh.DayOfWeek,
                    DayName = bh.DayOfWeek.ToString(),
                    OpenTime = bh.OpenTime.ToString("HH:mm"),
                    CloseTime = bh.CloseTime.ToString("HH:mm"),
                    bh.IsClosed
                })
            })
        };

        return Ok(Result<object>.Success(data));
    }
}
