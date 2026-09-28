using RubQue.Domain.Common;
using RubQue.Domain.Enums;

namespace RubQue.Domain.Entities;

public class Branch : BaseEntity
{
    public long BranchId { get; set; }
    public long ShopId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    // Navigation properties
    public Shop? Shop { get; set; }
    public ICollection<BusinessHour> BusinessHours { get; set; } = new List<BusinessHour>();
    public ICollection<Holiday> Holidays { get; set; } = new List<Holiday>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
