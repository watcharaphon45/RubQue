using Microsoft.EntityFrameworkCore;
using RubQue.Application.Auth.Interfaces;
using RubQue.Domain.Entities;
using RubQue.Domain.Enums;

namespace RubQue.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(RubQueDbContext context, IPasswordHasher passwordHasher)
    {
        // Check if database is already seeded
        if (await context.Shops.AnyAsync())
        {
            return;
        }

        // 1. Create Demo Shop
        var shop = new Shop
        {
            ShopName = "RubQue Classic Barber & Salon",
            Description = "บริการตัดผม ออกแบบทรงผม และเสริมสวยระดับพรีเมียม",
            Phone = "02-123-4567",
            Email = "contact@rubque.com",
            Address = "สยามสแควร์ ซอย 3 เขตปทุมวัน กรุงเทพฯ 10330",
            Status = RecordStatus.Active
        };
        context.Shops.Add(shop);
        await context.SaveChangesAsync();

        // 2. Create Branch
        var branch = new Branch
        {
            ShopId = shop.ShopId,
            BranchName = "สาขาสยามสแควร์ (Siam Square Flagship)",
            Phone = "02-123-4567",
            Address = "สยามสแควร์ ซอย 3 เขตปทุมวัน กรุงเทพฯ",
            Latitude = 13.7444m,
            Longitude = 100.5332m,
            Status = RecordStatus.Active
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        // 3. Business Hours (Open every day 10:00 - 20:00)
        var businessHours = Enum.GetValues<DayOfWeek>().Select(day => new BusinessHour
        {
            BranchId = branch.BranchId,
            DayOfWeek = day,
            OpenTime = new TimeOnly(10, 0),
            CloseTime = new TimeOnly(20, 0),
            IsClosed = false
        }).ToList();
        context.BusinessHours.AddRange(businessHours);

        // 4. Service Categories
        var catHaircut = new ServiceCategory { ShopId = shop.ShopId, CategoryName = "ตัดผม & ออกแบบทรงผม", DisplayOrder = 1 };
        var catColor = new ServiceCategory { ShopId = shop.ShopId, CategoryName = "ทำสีผม & ดัดผม", DisplayOrder = 2 };
        var catSpa = new ServiceCategory { ShopId = shop.ShopId, CategoryName = "ทรีตเมนต์ & สปาผม", DisplayOrder = 3 };

        context.ServiceCategories.AddRange(catHaircut, catColor, catSpa);
        await context.SaveChangesAsync();

        // 5. Services
        var s1 = new Service
        {
            CategoryId = catHaircut.CategoryId,
            ServiceName = "ตัดผมชาย สไตล์โมเดิร์น/วินเทจ",
            Description = "ตัด สระ เซ็ตจัดแต่งทรงผมอย่างประณีต",
            DurationMinutes = 45,
            Price = 400m,
            DepositAmount = 100m,
            Status = RecordStatus.Active
        };
        var s2 = new Service
        {
            CategoryId = catHaircut.CategoryId,
            ServiceName = "สระไดร์ & เซ็ตทรงพรีเมียม",
            Description = "สระนวดศีรษะ ผ่อนคลาย และไดร์จัดทรง",
            DurationMinutes = 30,
            Price = 250m,
            DepositAmount = 0m,
            Status = RecordStatus.Active
        };
        var s3 = new Service
        {
            CategoryId = catColor.CategoryId,
            ServiceName = "ดัดวอลลุ่มผมชาย (Men Perm)",
            Description = "ดัดลอนเกาหลี เพิ่มวอลลุ่ม จัดทรงง่าย อยู่ได้นาน 2-3 เดือน",
            DurationMinutes = 90,
            Price = 1600m,
            DepositAmount = 500m,
            Status = RecordStatus.Active
        };
        var s4 = new Service
        {
            CategoryId = catColor.CategoryId,
            ServiceName = "ทำสีผมแฟชั่น / ฟอกสี",
            Description = "ทำสีผมด้วยผลิตภัณฑ์ออร์แกนิกถนอมเส้นผม",
            DurationMinutes = 120,
            Price = 2200m,
            DepositAmount = 500m,
            Status = RecordStatus.Active
        };
        var s5 = new Service
        {
            CategoryId = catSpa.CategoryId,
            ServiceName = "สปาดีท็อกซ์หนังศีรษะ (Scalp Detox Spa)",
            Description = "ทำความสะอาดล้ำลึก ขจัดความมันและรังแค พร้อมนวดผ่อนคลาย",
            DurationMinutes = 45,
            Price = 750m,
            DepositAmount = 200m,
            Status = RecordStatus.Active
        };

        context.Services.AddRange(s1, s2, s3, s4, s5);
        await context.SaveChangesAsync();

        // 6. Employees
        var emp1 = new Employee
        {
            BranchId = branch.BranchId,
            EmployeeCode = "EMP01",
            Name = "เอกชัย วงศ์สุวรรณ",
            Nickname = "ช่างเอก (Master Barber)",
            Phone = "081-111-2222",
            Position = "Master Stylist",
            Status = RecordStatus.Active
        };
        var emp2 = new Employee
        {
            BranchId = branch.BranchId,
            EmployeeCode = "EMP02",
            Name = "พงศธร มีสุข",
            Nickname = "ช่างบอย",
            Phone = "082-222-3333",
            Position = "Senior Barber",
            Status = RecordStatus.Active
        };
        var emp3 = new Employee
        {
            BranchId = branch.BranchId,
            EmployeeCode = "EMP03",
            Name = "แจ็คกี้ สมชาย",
            Nickname = "ช่างแจ็ค (Color Specialist)",
            Phone = "083-333-4444",
            Position = "Color & Perm Specialist",
            Status = RecordStatus.Active
        };

        context.Employees.AddRange(emp1, emp2, emp3);
        await context.SaveChangesAsync();

        // 7. Employee Services Mapping
        context.EmployeeServices.AddRange(
            // Master Barber Aek does haircut, wash, scalp
            new EmployeeService { EmployeeId = emp1.EmployeeId, ServiceId = s1.ServiceId },
            new EmployeeService { EmployeeId = emp1.EmployeeId, ServiceId = s2.ServiceId },
            new EmployeeService { EmployeeId = emp1.EmployeeId, ServiceId = s5.ServiceId },
            // Boy does haircut and wash
            new EmployeeService { EmployeeId = emp2.EmployeeId, ServiceId = s1.ServiceId },
            new EmployeeService { EmployeeId = emp2.EmployeeId, ServiceId = s2.ServiceId },
            // Jack does color, perm, haircut, wash
            new EmployeeService { EmployeeId = emp3.EmployeeId, ServiceId = s1.ServiceId },
            new EmployeeService { EmployeeId = emp3.EmployeeId, ServiceId = s2.ServiceId },
            new EmployeeService { EmployeeId = emp3.EmployeeId, ServiceId = s3.ServiceId },
            new EmployeeService { EmployeeId = emp3.EmployeeId, ServiceId = s4.ServiceId }
        );

        // 8. Employee Schedule (Mon-Sun 10:00 - 19:00 with one day off)
        var employees = new[] { emp1, emp2, emp3 };
        var offDays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday };

        for (int i = 0; i < employees.Length; i++)
        {
            var emp = employees[i];
            var offDay = offDays[i];

            foreach (var day in Enum.GetValues<DayOfWeek>())
            {
                context.EmployeeSchedules.Add(new EmployeeSchedule
                {
                    EmployeeId = emp.EmployeeId,
                    DayOfWeek = day,
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(19, 0),
                    IsOff = day == offDay
                });
            }
        }

        // 9. Users (Owner & Staff)
        var adminPasswordHash = passwordHasher.HashPassword("Admin1234!");
        var staffPasswordHash = passwordHasher.HashPassword("Staff1234!");

        var adminUser = new User
        {
            ShopId = shop.ShopId,
            BranchId = branch.BranchId,
            Username = "admin",
            PasswordHash = adminPasswordHash,
            FullName = "เจ้าของร้าน (Owner Admin)",
            Email = "admin@rubque.com",
            Role = UserRole.Owner,
            Status = RecordStatus.Active
        };

        var staffUser = new User
        {
            ShopId = shop.ShopId,
            BranchId = branch.BranchId,
            Username = "staff",
            PasswordHash = staffPasswordHash,
            FullName = "พนักงานประจำหน้าร้าน",
            Email = "staff@rubque.com",
            Role = UserRole.Staff,
            Status = RecordStatus.Active
        };

        context.Users.AddRange(adminUser, staffUser);

        await context.SaveChangesAsync();
    }
}
