using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubQue.Application.Booking.DTOs;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;
using RubQue.Domain.Entities;
using RubQue.Domain.Enums;

namespace RubQue.WebApi.Controllers;

public class BookingsController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public BookingsController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ดึงรายการ Booking ทั้งหมด พร้อม Filter
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBookings(
        [FromQuery] long? branchId,
        [FromQuery] string? date,
        [FromQuery] BookingStatus? status,
        [FromQuery] long? employeeId,
        CancellationToken cancellationToken)
    {
        var query = _context.Bookings
            .Include(b => b.Branch)
            .Include(b => b.Customer)
            .Include(b => b.Employee)
            .Include(b => b.Items)
            .AsQueryable();

        if (branchId.HasValue) query = query.Where(b => b.BranchId == branchId.Value);
        if (employeeId.HasValue) query = query.Where(b => b.EmployeeId == employeeId.Value);
        if (status.HasValue) query = query.Where(b => b.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(date) && DateOnly.TryParse(date, out var parsedDate))
        {
            query = query.Where(b => b.BookingDate == parsedDate);
        }

        var bookings = await query
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.StartTime)
            .Select(b => new BookingDto(
                b.BookingId,
                b.BookingNo,
                b.ShopId,
                b.BranchId,
                b.Branch != null ? b.Branch.BranchName : "",
                b.CustomerId,
                b.Customer != null ? b.Customer.Name : "",
                b.Customer != null ? (b.Customer.Phone ?? "") : "",
                b.EmployeeId,
                b.Employee != null ? b.Employee.Nickname ?? b.Employee.Name : "ไม่ระบุช่าง",
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.Status.ToString(),
                b.TotalAmount,
                b.DepositAmount,
                b.Note,
                b.Items.Select(i => new BookingItemDto(i.ServiceId, i.ServiceName, i.Price, i.DurationMinutes)).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Ok(Result<List<BookingDto>>.Success(bookings));
    }

    /// <summary>
    /// สรุปคิววันนี้สำหรับ Dashboard
    /// </summary>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodaySummary([FromQuery] long? branchId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Employee)
            .Include(b => b.Items)
            .Where(b => b.BookingDate == today);

        if (branchId.HasValue)
        {
            query = query.Where(b => b.BranchId == branchId.Value);
        }

        var todayList = await query
            .OrderBy(b => b.StartTime)
            .Select(b => new BookingDto(
                b.BookingId,
                b.BookingNo,
                b.ShopId,
                b.BranchId,
                b.Branch != null ? b.Branch.BranchName : "",
                b.CustomerId,
                b.Customer != null ? b.Customer.Name : "",
                b.Customer != null ? (b.Customer.Phone ?? "") : "",
                b.EmployeeId,
                b.Employee != null ? b.Employee.Nickname ?? b.Employee.Name : "ไม่ระบุช่าง",
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.Status.ToString(),
                b.TotalAmount,
                b.DepositAmount,
                b.Note,
                b.Items.Select(i => new BookingItemDto(i.ServiceId, i.ServiceName, i.Price, i.DurationMinutes)).ToList()
            ))
            .ToListAsync(cancellationToken);

        var summary = new
        {
            TodayDate = today.ToString("yyyy-MM-dd"),
            Total = todayList.Count,
            Pending = todayList.Count(x => x.Status == BookingStatus.Pending),
            Confirmed = todayList.Count(x => x.Status == BookingStatus.Confirmed),
            CheckedIn = todayList.Count(x => x.Status == BookingStatus.CheckedIn),
            InService = todayList.Count(x => x.Status == BookingStatus.InService),
            Completed = todayList.Count(x => x.Status == BookingStatus.Completed),
            Cancelled = todayList.Count(x => x.Status == BookingStatus.Cancelled),
            TotalRevenue = todayList.Where(x => x.Status == BookingStatus.Completed).Sum(x => x.TotalAmount),
            Bookings = todayList
        };

        return Ok(Result<object>.Success(summary));
    }

    /// <summary>
    /// ค้นหาคิวของลูกค้าด้วยเบอร์โทรศัพท์ (สำหรับหน้า คิวของฉัน ใน LIFF)
    /// </summary>
    [HttpGet("my-queue")]
    public async Task<IActionResult> GetMyQueue([FromQuery] string phone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return BadRequest(Result.Failure("กรุณาระบุเบอร์โทรศัพท์"));
        }

        var cleanPhone = phone.Trim().Replace("-", "");

        var bookings = await _context.Bookings
            .Include(b => b.Branch)
            .Include(b => b.Customer)
            .Include(b => b.Employee)
            .Include(b => b.Items)
            .Where(b => b.Customer != null && b.Customer.Phone != null && b.Customer.Phone.Replace("-", "") == cleanPhone)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.StartTime)
            .Select(b => new BookingDto(
                b.BookingId,
                b.BookingNo,
                b.ShopId,
                b.BranchId,
                b.Branch != null ? b.Branch.BranchName : "",
                b.CustomerId,
                b.Customer != null ? b.Customer.Name : "",
                b.Customer != null ? (b.Customer.Phone ?? "") : "",
                b.EmployeeId,
                b.Employee != null ? b.Employee.Nickname ?? b.Employee.Name : "ไม่ระบุช่าง",
                b.BookingDate,
                b.StartTime,
                b.EndTime,
                b.Status,
                b.Status.ToString(),
                b.TotalAmount,
                b.DepositAmount,
                b.Note,
                b.Items.Select(i => new BookingItemDto(i.ServiceId, i.ServiceName, i.Price, i.DurationMinutes)).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Ok(Result<List<BookingDto>>.Success(bookings));
    }

    /// <summary>
    /// สร้างการจองคิวใหม่ (Customer LIFF & Admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName) || string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            return BadRequest(Result.Failure("กรุณาระบุชื่อและเบอร์โทรศัพท์ของลูกค้า"));
        }

        var service = await _context.Services.FindAsync(new object[] { request.ServiceId }, cancellationToken);
        if (service == null)
        {
            return NotFound(Result.Failure("ไม่พบบริการที่เลือก"));
        }

        // 1. Find or create Customer
        var cleanPhone = request.CustomerPhone.Trim();
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Phone == cleanPhone, cancellationToken);

        if (customer == null)
        {
            customer = new Customer
            {
                Name = request.CustomerName.Trim(),
                Phone = cleanPhone
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else
        {
            customer.Name = request.CustomerName.Trim();
        }

        // 2. Calculate EndTime based on service duration
        var endTime = request.StartTime.AddMinutes(service.DurationMinutes);

        // 3. Concurrency check: check if employee or slot is already booked for overlapping time
        if (request.EmployeeId.HasValue && request.EmployeeId.Value > 0)
        {
            var isOverlapping = await _context.Bookings.AnyAsync(b =>
                b.BranchId == request.BranchId &&
                b.EmployeeId == request.EmployeeId.Value &&
                b.BookingDate == request.BookingDate &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.NoShow &&
                ((request.StartTime >= b.StartTime && request.StartTime < b.EndTime) ||
                 (endTime > b.StartTime && endTime <= b.EndTime) ||
                 (request.StartTime <= b.StartTime && endTime >= b.EndTime)),
                cancellationToken);

            if (isOverlapping)
            {
                return Conflict(Result.Failure("ช่วงเวลานี้ช่างมีคิวอื่นจองแล้ว กรุณาเลือกช่วงเวลาอื่น"));
            }
        }

        // 4. Generate unique Booking No: BK + YYYYMMDD + 4 random digits
        var randomSuffix = new Random().Next(1000, 9999);
        var bookingNo = $"BK{request.BookingDate:yyyyMMdd}{randomSuffix}";

        var booking = new Booking
        {
            BookingNo = bookingNo,
            ShopId = request.ShopId > 0 ? request.ShopId : 1,
            BranchId = request.BranchId > 0 ? request.BranchId : 1,
            CustomerId = customer.CustomerId,
            EmployeeId = request.EmployeeId > 0 ? request.EmployeeId : null,
            BookingDate = request.BookingDate,
            StartTime = request.StartTime,
            EndTime = endTime,
            Status = BookingStatus.Confirmed,
            TotalAmount = service.Price,
            DepositAmount = service.DepositAmount,
            Note = request.Note,
            Items = new List<BookingItem>
            {
                new BookingItem
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    Price = service.Price,
                    DurationMinutes = service.DurationMinutes
                }
            }
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(cancellationToken);

        // Simulated LINE Notification Log
        _context.NotificationLogs.Add(new NotificationLog
        {
            CustomerId = customer.CustomerId,
            BookingId = booking.BookingId,
            Channel = "LINE",
            Status = "Success",
            Message = $"ยืนยันการจองคิว {booking.BookingNo} เรียบร้อยแล้ว วันที่ {booking.BookingDate:dd/MM/yyyy} เวลา {booking.StartTime:HH:mm} น."
        });
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(Result<object>.Success(new
        {
            booking.BookingId,
            booking.BookingNo,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            BookingDate = booking.BookingDate.ToString("yyyy-MM-dd"),
            StartTime = booking.StartTime.ToString("HH:mm"),
            EndTime = booking.EndTime.ToString("HH:mm"),
            ServiceName = service.ServiceName,
            DurationMinutes = service.DurationMinutes,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status.ToString()
        }, "จองคิวสำเร็จ!"));
    }

    /// <summary>
    /// อัปเดตสถานะคิว (Admin / Staff)
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings.FindAsync(new object[] { id }, cancellationToken);
        if (booking == null)
        {
            return NotFound(Result.Failure("ไม่พบคิวที่ต้องการอัปเดต"));
        }

        booking.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(Result<object>.Success(new { booking.BookingId, Status = booking.Status.ToString() }, $"อัปเดตสถานะเป็น {booking.Status} สำเร็จ"));
    }
}
