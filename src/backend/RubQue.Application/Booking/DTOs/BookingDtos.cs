using RubQue.Domain.Enums;

namespace RubQue.Application.Booking.DTOs;

public record CreateBookingRequest(
    long ShopId,
    long BranchId,
    long? EmployeeId,
    long ServiceId,
    DateOnly BookingDate,
    TimeOnly StartTime,
    string CustomerName,
    string CustomerPhone,
    string? Note
);

public record BookingDto(
    long BookingId,
    string BookingNo,
    long ShopId,
    long BranchId,
    string BranchName,
    long CustomerId,
    string CustomerName,
    string CustomerPhone,
    long? EmployeeId,
    string? EmployeeName,
    DateOnly BookingDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    BookingStatus Status,
    string StatusText,
    decimal TotalAmount,
    decimal DepositAmount,
    string? Note,
    List<BookingItemDto> Items
);

public record BookingItemDto(
    long ServiceId,
    string ServiceName,
    decimal Price,
    int DurationMinutes
);

public record UpdateBookingStatusRequest(
    BookingStatus Status
);

public record AvailabilitySlot(
    string Time,
    bool Available,
    string? Reason
);
