using RubQue.Domain.Common;
using RubQue.Domain.Enums;

namespace RubQue.Domain.Entities;

public class Employee : BaseEntity
{
    public long EmployeeId { get; set; }
    public long BranchId { get; set; }
    public string? EmployeeCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string? Phone { get; set; }
    public string? Position { get; set; }
    public string? ProfileImage { get; set; }
    public RecordStatus Status { get; set; } = RecordStatus.Active;

    public Branch? Branch { get; set; }
    public ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
    public ICollection<EmployeeSchedule> EmployeeSchedules { get; set; } = new List<EmployeeSchedule>();
    public ICollection<EmployeeScheduleOverride> ScheduleOverrides { get; set; } = new List<EmployeeScheduleOverride>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class EmployeeService
{
    public long EmployeeId { get; set; }
    public long ServiceId { get; set; }

    public Employee? Employee { get; set; }
    public Service? Service { get; set; }
}

public class EmployeeSchedule
{
    public long EmployeeScheduleId { get; set; }
    public long EmployeeId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsOff { get; set; }

    public Employee? Employee { get; set; }
}

public class EmployeeScheduleOverride
{
    public long OverrideId { get; set; }
    public long EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool IsOff { get; set; }
    public string? Reason { get; set; }

    public Employee? Employee { get; set; }
}
