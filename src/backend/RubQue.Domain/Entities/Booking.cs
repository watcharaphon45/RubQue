using RubQue.Domain.Common;
using RubQue.Domain.Enums;

namespace RubQue.Domain.Entities;

public class Booking : BaseEntity
{
    public long BookingId { get; set; }
    public string BookingNo { get; set; } = string.Empty;
    public long ShopId { get; set; }
    public long BranchId { get; set; }
    public long CustomerId { get; set; }
    public long? EmployeeId { get; set; }
    public DateOnly BookingDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public decimal TotalAmount { get; set; } = 0;
    public decimal DepositAmount { get; set; } = 0;
    public string? Note { get; set; }

    // Concurrency token for optimistic locking
    public uint Version { get; set; }

    // Navigation properties
    public Shop? Shop { get; set; }
    public Branch? Branch { get; set; }
    public Customer? Customer { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}

public class BookingItem
{
    public long BookingItemId { get; set; }
    public long BookingId { get; set; }
    public long ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }

    public Booking? Booking { get; set; }
    public Service? Service { get; set; }
}
