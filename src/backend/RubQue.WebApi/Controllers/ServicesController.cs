using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;
using RubQue.Domain.Enums;

namespace RubQue.WebApi.Controllers;

public class ServicesController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public ServicesController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ดึงรายการบริการทั้งหมดแยกตามหมวดหมู่
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetServices(CancellationToken cancellationToken)
    {
        var categories = await _context.ServiceCategories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new
            {
                c.CategoryId,
                c.CategoryName,
                c.DisplayOrder,
                Services = c.Services
                    .Where(s => s.Status == RecordStatus.Active)
                    .Select(s => new
                    {
                        s.ServiceId,
                        s.ServiceName,
                        s.Description,
                        s.DurationMinutes,
                        s.Price,
                        s.DepositAmount
                    }).ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(Result<object>.Success(categories));
    }
}
