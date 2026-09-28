using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubQue.Application.Booking.DTOs;
using RubQue.Application.Common.Interfaces;
using RubQue.Application.Common.Models;
using RubQue.Domain.Enums;

namespace RubQue.WebApi.Controllers;

public class AvailabilityController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public AvailabilityController(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// คำนวณช่วงเวลาว่างที่สามารถจองได้ตาม Service, ช่าง, วัน และสาขา
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] long branchId,
        [FromQuery] long serviceId,
        [FromQuery] long? employeeId,
        [FromQuery] string date,
        CancellationToken cancellationToken)
    {
        if (!DateOnly.TryParse(date, out var bookingDate))
        {
            bookingDate = DateOnly.FromDateTime(DateTime.Now);
        }

        var service = await _context.Services.FindAsync(new object[] { serviceId }, cancellationToken);
        var duration = service?.DurationMinutes ?? 45;

        // 1. Get branch business hours for that day of week
        var dayOfWeek = bookingDate.DayOfWeek;
        var businessHour = await _context.BusinessHours
            .FirstOrDefaultAsync(bh => bh.BranchId == branchId && bh.DayOfWeek == dayOfWeek, cancellationToken);

        var openTime = businessHour?.OpenTime ?? new TimeOnly(10, 0);
        var closeTime = businessHour?.CloseTime ?? new TimeOnly(20, 0);

        // 2. Get existing bookings for that day
        var bookingsQuery = _context.Bookings
            .Where(b => b.BranchId == branchId &&
                        b.BookingDate == bookingDate &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.NoShow);

        if (employeeId.HasValue && employeeId.Value > 0)
        {
            bookingsQuery = bookingsQuery.Where(b => b.EmployeeId == employeeId.Value);
        }

        var existingBookings = await bookingsQuery
            .Select(b => new { b.StartTime, b.EndTime })
            .ToListAsync(cancellationToken);

        // 3. Generate slots at 30 or 45-minute intervals between openTime and closeTime
        var slotInterval = 30; // standard 30-min slot intervals
        var slots = new List<AvailabilitySlot>();
        var current = openTime;

        while (current.AddMinutes(duration) <= closeTime)
        {
            var slotEnd = current.AddMinutes(duration);
            var isConflict = existingBookings.Any(b =>
                (current >= b.StartTime && current < b.EndTime) ||
                (slotEnd > b.StartTime && slotEnd <= b.EndTime) ||
                (current <= b.StartTime && slotEnd >= b.EndTime)
            );

            slots.Add(new AvailabilitySlot(
                Time: current.ToString("HH:mm"),
                Available: !isConflict,
                Reason: isConflict ? "มีผู้จองแล้ว" : null
            ));

            current = current.AddMinutes(slotInterval);
        }

        return Ok(Result<List<AvailabilitySlot>>.Success(slots));
    }
}
