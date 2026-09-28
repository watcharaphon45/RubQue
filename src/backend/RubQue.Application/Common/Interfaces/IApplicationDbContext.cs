using Microsoft.EntityFrameworkCore;
using RubQue.Domain.Entities;

namespace RubQue.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Shop> Shops { get; }
    DbSet<Branch> Branches { get; }
    DbSet<BusinessHour> BusinessHours { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<Service> Services { get; }
    DbSet<Employee> Employees { get; }
    DbSet<EmployeeService> EmployeeServices { get; }
    DbSet<EmployeeSchedule> EmployeeSchedules { get; }
    DbSet<EmployeeScheduleOverride> EmployeeScheduleOverrides { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerLineAccount> CustomerLineAccounts { get; }
    DbSet<RubQue.Domain.Entities.Booking> Bookings { get; }
    DbSet<BookingItem> BookingItems { get; }
    DbSet<LineChannel> LineChannels { get; }
    DbSet<NotificationLog> NotificationLogs { get; }
    DbSet<User> Users { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
