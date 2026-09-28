using RubQue.Domain.Common;

namespace RubQue.Domain.Entities;

public class Customer : BaseEntity
{
    public long CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Note { get; set; }

    public ICollection<CustomerLineAccount> LineAccounts { get; set; } = new List<CustomerLineAccount>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}

public class CustomerLineAccount : BaseEntity
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string Provider { get; set; } = "LINE";
    public string LineUserId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? PictureUrl { get; set; }

    public Customer? Customer { get; set; }
}
