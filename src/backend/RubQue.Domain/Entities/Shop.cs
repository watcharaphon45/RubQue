using RubQue.Domain.Common;
using RubQue.Domain.Enums;

namespace RubQue.Domain.Entities;

public class Shop : BaseEntity
{
    public long ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    // Navigation properties
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<ServiceCategory> ServiceCategories { get; set; } = new List<ServiceCategory>();
    public ICollection<LineChannel> LineChannels { get; set; } = new List<LineChannel>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
