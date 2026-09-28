using RubQue.Domain.Common;
using RubQue.Domain.Enums;

namespace RubQue.Domain.Entities;

public class ServiceCategory : BaseEntity
{
    public long CategoryId { get; set; }
    public long ShopId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;

    public Shop? Shop { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
}

public class Service : BaseEntity
{
    public long ServiceId { get; set; }
    public long CategoryId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public decimal Price { get; set; }
    public decimal DepositAmount { get; set; } = 0;
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public ServiceCategory? Category { get; set; }
    public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    public ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
}
