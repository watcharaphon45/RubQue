import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';

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

export interface BarberScheduleItem {
  dayOfWeek: number;
  dayName: string;
  startTime: string;
  endTime: string;
  isOff: boolean;
}

export interface AdminBarberItem {
  id: number;
  name: string;
  nickname: string;
  position: string;
  phone: string;
  status: string;
  offDaysText?: string;
  services?: string[];
  schedules?: BarberScheduleItem[];
}

export interface CurrentAdminUser {
  userId: number;
  username: string;
  fullName: string;
  role: string;
  shopName?: string;
  branchName?: string;
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
  private apiUrl = 'http://localhost:5005/api';

  // Auth signals
  token = signal<string | null>(localStorage.getItem('rubque_admin_token'));
  currentUser = signal<CurrentAdminUser | null>(null);
  loginUsername = signal<string>('admin');
  loginPassword = signal<string>('Admin1234!');
  loginError = signal<string>('');
  isLoggingIn = signal<boolean>(false);

  // App UI signals
  activeMenu = signal<'dashboard' | 'bookings' | 'services' | 'barbers'>('dashboard');
  filterStatus = signal<string>('ALL');
  searchQuery = signal<string>('');
  showWalkInModal = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  actionMessage = signal<string>('');

  // Walk-in form signals
  walkInName = signal<string>('');
  walkInPhone = signal<string>('');
  walkInServiceId = signal<number>(1);
  walkInBarberId = signal<number>(1);
  walkInDate = signal<string>(new Date().toISOString().slice(0, 10));
  walkInTime = signal<string>('14:00');
  walkInNote = signal<string>('');
  walkInError = signal<string>('');
  isSubmittingWalkIn = signal<boolean>(false);

  bookings = signal<BookingAdminItem[]>([]);
  services = signal<AdminServiceItem[]>([]);
  barbers = signal<AdminBarberItem[]>([]);

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
    const storedToken = this.token();
    if (storedToken) {
      this.checkAuthMe(storedToken);
    }
  }

  private getAuthHeaders(): HttpHeaders {
    const currentToken = this.token();
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (currentToken) {
      headers = headers.set('Authorization', `Bearer ${currentToken}`);
    }
    return headers;
  }

  // 1. JWT Authentication
  login(preUsername?: string, prePassword?: string) {
    const username = preUsername ?? this.loginUsername().trim();
    const password = prePassword ?? this.loginPassword().trim();

    if (!username || !password) {
      this.loginError.set('กรุณาระบุ Username และ Password');
      return;
    }

    this.isLoggingIn.set(true);
    this.loginError.set('');

    this.http.post<any>(`${this.apiUrl}/auth/login`, { username, password })
      .subscribe({
        next: (res) => {
          this.isLoggingIn.set(false);
          if (res?.succeeded && res.data?.token) {
            const token = res.data.token;
            localStorage.setItem('rubque_admin_token', token);
            this.token.set(token);
            this.currentUser.set({
              userId: res.data.userId,
              username: res.data.username,
              fullName: res.data.fullName,
              role: res.data.role === 1 ? 'Owner' : 'Staff',
              shopName: 'RubQue Barber & Salon',
              branchName: 'สาขาสยามสแควร์'
            });
            this.loadAllData();
          } else {
            this.loginError.set(res?.message || 'เข้าสู่ระบบไม่สำเร็จ');
          }
        },
        error: (err) => {
          this.isLoggingIn.set(false);
          this.loginError.set(err?.error?.message || 'ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง');
        }
      });
  }

  logout() {
    localStorage.removeItem('rubque_admin_token');
    this.token.set(null);
    this.currentUser.set(null);
    this.bookings.set([]);
  }

  checkAuthMe(token: string) {
    this.http.get<any>(`${this.apiUrl}/auth/me`, {
      headers: new HttpHeaders({ 'Authorization': `Bearer ${token}` })
    }).subscribe({
      next: (res) => {
        if (res?.succeeded && res.data) {
          this.currentUser.set({
            userId: res.data.userId,
            username: res.data.username,
            fullName: res.data.fullName,
            role: res.data.role,
            shopName: res.data.shopName || 'RubQue Barber & Salon',
            branchName: res.data.branchName || 'สาขาสยามสแควร์'
          });
          this.loadAllData();
        } else {
          this.logout();
        }
      },
      error: () => {
        this.logout();
      }
    });
  }

  loadAllData() {
    this.refreshBookings();
    this.loadServices();
    this.loadEmployees();
  }

  // 2. Fetch Live Bookings
  refreshBookings() {
    this.isLoading.set(true);
    this.http.get<any>(`${this.apiUrl}/bookings`, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (res) => {
          this.isLoading.set(false);
          if (res?.data) {
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
        }
      });
  }

  // 3. Fetch Live Services
  loadServices() {
    this.http.get<any>(`${this.apiUrl}/services`, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (res) => {
          if (res?.data) {
            const allServices: AdminServiceItem[] = [];
            for (const cat of res.data) {
              if (cat.services) {
                for (const s of cat.services) {
                  allServices.push({
                    id: s.serviceId,
                    name: s.serviceName,
                    category: cat.categoryName,
                    duration: s.durationMinutes,
                    price: s.price,
                    deposit: s.depositAmount
                  });
                }
              }
            }
            this.services.set(allServices);
            if (allServices.length > 0) {
              this.walkInServiceId.set(allServices[0].id);
            }
          }
        },
        error: () => {}
      });
  }

  // 4. Fetch Live Employees with Schedules & Off-Days
  loadEmployees() {
    this.http.get<any>(`${this.apiUrl}/employees?branchId=1`, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (res) => {
          if (res?.data) {
            const dayNamesTh: Record<number, string> = {
              0: 'อาทิตย์',
              1: 'จันทร์',
              2: 'อังคาร',
              3: 'พุธ',
              4: 'พฤหัสบดี',
              5: 'ศุกร์',
              6: 'เสาร์'
            };

            const empList: AdminBarberItem[] = res.data.map((e: any) => {
              const offDays = (e.schedules || [])
                .filter((s: any) => s.isOff)
                .map((s: any) => `วัน${dayNamesTh[s.dayOfWeek] || s.dayName}`)
                .join(', ');

              return {
                id: e.employeeId,
                name: e.name,
                nickname: e.nickname ? `ช่าง${e.nickname}` : e.name,
                position: e.position || 'Barber Stylist',
                phone: '08' + (Math.floor(10000000 + Math.random() * 90000000)),
                status: 'พร้อมรับงาน',
                offDaysText: offDays ? `หยุดทุก${offDays}` : 'ทำงานทุกวัน',
                services: (e.services || []).map((s: any) => s.serviceName),
                schedules: e.schedules
              };
            });

            this.barbers.set(empList);
            if (empList.length > 0) {
              this.walkInBarberId.set(empList[0].id);
            }
          }
        },
        error: () => {}
      });
  }

  // 5. Update status (Check-in, Start Service, Complete, Cancel)
  updateStatus(id: number, newStatus: 'Confirmed' | 'CheckedIn' | 'InService' | 'Completed' | 'Cancelled') {
    const statusEnumMap: Record<string, number> = {
      'Pending': 0,
      'Confirmed': 1,
      'CheckedIn': 2,
      'InService': 3,
      'Completed': 4,
      'Cancelled': 5
    };

    // Optimistic UI update
    this.bookings.update(list =>
      list.map(b => b.bookingId === id ? { ...b, status: newStatus } : b)
    );

    this.http.patch(`${this.apiUrl}/bookings/${id}/status`, 
      { status: statusEnumMap[newStatus] },
      { headers: this.getAuthHeaders() }
    ).subscribe({
      next: () => {
        this.showToast(`อัปเดตสถานะคิวเป็น "${this.getStatusLabel(newStatus)}" เรียบร้อย`);
        this.refreshBookings();
      },
      error: (err) => {
        this.showToast(`เกิดข้อผิดพลาดในการอัปเดตสถานะ: ${err?.error?.message || 'กรุณาลองใหม่อีกครั้ง'}`);
        this.refreshBookings();
      }
    });
  }

  // 6. Save new walk-in booking from modal to backend
  saveWalkIn() {
    const name = this.walkInName().trim();
    const phone = this.walkInPhone().trim();
    if (!name || !phone) {
      this.walkInError.set('กรุณาระบุชื่อและเบอร์โทรศัพท์ของลูกค้า');
      return;
    }

    this.isSubmittingWalkIn.set(true);
    this.walkInError.set('');

    const payload = {
      shopId: 1,
      branchId: 1,
      employeeId: this.walkInBarberId() > 0 ? this.walkInBarberId() : null,
      serviceId: this.walkInServiceId(),
      bookingDate: this.walkInDate(),
      startTime: this.walkInTime(),
      customerName: name,
      customerPhone: phone,
      note: this.walkInNote() ? `[Walk-in] ${this.walkInNote()}` : '[Walk-in หน้าร้าน]'
    };

    this.http.post<any>(`${this.apiUrl}/bookings`, payload, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (res) => {
          this.isSubmittingWalkIn.set(false);
          if (res?.succeeded) {
            this.showWalkInModal.set(false);
            this.resetWalkInForm();
            this.showToast(`สร้างคิว Walk-in เรียบร้อย: ${res.data?.bookingNo || ''}`);
            this.refreshBookings();
          } else {
            this.walkInError.set(res?.message || 'ไม่สามารถสร้างคิวได้');
          }
        },
        error: (err) => {
          this.isSubmittingWalkIn.set(false);
          const errorMsg = err?.error?.message || 'เกิดข้อผิดพลาดในการบันทึก กรุณาตรวจสอบเวลาหรือช่าง';
          this.walkInError.set(errorMsg);
        }
      });
  }

  showToast(msg: string) {
    this.actionMessage.set(msg);
    setTimeout(() => {
      this.actionMessage.set('');
    }, 4000);
  }

  resetWalkInForm() {
    this.walkInName.set('');
    this.walkInPhone.set('');
    this.walkInNote.set('');
    this.walkInError.set('');
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Pending': return 'รอยืนยัน';
      case 'Confirmed': return 'ยืนยันแล้ว';
      case 'CheckedIn': return 'เช็คอินแล้ว';
      case 'InService': return 'กำลังบริการ';
      case 'Completed': return 'เสร็จสิ้น';
      case 'Cancelled': return 'ยกเลิก';
      default: return status;
    }
  }
}
