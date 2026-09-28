using Microsoft.EntityFrameworkCore;
using RubQue.Application.Common.Interfaces;
using RubQue.Domain.Common;
using RubQue.Domain.Entities;

namespace RubQue.Infrastructure.Persistence;

public class RubQueDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public RubQueDbContext(
        DbContextOptions<RubQueDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmployeeService> EmployeeServices => Set<EmployeeService>();
    public DbSet<EmployeeSchedule> EmployeeSchedules => Set<EmployeeSchedule>();
    public DbSet<EmployeeScheduleOverride> EmployeeScheduleOverrides => Set<EmployeeScheduleOverride>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerLineAccount> CustomerLineAccounts => Set<CustomerLineAccount>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<LineChannel> LineChannels => Set<LineChannel>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Shop
        modelBuilder.Entity<Shop>(b =>
        {
            b.HasKey(x => x.ShopId);
            b.Property(x => x.ShopName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.Email).HasMaxLength(100);
            b.Property(x => x.Address).HasMaxLength(500);
        });

        // Branch
        modelBuilder.Entity<Branch>(b =>
        {
            b.HasKey(x => x.BranchId);
            b.Property(x => x.BranchName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.Address).HasMaxLength(500);
            b.Property(x => x.Latitude).HasPrecision(10, 7);
            b.Property(x => x.Longitude).HasPrecision(10, 7);

            b.HasOne(x => x.Shop)
                .WithMany(s => s.Branches)
                .HasForeignKey(x => x.ShopId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BusinessHour
        modelBuilder.Entity<BusinessHour>(b =>
        {
            b.HasKey(x => x.BusinessHourId);
            b.HasOne(x => x.Branch)
                .WithMany(br => br.BusinessHours)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Holiday
        modelBuilder.Entity<Holiday>(b =>
        {
            b.HasKey(x => x.HolidayId);
            b.Property(x => x.Reason).HasMaxLength(200);
            b.HasOne(x => x.Branch)
                .WithMany(br => br.Holidays)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ServiceCategory
        modelBuilder.Entity<ServiceCategory>(b =>
        {
            b.HasKey(x => x.CategoryId);
            b.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();

            b.HasOne(x => x.Shop)
                .WithMany(s => s.ServiceCategories)
                .HasForeignKey(x => x.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Service
        modelBuilder.Entity<Service>(b =>
        {
            b.HasKey(x => x.ServiceId);
            b.Property(x => x.ServiceName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Price).HasPrecision(18, 2);
            b.Property(x => x.DepositAmount).HasPrecision(18, 2);

            b.HasOne(x => x.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Employee
        modelBuilder.Entity<Employee>(b =>
        {
            b.HasKey(x => x.EmployeeId);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Nickname).HasMaxLength(50);
            b.Property(x => x.EmployeeCode).HasMaxLength(30);
            b.Property(x => x.Phone).HasMaxLength(20);
            b.Property(x => x.Position).HasMaxLength(100);

            b.HasOne(x => x.Branch)
                .WithMany(br => br.Employees)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeService (Composite Key)
        modelBuilder.Entity<EmployeeService>(b =>
        {
            b.HasKey(es => new { es.EmployeeId, es.ServiceId });

            b.HasOne(es => es.Employee)
                .WithMany(e => e.EmployeeServices)
                .HasForeignKey(es => es.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(es => es.Service)
                .WithMany(s => s.EmployeeServices)
                .HasForeignKey(es => es.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EmployeeSchedule
        modelBuilder.Entity<EmployeeSchedule>(b =>
        {
            b.HasKey(x => x.EmployeeScheduleId);
            b.HasOne(x => x.Employee)
                .WithMany(e => e.EmployeeSchedules)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EmployeeScheduleOverride
        modelBuilder.Entity<EmployeeScheduleOverride>(b =>
        {
            b.HasKey(x => x.OverrideId);
            b.Property(x => x.Reason).HasMaxLength(200);
            b.HasOne(x => x.Employee)
                .WithMany(e => e.ScheduleOverrides)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Customer
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasKey(x => x.CustomerId);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Phone).HasMaxLength(20);
            b.HasIndex(x => x.Phone);
        });

        // CustomerLineAccount
        modelBuilder.Entity<CustomerLineAccount>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.LineUserId).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.LineUserId).IsUnique();

            b.HasOne(x => x.Customer)
                .WithMany(c => c.LineAccounts)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Booking
        modelBuilder.Entity<Booking>(b =>
        {
            b.HasKey(x => x.BookingId);
            b.Property(x => x.BookingNo).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.BookingNo).IsUnique();
            b.Property(x => x.TotalAmount).HasPrecision(18, 2);
            b.Property(x => x.DepositAmount).HasPrecision(18, 2);
            b.Property(x => x.Version).IsConcurrencyToken();

            b.HasOne(x => x.Shop)
                .WithMany()
                .HasForeignKey(x => x.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Branch)
                .WithMany(br => br.Bookings)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Employee)
                .WithMany(e => e.Bookings)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // BookingItem
        modelBuilder.Entity<BookingItem>(b =>
        {
            b.HasKey(x => x.BookingItemId);
            b.Property(x => x.ServiceName).HasMaxLength(150).IsRequired();
            b.Property(x => x.Price).HasPrecision(18, 2);

            b.HasOne(x => x.Booking)
                .WithMany(bkg => bkg.Items)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Service)
                .WithMany(s => s.BookingItems)
                .HasForeignKey(x => x.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LineChannel
        modelBuilder.Entity<LineChannel>(b =>
        {
            b.HasKey(x => x.ChannelId);
            b.Property(x => x.ChannelName).HasMaxLength(100).IsRequired();
            b.Property(x => x.ChannelSecret).HasMaxLength(255).IsRequired();
            b.Property(x => x.AccessToken).HasMaxLength(1000).IsRequired();
            b.Property(x => x.LiffId).HasMaxLength(100);

            b.HasOne(x => x.Shop)
                .WithMany(s => s.LineChannels)
                .HasForeignKey(x => x.ShopId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // NotificationLog
        modelBuilder.Entity<NotificationLog>(b =>
        {
            b.HasKey(x => x.NotificationLogId);
            b.Property(x => x.Channel).HasMaxLength(50).IsRequired();
            b.Property(x => x.Status).HasMaxLength(50).IsRequired();

            b.HasOne(x => x.Customer)
                .WithMany(c => c.NotificationLogs)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Booking)
                .WithMany(bkg => bkg.NotificationLogs)
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // User
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.UserId);
            b.Property(x => x.Username).HasMaxLength(50).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.FullName).HasMaxLength(100).IsRequired();
            b.Property(x => x.Email).HasMaxLength(100);
            b.Property(x => x.Phone).HasMaxLength(20);

            b.HasOne(x => x.Shop)
                .WithMany(s => s.Users)
                .HasForeignKey(x => x.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.HasKey(x => x.AuditLogId);
            b.Property(x => x.Action).HasMaxLength(50).IsRequired();
            b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            b.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var currentUserName = _currentUserService?.Username ?? "system";

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = now;
                    entry.Entity.CreatedBy = currentUserName;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedDate = now;
                    entry.Entity.UpdatedBy = currentUserName;
                    break;
            }
        }

        // Concurrency token update for Booking
        foreach (var entry in ChangeTracker.Entries<Booking>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version++;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
