import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

export interface BookingAdminItem {
  bookingId: number;
  bookingNo: string;
  customerName: string;
  customerPhone: string;
  serviceName: string;
  barberName: string;
  dateStr: string;
  timeStr: string;
  durationMinutes: number;
  totalAmount: number;
  status: 'Pending' | 'Confirmed' | 'CheckedIn' | 'InService' | 'Completed' | 'Cancelled';
  note?: string;
}

export interface AdminServiceItem {
  id: number;
  name: string;
  category: string;
  duration: number;
  price: number;
  deposit: number;
}

export interface AdminBarberItem {
  id: number;
  name: string;
  nickname: string;
  position: string;
  phone: string;
  status: string;
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

  activeMenu = signal<'dashboard' | 'bookings' | 'services' | 'barbers'>('dashboard');
  filterStatus = signal<string>('ALL');
  searchQuery = signal<string>('');
  showWalkInModal = signal<boolean>(false);
  isLoading = signal<boolean>(false);

  // Walk-in form signals
  walkInName = signal<string>('');
  walkInPhone = signal<string>('');
  walkInServiceId = signal<number>(1);
  walkInBarberId = signal<number>(1);
  walkInTime = signal<string>('14:00');
  walkInNote = signal<string>('');

  bookings = signal<BookingAdminItem[]>([
    {
      bookingId: 1,
      bookingNo: 'BK2026092801',
      customerName: 'คุณวัชรพล ภูสุวรรณ',
      customerPhone: '089-123-4567',
      serviceName: 'ตัดผมชาย สไตล์โมเดิร์น / วินเทจ',
      barberName: 'ช่างเอก (Master Barber)',
      dateStr: '2026-09-28',
      timeStr: '10:00',
      durationMinutes: 45,
      totalAmount: 400,
      status: 'Completed',
      note: 'ขอเฟดข้างเบอร์ 1'
    },
    {
      bookingId: 2,
      bookingNo: 'BK2026092802',
      customerName: 'คุณธนภัทร สุขสม',
      customerPhone: '081-555-8899',
      serviceName: 'สระไดร์ & เซ็ตทรงพรีเมียม',
      barberName: 'ช่างบอย',
      dateStr: '2026-09-28',
      timeStr: '11:00',
      durationMinutes: 30,
      totalAmount: 250,
      status: 'InService'
    },
    {
      bookingId: 3,
      bookingNo: 'BK2026092803',
      customerName: 'คุณกิตติศักดิ์ เจริญพร',
      customerPhone: '086-444-2211',
      serviceName: 'ดัดวอลลุ่มผมชาย (Men Perm)',
      barberName: 'ช่างแจ็ค (Color Specialist)',
      dateStr: '2026-09-28',
      timeStr: '13:00',
      durationMinutes: 90,
      totalAmount: 1600,
      status: 'CheckedIn',
      note: 'ดัดลอนธรรมชาติ'
    },
    {
      bookingId: 4,
      bookingNo: 'BK2026092804',
      customerName: 'คุณณัฐพล อารีย์',
      customerPhone: '095-777-3322',
      serviceName: 'ตัดผมชาย สไตล์โมเดิร์น / วินเทจ',
      barberName: 'ช่างเอก (Master Barber)',
      dateStr: '2026-09-28',
      timeStr: '15:00',
      durationMinutes: 45,
      totalAmount: 400,
      status: 'Confirmed'
    },
    {
      bookingId: 5,
      bookingNo: 'BK2026092805',
      customerName: 'คุณศุภชัย มั่นคง',
      customerPhone: '083-999-1122',
      serviceName: 'ทำสีผมแฟชั่น / ฟอกสี',
      barberName: 'ช่างแจ็ค (Color Specialist)',
      dateStr: '2026-09-28',
      timeStr: '16:30',
      durationMinutes: 120,
      totalAmount: 2200,
      status: 'Pending'
    }
  ]);

  services: AdminServiceItem[] = [
    { id: 1, name: 'ตัดผมชาย สไตล์โมเดิร์น / วินเทจ', category: 'ตัดผม', duration: 45, price: 400, deposit: 100 },
    { id: 2, name: 'สระไดร์ & เซ็ตทรงพรีเมียม', category: 'ตัดผม', duration: 30, price: 250, deposit: 0 },
    { id: 3, name: 'ดัดวอลลุ่มผมชาย (Men Perm)', category: 'ทำสี & ดัด', duration: 90, price: 1600, deposit: 500 },
    { id: 4, name: 'ทำสีผมแฟชั่น / ฟอกสี', category: 'ทำสี & ดัด', duration: 120, price: 2200, deposit: 500 },
    { id: 5, name: 'สปาดีท็อกซ์หนังศีรษะ', category: 'สปา', duration: 45, price: 750, deposit: 200 }
  ];

  barbers: AdminBarberItem[] = [
    { id: 1, name: 'เอกชัย วงศ์สุวรรณ', nickname: 'ช่างเอก (Master Barber)', position: 'Master Stylist', phone: '081-111-2222', status: 'พร้อมรับงาน' },
    { id: 2, name: 'พงศธร มีสุข', nickname: 'ช่างบอย', position: 'Senior Barber', phone: '082-222-3333', status: 'ติดลูกค้า' },
    { id: 3, name: 'แจ็คกี้ สมชาย', nickname: 'ช่างแจ็ค', position: 'Color & Perm Specialist', phone: '083-333-4444', status: 'พร้อมรับงาน' }
  ];

  // Computed KPI statistics
  stats = computed(() => {
    const list = this.bookings();
    return {
      total: list.length,
      pending: list.filter(b => b.status === 'Pending').length,
      confirmed: list.filter(b => b.status === 'Confirmed').length,
      checkedIn: list.filter(b => b.status === 'CheckedIn').length,
      inService: list.filter(b => b.status === 'InService').length,
      completed: list.filter(b => b.status === 'Completed').length,
      cancelled: list.filter(b => b.status === 'Cancelled').length,
      revenue: list.filter(b => b.status === 'Completed').reduce((sum, b) => sum + b.totalAmount, 0)
    };
  });

  filteredBookings = computed(() => {
    const filter = this.filterStatus();
    const query = this.searchQuery().toLowerCase().trim();
    let list = this.bookings();

    if (filter !== 'ALL') {
      list = list.filter(b => b.status === filter);
    }

    if (query) {
      list = list.filter(b =>
        b.customerName.toLowerCase().includes(query) ||
        b.customerPhone.includes(query) ||
        b.bookingNo.toLowerCase().includes(query) ||
        b.barberName.toLowerCase().includes(query)
      );
    }

    return list;
  });

  ngOnInit() {
    this.refreshBookings();
  }

  refreshBookings() {
    this.isLoading.set(true);
    this.http.get<any>(`${this.apiUrl}/bookings`)
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res?.data && res.data.length > 0) {
            const list: BookingAdminItem[] = res.data.map((b: any) => ({
              bookingId: b.bookingId,
              bookingNo: b.bookingNo,
              customerName: b.customerName,
              customerPhone: b.customerPhone,
              serviceName: b.items?.[0]?.serviceName || 'บริการตัดแต่งทรงผม',
              barberName: b.employeeName || 'ไม่ระบุช่าง',
              dateStr: b.bookingDate,
              timeStr: b.startTime,
              durationMinutes: b.items?.[0]?.durationMinutes || 45,
              totalAmount: b.totalAmount,
              status: b.statusText,
              note: b.note
            }));
            this.bookings.set(list);
          }
        },
        error: () => {
          this.isLoading.set(false);
          // Backend offline or fallback to mock list
        }
      });
  }

  // Update status (Check-in, Start Service, Complete, Cancel)
  updateStatus(id: number, newStatus: 'Confirmed' | 'CheckedIn' | 'InService' | 'Completed' | 'Cancelled') {
    // 1. Optimistic UI update
    this.bookings.update(list =>
      list.map(b => b.bookingId === id ? { ...b, status: newStatus } : b)
    );

    // 2. Call backend PATCH
    const statusEnumMap: Record<string, number> = {
      'Pending': 0,
      'Confirmed': 1,
      'CheckedIn': 2,
      'InService': 3,
      'Completed': 4,
      'Cancelled': 5
    };

    this.http.patch(`${this.apiUrl}/bookings/${id}/status`, { status: statusEnumMap[newStatus] })
      .subscribe({
        next: () => {},
        error: () => {}
      });
  }

  // Save new walk-in booking from modal
  saveWalkIn() {
    const name = this.walkInName().trim() || 'ลูกค้า Walk-in';
    const phone = this.walkInPhone().trim() || 'หน้าร้าน';
    const service = this.services.find(s => s.id === this.walkInServiceId()) || this.services[0];
    const barber = this.barbers.find(b => b.id === this.walkInBarberId()) || this.barbers[0];
    const time = this.walkInTime();

    const newId = Date.now();
    const randomSuffix = Math.floor(1000 + Math.random() * 9000);
    const bookingNo = `BK${new Date().toISOString().slice(0, 10).replace(/-/g, '')}${randomSuffix}`;

    const newBooking: BookingAdminItem = {
      bookingId: newId,
      bookingNo,
      customerName: name,
      customerPhone: phone,
      serviceName: service.name,
      barberName: barber.nickname,
      dateStr: new Date().toISOString().slice(0, 10),
      timeStr: time,
      durationMinutes: service.duration,
      totalAmount: service.price,
      status: 'CheckedIn',
      note: this.walkInNote()
    };

    this.bookings.update(list => [newBooking, ...list]);

    // Send to backend
    this.http.post(`${this.apiUrl}/bookings`, {
      shopId: 1,
      branchId: 1,
      employeeId: barber.id,
      serviceId: service.id,
      bookingDate: newBooking.dateStr,
      startTime: time,
      customerName: name,
      customerPhone: phone,
      note: this.walkInNote()
    }).subscribe({
      next: () => {},
      error: () => {}
    });

    this.showWalkInModal.set(false);
    this.resetWalkInForm();
  }

  resetWalkInForm() {
    this.walkInName.set('');
    this.walkInPhone.set('');
    this.walkInNote.set('');
  }
}
