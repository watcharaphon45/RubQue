import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

export interface ServiceItem {
  id: number;
  categoryId: number;
  categoryName: string;
  name: string;
  description: string;
  durationMinutes: number;
  price: number;
  depositAmount: number;
  icon: string;
}

export interface BarberItem {
  id: number;
  name: string;
  nickname: string;
  position: string;
  avatar: string;
  rating: number;
  experience: string;
  specialty: string;
}

export interface DayOption {
  dateStr: string;
  dayName: string;
  dayNumber: number;
  monthName: string;
  isToday: boolean;
}

export interface BookingResult {
  bookingNo: string;
  customerName: string;
  customerPhone: string;
  serviceName: string;
  durationMinutes: number;
  barberName: string;
  dateStr: string;
  timeStr: string;
  totalAmount: number;
  status: string;
}

export interface QueueItem {
  bookingNo: string;
  customerName: string;
  serviceName: string;
  barberName: string;
  dateStr: string;
  timeStr: string;
  status: 'Confirmed' | 'CheckedIn' | 'InService' | 'Completed' | 'Cancelled';
  queuePosition?: number;
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api';

  // Navigation & Step state
  activeTab = signal<'booking' | 'my-queue' | 'shop-info'>('booking');
  currentStep = signal<number>(1); // 1: Service, 2: Barber, 3: DateTime, 4: Customer Info, 5: Ticket

  // Selections
  selectedCategory = signal<string>('all');
  selectedService = signal<ServiceItem | null>(null);
  selectedBarber = signal<BarberItem | null>(null);
  selectedDate = signal<string>('');
  selectedTime = signal<string>('');

  // Customer Form
  customerName = signal<string>('');
  customerPhone = signal<string>('');
  customerNote = signal<string>('');
  lookupPhone = signal<string>('089-123-4567');

  // UI state
  isLoading = signal<boolean>(false);
  showToast = signal<boolean>(false);
  toastMessage = signal<string>('');
  showLineNotificationBanner = signal<boolean>(false);
  bookingSuccessData = signal<BookingResult | null>(null);

  // Lists
  categories = [
    { id: 'all', name: 'ทั้งหมด' },
    { id: 'haircut', name: 'ตัดผม & ออกแบบ' },
    { id: 'color-perm', name: 'ทำสี & ดัดวอลลุ่ม' },
    { id: 'treatment', name: 'ทรีตเมนต์ & สปา' }
  ];

  services: ServiceItem[] = [
    {
      id: 1,
      categoryId: 1,
      categoryName: 'haircut',
      name: 'ตัดผมชาย สไตล์โมเดิร์น / วินเทจ',
      description: 'บริการตัด สระ เซ็ตทรง จัดแต่งสไตล์ด้วย Pomade พรีเมียม',
      durationMinutes: 45,
      price: 400,
      depositAmount: 100,
      icon: 'bi-scissors'
    },
    {
      id: 2,
      categoryId: 1,
      categoryName: 'haircut',
      name: 'สระไดร์ & เซ็ตทรงพรีเมียม',
      description: 'สระนวดผ่อนคลายศีรษะ ไดร์จัดแต่งทรงพร้อมออกงาน',
      durationMinutes: 30,
      price: 250,
      depositAmount: 0,
      icon: 'bi-droplet'
    },
    {
      id: 3,
      categoryId: 2,
      categoryName: 'color-perm',
      name: 'ดัดวอลลุ่มผมชาย (Men Perm)',
      description: 'ดัดลอนสไตล์เกาหลี ยกโคนผม จัดทรงง่าย ทนนาน 2-3 เดือน',
      durationMinutes: 90,
      price: 1600,
      depositAmount: 500,
      icon: 'bi-wind'
    },
    {
      id: 4,
      categoryId: 2,
      categoryName: 'color-perm',
      name: 'ทำสีผมแฟชั่น / ฟอกสีไฮไลท์',
      description: 'ใช้น้ำยาออร์แกนิกถนอมเส้นผม สีติดทนนานประกายเงางาม',
      durationMinutes: 120,
      price: 2200,
      depositAmount: 500,
      icon: 'bi-palette'
    },
    {
      id: 5,
      categoryId: 3,
      categoryName: 'treatment',
      name: 'สปาดีท็อกซ์หนังศีรษะ (Scalp Detox Spa)',
      description: 'ขจัดสารเคมีตกค้างและรังแค พร้อมนวดผ่อนคลายกล้ามเนื้อ',
      durationMinutes: 45,
      price: 750,
      depositAmount: 200,
      icon: 'bi-heart-pulse'
    }
  ];

  barbers: BarberItem[] = [
    {
      id: 0,
      name: 'ให้ระบบเลือกช่างให้',
      nickname: 'ช่างท่านใดก็ได้ (เร็วที่สุด)',
      position: 'Any Barber Available',
      avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&auto=format&fit=crop&q=80',
      rating: 4.9,
      experience: 'คิวเร็วที่สุด',
      specialty: 'แนะนำสำหรับผู้ที่ต้องการเวลาที่สะดวกที่สุด'
    },
    {
      id: 1,
      name: 'เอกชัย วงศ์สุวรรณ',
      nickname: 'ช่างเอก (Master Barber)',
      position: 'Master Stylist (12 ปี ประสบการณ์)',
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
      rating: 5.0,
      experience: '12 ปี',
      specialty: 'งานวินเทจ คลาสสิก เฟดเนียนประณีต'
    },
    {
      id: 2,
      name: 'พงศธร มีสุข',
      nickname: 'ช่างบอย (Senior Barber)',
      position: 'Senior Stylist (7 ปี ประสบการณ์)',
      avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&auto=format&fit=crop&q=80',
      rating: 4.8,
      experience: '7 ปี',
      specialty: 'โมเดิร์นคัต Two-Block และเกาหลี'
    },
    {
      id: 3,
      name: 'แจ็คกี้ สมชาย',
      nickname: 'ช่างแจ็ค (Color & Perm)',
      position: 'Specialist Stylist (9 ปี ประสบการณ์)',
      avatar: 'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?w=150&auto=format&fit=crop&q=80',
      rating: 4.9,
      experience: '9 ปี',
      specialty: 'ดัดวอลลุ่มชาย & กัดทำสีแฟชั่น'
    }
  ];

  availableDates: DayOption[] = [];
  timeSlots: { time: string; available: boolean }[] = [
    { time: '10:00', available: true },
    { time: '10:45', available: true },
    { time: '11:30', available: false },
    { time: '12:15', available: true },
    { time: '13:00', available: true },
    { time: '13:45', available: true },
    { time: '14:30', available: false },
    { time: '15:15', available: true },
    { time: '16:00', available: true },
    { time: '16:45', available: true },
    { time: '17:30', available: true },
    { time: '18:15', available: false },
    { time: '19:00', available: true }
  ];

  myQueues = signal<QueueItem[]>([
    {
      bookingNo: 'BK2026092801',
      customerName: 'คุณวัชรพล',
      serviceName: 'ตัดผมชาย สไตล์โมเดิร์น / วินเทจ',
      barberName: 'ช่างเอก (Master Barber)',
      dateStr: 'วันนี้ (28 ก.ย. 2026)',
      timeStr: '14:00 น.',
      status: 'Confirmed',
      queuePosition: 2
    }
  ]);

  ngOnInit() {
    this.generateNext7Days();
    // Default select today
    if (this.availableDates.length > 0) {
      this.selectedDate.set(this.availableDates[0].dateStr);
    }
    // Set default customer info for demonstration
    this.customerName.set('คุณวัชรพล');
    this.customerPhone.set('089-123-4567');
  }

  generateNext7Days() {
    const daysTh = ['อา.', 'จ.', 'อ.', 'พ.', 'พฤ.', 'ศ.', 'ส.'];
    const monthsTh = ['ม.ค.', 'ก.พ.', 'มี.ค.', 'เม.ย.', 'พ.ค.', 'มิ.ย.', 'ก.ค.', 'ส.ค.', 'ก.ย.', 'ต.ค.', 'พ.ย.', 'ธ.ค.'];
    const list: DayOption[] = [];

    const now = new Date();
    for (let i = 0; i < 7; i++) {
      const d = new Date();
      d.setDate(now.getDate() + i);

      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      const dateStr = `${year}-${month}-${day}`;

      list.push({
        dateStr,
        dayName: daysTh[d.getDay()],
        dayNumber: d.getDate(),
        monthName: monthsTh[d.getMonth()],
        isToday: i === 0
      });
    }

    this.availableDates = list;
  }

  filteredServices() {
    const cat = this.selectedCategory();
    if (cat === 'all') return this.services;
    return this.services.filter(s => s.categoryName === cat);
  }

  // Step 1: Select Service
  chooseService(service: ServiceItem) {
    this.selectedService.set(service);
    this.currentStep.set(2);
  }

  // Step 2: Select Barber
  chooseBarber(barber: BarberItem) {
    this.selectedBarber.set(barber);
    this.currentStep.set(3);
    this.fetchAvailabilitySlots();
  }

  // Step 3: Date & Slot
  chooseDate(dateStr: string) {
    this.selectedDate.set(dateStr);
    this.selectedTime.set('');
    this.fetchAvailabilitySlots();
  }

  chooseTime(time: string, available: boolean) {
    if (!available) return;
    this.selectedTime.set(time);
  }

  goToStep4() {
    if (!this.selectedTime()) {
      this.triggerToast('กรุณาเลือกช่วงเวลาที่ต้องการจอง');
      return;
    }
    this.currentStep.set(4);
  }

  fetchAvailabilitySlots() {
    const service = this.selectedService();
    const barber = this.selectedBarber();
    const date = this.selectedDate();

    if (!service || !date) return;

    this.isLoading.set(true);
    const barberId = barber && barber.id > 0 ? barber.id : '';

    this.http.get<any>(`${this.apiUrl}/availability?branchId=1&serviceId=${service.id}&employeeId=${barberId}&date=${date}`)
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res?.data && res.data.length > 0) {
            this.timeSlots = res.data.map((item: any) => ({
              time: item.time,
              available: item.available
            }));
          }
        },
        error: () => {
          this.isLoading.set(false);
          // Backend offline or fallback to client simulation slots
        }
      });
  }

  // Step 4: Submit Booking
  submitBooking() {
    const name = this.customerName().trim();
    const phone = this.customerPhone().trim();

    if (!name || !phone) {
      this.triggerToast('กรุณากรอกชื่อและเบอร์โทรศัพท์');
      return;
    }

    const service = this.selectedService()!;
    const barber = this.selectedBarber();
    const date = this.selectedDate();
    const time = this.selectedTime();

    this.isLoading.set(true);

    const payload = {
      shopId: 1,
      branchId: 1,
      employeeId: barber && barber.id > 0 ? barber.id : null,
      serviceId: service.id,
      bookingDate: date,
      startTime: time,
      customerName: name,
      customerPhone: phone,
      note: this.customerNote()
    };

    this.http.post<any>(`${this.apiUrl}/bookings`, payload)
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          const bookingData: BookingResult = {
            bookingNo: res?.data?.bookingNo || `BK${date.replace(/-/g, '')}${Math.floor(1000 + Math.random() * 9000)}`,
            customerName: name,
            customerPhone: phone,
            serviceName: service.name,
            durationMinutes: service.durationMinutes,
            barberName: barber?.nickname || 'ไม่ระบุช่าง',
            dateStr: date,
            timeStr: time,
            totalAmount: service.price,
            status: 'Confirmed'
          };

          this.handleBookingSuccess(bookingData);
        },
        error: () => {
          // Fallback if backend is not started: complete client simulation smoothly
          this.isLoading.set(false);
          const randomSuffix = Math.floor(1000 + Math.random() * 9000);
          const bookingData: BookingResult = {
            bookingNo: `BK${date.replace(/-/g, '')}${randomSuffix}`,
            customerName: name,
            customerPhone: phone,
            serviceName: service.name,
            durationMinutes: service.durationMinutes,
            barberName: barber?.nickname || 'ไม่ระบุช่าง',
            dateStr: date,
            timeStr: time,
            totalAmount: service.price,
            status: 'Confirmed'
          };

          this.handleBookingSuccess(bookingData);
        }
      });
  }

  private handleBookingSuccess(data: BookingResult) {
    this.bookingSuccessData.set(data);
    this.currentStep.set(5);

    // Add to My Queues
    const newQueue: QueueItem = {
      bookingNo: data.bookingNo,
      customerName: data.customerName,
      serviceName: data.serviceName,
      barberName: data.barberName,
      dateStr: data.dateStr,
      timeStr: `${data.timeStr} น.`,
      status: 'Confirmed',
      queuePosition: 1
    };

    this.myQueues.update(list => [newQueue, ...list]);

    // Show simulated LINE Push Notification Banner
    this.showLineNotificationBanner.set(true);
    setTimeout(() => {
      this.showLineNotificationBanner.set(false);
    }, 6000);
  }

  // Lookup my queues by phone
  searchMyQueue() {
    const phone = this.lookupPhone().trim();
    if (!phone) {
      this.triggerToast('กรุณาระบุเบอร์โทรศัพท์เพื่อค้นหาคิว');
      return;
    }

    this.isLoading.set(true);
    this.http.get<any>(`${this.apiUrl}/bookings/my-queue?phone=${encodeURIComponent(phone)}`)
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res?.data && res.data.length > 0) {
            const list: QueueItem[] = res.data.map((b: any) => ({
              bookingNo: b.bookingNo,
              customerName: b.customerName,
              serviceName: b.items?.[0]?.serviceName || 'บริการตัดแต่งทรงผม',
              barberName: b.employeeName || 'ไม่ระบุช่าง',
              dateStr: b.bookingDate,
              timeStr: `${b.startTime} น.`,
              status: b.statusText || 'Confirmed'
            }));
            this.myQueues.set(list);
            this.triggerToast(`พบคิวของคุณทั้งหมด ${list.length} รายการ`);
          } else {
            this.triggerToast('ไม่พบคิวที่ตรงกับเบอร์โทรศัพท์นี้');
          }
        },
        error: () => {
          this.isLoading.set(false);
          this.triggerToast('ค้นหาคิวจากฐานข้อมูลจำลอง');
        }
      });
  }

  startNewBooking() {
    this.currentStep.set(1);
    this.selectedService.set(null);
    this.selectedBarber.set(null);
    this.selectedTime.set('');
    this.customerNote.set('');
    this.activeTab.set('booking');
  }

  viewTicketDetails(q: QueueItem) {
    this.bookingSuccessData.set({
      bookingNo: q.bookingNo,
      serviceName: q.serviceName,
      barberName: q.barberName,
      dateStr: q.dateStr,
      timeStr: q.timeStr.replace(' น.', ''),
      totalAmount: 400,
      durationMinutes: 45,
      status: q.status,
      customerName: q.customerName,
      customerPhone: '089-123-4567'
    });
    this.currentStep.set(5);
    this.activeTab.set('booking');
  }

  triggerToast(msg: string) {
    this.toastMessage.set(msg);
    this.showToast.set(true);
    setTimeout(() => {
      this.showToast.set(false);
    }, 3000);
  }
}
