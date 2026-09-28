namespace RubQue.Domain.Entities;

public class BusinessHour
{
    public long BusinessHourId { get; set; }
    public long BranchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; }

    public Branch? Branch { get; set; }
}

public class Holiday
{
    public long HolidayId { get; set; }
    public long BranchId { get; set; }
    public DateOnly HolidayDate { get; set; }
    public string? Reason { get; set; }

    public Branch? Branch { get; set; }
}
