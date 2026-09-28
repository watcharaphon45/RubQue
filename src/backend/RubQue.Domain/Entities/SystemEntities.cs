using RubQue.Domain.Common;
using RubQue.Domain.Enums;

namespace RubQue.Domain.Entities;

public class LineChannel : BaseEntity
{
    public long ChannelId { get; set; }
    public long ShopId { get; set; }
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelSecret { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? LiffId { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public Shop? Shop { get; set; }
}

public class NotificationLog
{
    public long NotificationLogId { get; set; }
    public long CustomerId { get; set; }
    public long? BookingId { get; set; }
    public string Channel { get; set; } = "LINE"; // LINE, SMS, Email
    public string Message { get; set; } = string.Empty;
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Success, Failed, Pending
    public string? ErrorMessage { get; set; }

    public Customer? Customer { get; set; }
    public Booking? Booking { get; set; }
}

public class User : BaseEntity
{
    public long UserId { get; set; }
    public long ShopId { get; set; }
    public long? BranchId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.Staff;
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public Shop? Shop { get; set; }
    public Branch? Branch { get; set; }
}

public class AuditLog
{
    public long AuditLogId { get; set; }
    public long? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // CREATE, UPDATE, DELETE
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? IPAddress { get; set; }
}
