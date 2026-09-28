using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;
using RubQue.Domain.Enums;

namespace RubQue.WebApi.Controllers;

public class EmployeesController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public EmployeesController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ดึงรายชื่อช่างตัดผม / พนักงานประจำสาขา
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] long? branchId, CancellationToken cancellationToken)
    {
        var query = _context.Employees
            .Where(e => e.Status == RecordStatus.Active);

        if (branchId.HasValue)
        {
            query = query.Where(e => e.BranchId == branchId.Value);
        }

        var employees = await query
            .Select(e => new
            {
                e.EmployeeId,
                e.BranchId,
                e.EmployeeCode,
                e.Name,
                e.Nickname,
                e.Position,
                e.ProfileImage,
                Services = e.EmployeeServices.Select(es => new
                {
                    es.ServiceId,
                    ServiceName = es.Service != null ? es.Service.ServiceName : string.Empty
                }),
                Schedules = e.EmployeeSchedules.OrderBy(s => s.DayOfWeek).Select(s => new
                {
                    s.DayOfWeek,
                    DayName = s.DayOfWeek.ToString(),
                    StartTime = s.StartTime.ToString("HH:mm"),
                    EndTime = s.EndTime.ToString("HH:mm"),
                    s.IsOff
                })
            })
            .ToListAsync(cancellationToken);

        return Ok(Result<object>.Success(employees));
    }
}
