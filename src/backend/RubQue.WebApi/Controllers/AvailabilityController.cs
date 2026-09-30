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
    /// รองรับการเช็ควันหยุดร้าน, วันหยุดช่างประจำสัปดาห์, และกรณีช่างท่านใดก็ได้
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

        // 1. ตรวจสอบเวลาทำการของสาขาในวันดังกล่าว
        var dayOfWeek = bookingDate.DayOfWeek;
        var businessHour = await _context.BusinessHours
            .FirstOrDefaultAsync(bh => bh.BranchId == branchId && bh.DayOfWeek == dayOfWeek, cancellationToken);

        if (businessHour == null || businessHour.IsClosed)
        {
            return Ok(Result<List<AvailabilitySlot>>.Success(new List<AvailabilitySlot>(), "สาขาปิดให้บริการในวันดังกล่าว"));
        }

        var openTime = businessHour.OpenTime;
        var closeTime = businessHour.CloseTime;

        // 2. ตรวจสอบช่างตัดผม (กรณีเลือกช่างเจาะจง หรือ ให้ระบบเลือกช่างให้)
        var isSpecificEmployee = employeeId.HasValue && employeeId.Value > 0;
        List<long> eligibleEmployeeIds = new();

        if (isSpecificEmployee)
        {
            var emp = await _context.Employees
                .Include(e => e.EmployeeSchedules)
                .Include(e => e.EmployeeServices)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId.Value && e.Status == RecordStatus.Active, cancellationToken);

            if (emp == null)
            {
                return NotFound(Result.Failure("ไม่พบข้อมูลช่างที่เลือก"));
            }

            // ตรวจสอบว่าช่างทำบริการนี้ได้หรือไม่
            if (!emp.EmployeeServices.Any(es => es.ServiceId == serviceId))
            {
                return Ok(Result<List<AvailabilitySlot>>.Success(new List<AvailabilitySlot>(), $"ช่าง{emp.Nickname ?? emp.Name} ไม่ได้รับบริการรายการนี้"));
            }

            // ตรวจสอบวันหยุดประจำสัปดาห์ / ตารางงานของช่าง
            var schedule = emp.EmployeeSchedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
            if (schedule == null || schedule.IsOff)
            {
                return Ok(Result<List<AvailabilitySlot>>.Success(new List<AvailabilitySlot>(), $"ช่าง{emp.Nickname ?? emp.Name} หยุดประจำวัน{GetThaiDayName(dayOfWeek)}"));
            }

            // ปรับช่วงเวลาตามเวลาเข้า-ออกงานของช่าง
            if (schedule.StartTime > openTime) openTime = schedule.StartTime;
            if (schedule.EndTime < closeTime) closeTime = schedule.EndTime;

            eligibleEmployeeIds.Add(emp.EmployeeId);
        }
        else
        {
            // กรณีเลือก "ช่างท่านใดก็ได้": ค้นหาช่างทั้งหมดในสาขาที่ทำบริการนี้ได้ และไม่หยุดในวันนี้
            var eligibleEmployees = await _context.Employees
                .Include(e => e.EmployeeSchedules)
                .Include(e => e.EmployeeServices)
                .Where(e => e.BranchId == branchId &&
                            e.Status == RecordStatus.Active &&
                            e.EmployeeServices.Any(es => es.ServiceId == serviceId) &&
                            e.EmployeeSchedules.Any(s => s.DayOfWeek == dayOfWeek && !s.IsOff))
                .ToListAsync(cancellationToken);

            if (!eligibleEmployees.Any())
            {
                return Ok(Result<List<AvailabilitySlot>>.Success(new List<AvailabilitySlot>(), "ไม่มีช่างที่ปฏิบัติงานและให้บริการรายการนี้ในวันดังกล่าว"));
            }

            eligibleEmployeeIds = eligibleEmployees.Select(e => e.EmployeeId).ToList();
        }

        // 3. ดึงรายการ Booking ที่ยังไม่ถูกยกเลิกในวันนั้นของช่างที่เกี่ยวข้อง
        var existingBookings = await _context.Bookings
            .Where(b => b.BranchId == branchId &&
                        b.BookingDate == bookingDate &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.NoShow &&
                        b.EmployeeId.HasValue &&
                        eligibleEmployeeIds.Contains(b.EmployeeId.Value))
            .Select(b => new { b.EmployeeId, b.StartTime, b.EndTime })
            .ToListAsync(cancellationToken);

        // 4. คำนวณ Slot ทุกๆ 30 นาที
        var slotInterval = 30;
        var slots = new List<AvailabilitySlot>();
        var current = openTime;
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var currentTime = TimeOnly.FromDateTime(now);

        while (current.AddMinutes(duration) <= closeTime)
        {
            var slotEnd = current.AddMinutes(duration);
            bool isAvailable;
            string? reason = null;

            // ตรวจสอบว่าเลยเวลาปัจจุบันหรือไม่ (ถ้าจองของวันนี้)
            if (bookingDate == today && current <= currentTime)
            {
                isAvailable = false;
                reason = "เลยเวลาที่สามารถจองได้แล้ว";
            }
            else if (isSpecificEmployee)
            {
                // ช่างคนเดียว: ดูว่าติดคิวไหม
                var hasConflict = existingBookings.Any(b =>
                    (current >= b.StartTime && current < b.EndTime) ||
                    (slotEnd > b.StartTime && slotEnd <= b.EndTime) ||
                    (current <= b.StartTime && slotEnd >= b.EndTime));

                isAvailable = !hasConflict;
                reason = hasConflict ? "ช่างมีคิวอื่นแล้ว" : null;
            }
            else
            {
                // ช่างท่านใดก็ได้: ถ้ามีช่างที่ว่างอย่างน้อย 1 คน ถือว่าว่าง
                var availableBarberCount = eligibleEmployeeIds.Count(empId =>
                    !existingBookings.Any(b => b.EmployeeId == empId &&
                        ((current >= b.StartTime && current < b.EndTime) ||
                         (slotEnd > b.StartTime && slotEnd <= b.EndTime) ||
                         (current <= b.StartTime && slotEnd >= b.EndTime))));

                isAvailable = availableBarberCount > 0;
                reason = isAvailable ? null : "คิวเต็มทุกช่าง";
            }

            slots.Add(new AvailabilitySlot(
                Time: current.ToString("HH:mm"),
                Available: isAvailable,
                Reason: reason
            ));

            current = current.AddMinutes(slotInterval);
        }

        return Ok(Result<List<AvailabilitySlot>>.Success(slots));
    }

    private static string GetThaiDayName(DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        DayOfWeek.Sunday => "อาทิตย์",
        DayOfWeek.Monday => "จันทร์",
        DayOfWeek.Tuesday => "อังคาร",
        DayOfWeek.Wednesday => "พุธ",
        DayOfWeek.Thursday => "พฤหัสบดี",
        DayOfWeek.Friday => "ศุกร์",
        DayOfWeek.Saturday => "เสาร์",
        _ => dayOfWeek.ToString()
    };
}
