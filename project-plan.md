# Barber & Hair Salon Booking Platform

## 1. Project Overview

ระบบจองคิวสำหรับร้านตัดผม ร้านทำผม ร้านเสริมสวย และธุรกิจบริการที่มีลักษณะการนัดหมายเป็นช่วงเวลา

แนวคิดหลักของระบบคือ

> **LINE OA เป็นช่องทางหลักสำหรับลูกค้า**
>
> **LIFF เป็น Web Application ที่เปิดจาก LINE**
>
> **Web Admin เป็นระบบจัดการสำหรับเจ้าของร้านและพนักงาน**

### เป้าหมาย

1. ลูกค้าสามารถจองคิวผ่าน LINE OA ได้โดยไม่ต้องติดตั้ง Mobile Application
2. ร้านสามารถจัดการบริการ ช่าง ตารางงาน และ Booking ได้
3. ระบบสามารถคำนวณช่วงเวลาที่ว่างจากเวลาทำการ ช่าง บริการ และ Booking ที่มีอยู่
4. ระบบสามารถส่ง Notification ผ่าน LINE
5. ออกแบบ Architecture ให้สามารถรองรับหลายร้านและหลายสาขาในอนาคต
6. สามารถพัฒนาเป็น SaaS ได้ในอนาคต

---

# 2. System Concept

```text
                         Booking Platform
                                │
             ┌──────────────────┴──────────────────┐
             │                                     │
        Customer Side                         Admin Side
             │                                     │
          LINE OA                              Web Admin
             │                                     │
           LIFF                                 Angular
             │                                     │
          Angular                              API
             │                                     │
             └──────────────────┬──────────────────┘
                                │
                         ASP.NET Core API
                                │
                         Business Logic
                                │
                            Database
```

---

# 3. Technology Stack

## Frontend

```text
Angular
TypeScript
HTML / CSS
```

ใช้ Angular สำหรับ

- Customer Web ผ่าน LIFF
- Admin Web
- Staff Web

---

## Backend

```text
ASP.NET Core Web API
C#
Entity Framework Core
REST API
```

---

## Database

แนะนำ

```text
PostgreSQL
```

แต่สามารถเปลี่ยนเป็น SQL Server ได้หากต้องการใช้ Technology Stack เดียวกับระบบ Enterprise อื่น

---

## LINE Platform

```text
LINE Official Account
LINE Messaging API
LINE LIFF
LINE Webhook
Rich Menu
```

---

## Deployment

MVP

```text
IIS
Windows Server
Database Server
```

ไม่จำเป็นต้องใช้ Docker ในระยะแรก

---

# 4. System Modules

ระบบแบ่งออกเป็น Modules ดังนี้

```text
1. Shop
2. Branch
3. Service
4. Employee
5. Schedule
6. Customer
7. Booking
8. Notification
9. LINE Integration
10. User & Permission
11. Report
12. Audit Log
```

---

# 5. Customer Experience

## 5.1 Entry Point

ลูกค้าจะเข้าสู่ระบบผ่าน

```text
LINE OA
   ↓
Rich Menu
   ↓
LIFF
   ↓
Angular Web Application
```

LINE OA ทำหน้าที่เป็น

- Entry Point
- Customer Identity
- Notification Channel
- Communication Channel
- Marketing Channel

---

# 6. Customer Menu

เมนูหลักของลูกค้าควรมีเฉพาะสิ่งที่จำเป็น

```text
ลูกค้า
├── จองคิว
├── คิวของฉัน
├── ประวัติการใช้บริการ
├── ข้อมูลของฉัน
└── ข้อมูลร้าน
```

---

# 7. Customer Booking Flow

```text
LINE OA
   ↓
กด "จองคิว"
   ↓
เปิด LIFF
   ↓
เลือกสาขา
   ↓
เลือกบริการ
   ↓
เลือกช่าง / ไม่เลือกช่าง
   ↓
เลือกวันที่
   ↓
เลือกเวลา
   ↓
ตรวจสอบ Availability
   ↓
ยืนยันการจอง
   ↓
สร้าง Booking
   ↓
แสดง Booking Confirmation
   ↓
ส่ง LINE Notification
```

---

# 8. Booking Business Rule

ระบบต้องตรวจสอบ

```text
Branch Business Hours
+
Employee Schedule
+
Employee Service
+
Holiday
+
Existing Booking
+
Service Duration
```

ก่อนยืนยัน Booking

---

## 8.1 ตัวอย่าง

ร้านเปิด

```text
10:00 - 20:00
```

ช่าง A ทำงาน

```text
10:00 - 18:00
```

บริการตัดผมใช้เวลา

```text
45 นาที
```

Booking ปัจจุบัน

```text
10:00 - 10:45
11:30 - 12:15
```

ระบบต้องคำนวณ Slot ที่สามารถจองได้จากข้อมูลทั้งหมด

---

# 9. Booking Status

สถานะมาตรฐาน

```text
Pending
Confirmed
CheckedIn
InService
Completed
Cancelled
NoShow
```

ตัวอย่าง Flow

```text
Pending
   ↓
Confirmed
   ↓
CheckedIn
   ↓
InService
   ↓
Completed
```

กรณีอื่น

```text
Pending / Confirmed
       ↓
   Cancelled
```

หรือ

```text
Confirmed
    ↓
  NoShow
```

---

# 10. Admin Application

เจ้าของร้านใช้ Web Admin

## Main Menu

```text
Dashboard

Booking
├── รายการจอง
├── Calendar
└── คิววันนี้

Customer

Service
├── Service Category
└── Service

Employee
├── Employee
├── Employee Service
└── Schedule

Shop
├── Shop Information
├── Branch
├── Business Hours
└── Holiday

LINE
├── LINE OA
├── Rich Menu
└── Notification

Reports

Settings
├── User
├── Role
└── Permission
```

---

# 11. Dashboard

Dashboard ต้องเน้นข้อมูลที่เจ้าของร้านต้องดูทุกวัน

## Summary

```text
Today's Booking
Waiting
Confirmed
Completed
Cancelled
No Show
Revenue
```

ตัวอย่าง

```text
วันนี้

Booking       25
Confirmed     18
Waiting        3
Completed      2
Cancelled      2

Revenue    8,500 THB
```

---

# 12. Booking Management

Admin สามารถ

```text
Create Booking
Edit Booking
Confirm Booking
Check-in
Start Service
Complete
Cancel
No Show
```

และสามารถเพิ่ม Booking เองจากหน้า Admin ได้

กรณีลูกค้า

- โทรมาจอง
- เดินเข้าร้าน
- พนักงานจองให้
- ลูกค้ายังไม่มี LINE

---

# 13. Calendar

แสดง Booking ในรูปแบบ Calendar / Timeline

ตัวอย่าง

```text
10:00   Customer A   Haircut      Employee A
10:45   Customer B   Shampoo      Employee B
11:30   Customer C   Haircut      Employee A
12:30   Customer D   Hair Color   Employee C
```

ควรสามารถ Filter

```text
Branch
Employee
Service
Status
Date
```

ได้

---

# 14. Master Data

## 14.1 Shop

```text
Shop
- ShopId
- ShopName
- Logo
- Description
- Phone
- Email
- Address
- Status
- CreatedDate
- UpdatedDate
```

---

## 14.2 Branch

```text
Branch
- BranchId
- ShopId
- BranchName
- Phone
- Address
- Latitude
- Longitude
- Status
```

ควรออกแบบรองรับหลายสาขาตั้งแต่ต้น แม้ MVP จะใช้งานเพียงสาขาเดียว

---

## 14.3 Service Category

ตัวอย่าง

```text
Haircut
Color
Perm
Straightening
Treatment
```

---

## 14.4 Service

```text
Service
- ServiceId
- CategoryId
- ServiceName
- Description
- DurationMinutes
- Price
- DepositAmount
- Status
```

ตัวอย่าง

```text
ตัดผมชาย
45 นาที
300 บาท
```

---

## 14.5 Employee

```text
Employee
- EmployeeId
- BranchId
- EmployeeCode
- Name
- Nickname
- Phone
- Position
- ProfileImage
- Status
```

---

## 14.6 Employee Service

ใช้กำหนดว่าช่างคนใดให้บริการอะไรได้บ้าง

```text
EmployeeService
- EmployeeId
- ServiceId
```

ตัวอย่าง

```text
Employee A
 ├── Haircut
 ├── Shampoo
 └── Color

Employee B
 ├── Haircut
 └── Perm
```

---

## 14.7 Business Hours

```text
BusinessHour
- BusinessHourId
- BranchId
- DayOfWeek
- OpenTime
- CloseTime
- IsClosed
```

---

## 14.8 Employee Schedule

```text
EmployeeSchedule
- EmployeeScheduleId
- EmployeeId
- DayOfWeek
- StartTime
- EndTime
- IsOff
```

ในอนาคตควรรองรับ Schedule แบบรายวันด้วย

```text
EmployeeScheduleOverride
- EmployeeId
- WorkDate
- StartTime
- EndTime
- IsOff
- Reason
```

เพื่อรองรับ

- ลาพักร้อน
- ลาป่วย
- เปลี่ยนเวลาทำงาน
- ทำงานแทน
- OT

---

## 14.9 Holiday

```text
Holiday
- HolidayId
- BranchId
- HolidayDate
- Reason
```

---

# 15. Customer Data

## Customer

```text
Customer
- CustomerId
- Name
- Phone
- Gender
- BirthDate
- Note
- CreatedDate
- UpdatedDate
```

---

## Customer LINE Account

แยก LINE Identity ออกจาก Customer

```text
CustomerLineAccount
- Id
- CustomerId
- Provider
- LineUserId
- DisplayName
- PictureUrl
- CreatedDate
- UpdatedDate
```

แนวทางนี้ช่วยให้อนาคตสามารถรองรับ Identity Provider อื่นได้

```text
LINE
Google
Facebook
Phone
Email
```

---

# 16. Booking Data

```text
Booking
- BookingId
- BookingNo
- ShopId
- BranchId
- CustomerId
- EmployeeId
- BookingDate
- StartTime
- EndTime
- Status
- TotalAmount
- DepositAmount
- Note
- CreatedDate
- CreatedBy
- UpdatedDate
- UpdatedBy
```

หากในอนาคตหนึ่ง Booking สามารถมีหลายบริการ ควรแยก

```text
Booking
   │
   └── BookingItem
```

ตัวอย่าง

```text
Booking #BK20260928001

Customer: A

Service:
- Haircut
- Shampoo
- Treatment
```

---

# 17. Notification

ระบบต้องรองรับ

```text
Booking Confirmation
Booking Reminder
Booking Cancellation
Booking Change
Queue Called
Promotion
```

---

## 17.1 Notification Log

```text
NotificationLog
- NotificationLogId
- CustomerId
- BookingId
- Channel
- Message
- SentDate
- Status
- ErrorMessage
```

เพื่อให้สามารถตรวจสอบได้ว่า Notification ถูกส่งสำเร็จหรือไม่

---

# 18. LINE Integration

## LINE OA

ระบบต้องเก็บ Configuration ที่จำเป็น

```text
LineChannel
- ChannelId
- ChannelName
- ChannelSecret
- AccessToken
- LiffId
- Status
```

Credential จริงไม่ควรเก็บแบบ Plain Text ใน Database

ควรออกแบบเป็น

```text
Encrypted Secret
```

หรือใช้ Secret Management ใน Production

---

# 19. Rich Menu

เบื้องต้น

```text
┌─────────────────────────────┐
│                             │
│       📅 จองคิว            │
│                             │
├──────────────┬──────────────┤
│ 📋 คิวของฉัน │ 👤 โปรไฟล์   │
├──────────────┼──────────────┤
│ 🎁 Promotion │ 🏪 ข้อมูลร้าน │
└──────────────┴──────────────┘
```

---

# 20. Staff Application

Staff ไม่ควรมี Permission เท่ากับ Owner

## Staff Menu

```text
Dashboard

Today's Queue

Booking

Customer

My Schedule
```

Staff ไม่ควรแก้ข้อมูลสำคัญ เช่น

```text
Service Price
Business Hours
LINE Configuration
User Permission
```

เว้นแต่ได้รับ Permission เพิ่มเติม

---

# 21. User / Role / Permission

## Role เบื้องต้น

```text
Owner
Admin
Staff
```

ตัวอย่าง

### Owner

```text
ทุก Module
```

### Admin

```text
Booking
Customer
Service
Employee
Schedule
Report
```

### Staff

```text
Today's Queue
Booking
Customer
My Schedule
```

---

# 22. Report

MVP ใช้รายงานพื้นฐาน

```text
Booking Report
Revenue Summary
Service Popularity
Employee Booking
Cancellation Report
No Show Report
```

Filter

```text
Date
Branch
Employee
Service
Status
```

---

# 23. Audit Log

ควรออกแบบตั้งแต่ต้น แม้ MVP จะยังไม่เปิดหน้า UI

```text
AuditLog
- AuditLogId
- UserId
- Action
- EntityName
- EntityId
- OldValue
- NewValue
- CreatedDate
- IPAddress
```

ตัวอย่าง

```text
User: Admin001
Action: UPDATE
Entity: Service
EntityId: 15

Price
500 → 550
```

---

# 24. Database Structure

ความสัมพันธ์หลัก

```text
Shop
 │
 ├── Branch
 │     │
 │     ├── Employee
 │     │     ├── EmployeeService
 │     │     └── EmployeeSchedule
 │     │
 │     ├── BusinessHour
 │     └── Holiday
 │
 └── Service
        │
        └── EmployeeService


Customer
 │
 ├── CustomerLineAccount
 │
 └── Booking
         │
         ├── Branch
         ├── Employee
         └── BookingItem
                   │
                   └── Service
```

---

# 25. Architecture

แนะนำ Modular Monolith + Clean Architecture

```text
Project
│
├── Domain
│   ├── Entities
│   ├── Enums
│   └── Interfaces
│
├── Application
│   ├── Booking
│   ├── Customer
│   ├── Employee
│   ├── Service
│   ├── Shop
│   ├── Notification
│   └── LINE
│
├── Infrastructure
│   ├── Persistence
│   ├── LINE
│   ├── Notification
│   └── Security
│
└── Web
    ├── API
    └── SPA
        └── Angular
```

ไม่แนะนำให้แยก Microservice ใน MVP

---

# 26. API Modules

ตัวอย่าง API

```text
/api/auth

/api/shops
/api/branches

/api/services
/api/service-categories

/api/employees
/api/employee-services
/api/employee-schedules

/api/customers

/api/bookings
/api/availability

/api/notifications

/api/line/webhook
/api/line/rich-menu
```

---

# 27. Availability Engine

เป็น Module สำคัญของระบบ

หน้าที่คือ

> คำนวณเวลาที่สามารถจองได้

Input

```text
Branch
Service
Employee
Date
```

Output

```text
Available Slots
```

ตัวอย่าง

```text
GET /api/availability

branchId = 1
serviceId = 2
employeeId = 5
date = 2026-09-28
```

ผลลัพธ์

```json
["10:00", "10:45", "11:30", "13:00", "13:45"]
```

---

# 28. Booking Concurrency

ต้องป้องกันกรณีลูกค้า 2 คนกด Slot เดียวกันพร้อมกัน

ตัวอย่าง

```text
Customer A → 14:00
Customer B → 14:00
```

ระบบต้องไม่สร้าง Booking ซ้ำ

ดังนั้นการตรวจสอบ Availability อย่างเดียวไม่เพียงพอ ต้องมี Transaction / Database Constraint / Concurrency Control ตอนสร้าง Booking จริงด้วย

---

# 29. MVP Scope

## Phase 1 - Core Booking

เป้าหมายคือสามารถเปิดให้ร้านทดลองใช้ได้

### Customer

```text
LINE OA
LIFF
จองคิว
ดูคิว
ยกเลิกคิว
ดูข้อมูลร้าน
ดู Profile
```

### Admin

```text
Dashboard
Booking
Customer
Service
Employee
Schedule
Shop
LINE
```

### Backend

```text
Authentication
Booking API
Availability API
Customer API
Master Data API
LINE Integration
Notification
```

---

# 30. Phase 2 - Business Enhancement

เพิ่ม

```text
Promotion
Coupon
Deposit
Payment
Customer History
Review
Revenue Report
Advanced Report
```

---

# 31. Phase 3 - SaaS

รองรับหลายร้าน

```text
Tenant
   │
   ├── Branch
   ├── Employee
   ├── Service
   ├── Customer
   └── Booking
```

ตัวอย่าง

```text
Tenant A
 ├── Branch 1
 └── Branch 2

Tenant B
 └── Branch 1

Tenant C
 ├── Branch 1
 ├── Branch 2
 └── Branch 3
```

---

# 32. Multi-Tenant Design

ทุกข้อมูล Business ต้องสามารถระบุ Tenant ได้

ตัวอย่าง

```text
Shop / Tenant
   ↓
Branch
   ↓
Employee
Service
Customer
Booking
```

ควรกำหนด Tenant Isolation ให้ชัดเจนตั้งแต่ Database และ Application Layer

---

# 33. Future Features

ระบบสามารถต่อยอดได้เป็น

```text
Online Payment
Deposit
Promotion
Coupon
Loyalty
Membership
Customer CRM
Marketing
Broadcast
Review
Analytics
Multi Branch
Multi Tenant
Subscription
```

---

# 34. สิ่งที่ไม่ควรทำใน MVP

ยังไม่ควรเริ่มด้วย

```text
AI Chatbot เต็มรูปแบบ
ระบบบัญชี
Payroll
Inventory
Loyalty ซับซ้อน
Membership หลายระดับ
Microservices
Mobile Application Native
Payment หลาย Provider
```

ควรทำ Core Booking ให้เสถียรก่อน

---

# 35. Development Roadmap

## Sprint 1

```text
Project Setup
Architecture
Database
Authentication
Base API
```

## Sprint 2

```text
Shop
Branch
Service
Employee
Schedule
```

## Sprint 3

```text
Customer
Booking
Availability
Booking Status
```

## Sprint 4

```text
LINE OA
LIFF
Rich Menu
LINE Login / Identity
```

## Sprint 5

```text
Notification
Reminder
Booking Confirmation
Cancellation
```

## Sprint 6

```text
Admin Dashboard
Calendar
Reports
Permission
Audit Log
```

## Sprint 7

```text
Testing
Performance
Security
UAT
Production Deployment
```

---

# 36. MVP Definition of Done

MVP จะถือว่าพร้อมทดลองใช้เมื่อสามารถทำ Flow นี้ได้ครบ

```text
ร้านสร้าง Account
       ↓
สร้างสาขา
       ↓
สร้างบริการ
       ↓
เพิ่มช่าง
       ↓
กำหนดบริการของช่าง
       ↓
กำหนดเวลาทำงาน
       ↓
เชื่อม LINE OA
       ↓
ลูกค้า Add Friend
       ↓
ลูกค้ากดจองคิว
       ↓
เลือกบริการ
       ↓
เลือกช่าง
       ↓
เลือกวัน
       ↓
เลือกเวลา
       ↓
Confirm
       ↓
สร้าง Booking
       ↓
LINE แจ้งเตือน
       ↓
ร้านเห็น Booking
       ↓
ลูกค้า Check-in
       ↓
ให้บริการ
       ↓
Complete
```

---

# 37. Priority

## Must Have

```text
Shop
Branch
Service
Employee
Employee Service
Business Hours
Employee Schedule
Customer
LINE Identity
Booking
Availability
LINE Notification
Admin
Role / Permission
```

## Should Have

```text
Holiday
Calendar
Customer History
Report
Audit Log
Booking Reminder
```

## Could Have

```text
Promotion
Coupon
Deposit
Payment
Review
Loyalty
```

## Future

```text
Multi-Tenant SaaS
Subscription
Advanced CRM
Marketing Automation
AI
```

---

# 38. Core Principle

ระบบนี้ควรยึดหลัก

```text
LINE = Customer Entry Point + Identity + Communication

LIFF = Customer Web Application

Angular Admin = Shop Management

ASP.NET Core = Business Logic

Database = Business Data
```

และไม่ควรให้ Business Logic ไปอยู่ใน LINE Bot

```text
Wrong

LINE Bot
 └── Booking Logic
```

ควรเป็น

```text
LINE
 ↓
LIFF / API
 ↓
Booking Service
 ↓
Database
```

เพื่อให้ในอนาคตสามารถเพิ่มช่องทางอื่นได้ เช่น

```text
Website
QR Code
Facebook
Mobile App
```

โดยใช้ Booking Engine ตัวเดียวกัน

---

# 39. Final Product Vision

```text
                 Customer
                    │
                    ▼
              ┌───────────┐
              │  LINE OA  │
              └─────┬─────┘
                    │
                   LIFF
                    │
                    ▼
              Customer Web
                    │
                    ▼
             Booking Platform
                    │
       ┌────────────┼────────────┐
       │            │            │
    Booking      Customer      LINE
       │            │            │
       └────────────┼────────────┘
                    │
                    ▼
                Database

                    ▲
                    │
                Web Admin
                    │
             Owner / Staff
```

เป้าหมายสุดท้ายคือทำให้ระบบนี้เป็น

> **แพลตฟอร์มจองคิวที่ใช้ LINE เป็นช่องทางหลักสำหรับลูกค้า และใช้ Web Admin เป็นศูนย์กลางการบริหารร้าน**
>
> โดยสามารถพัฒนาเฉพาะสำหรับร้านตัดผมในช่วงแรก และขยายไปยังร้านทำผม ร้านเสริมสวย ร้านสปา หรือธุรกิจที่ใช้ระบบ Appointment ได้ในอนาคต
