import { Component, computed, inject, signal, OnInit, effect, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CalendarModule, CalendarComponent } from '@syncfusion/ej2-angular-calendars';
import { RenderDayCellEventArgs } from '@syncfusion/ej2-calendars';
import { loadCldr, L10n } from '@syncfusion/ej2-base';
import { TenantService } from '../services/tenant.service';
import { BusinessService } from '../services/business.service';
import { EmployeeScheduleService } from '../services/employee-schedule.service';
import { BookingService, CreateBooking } from '../services/booking.service';
import { ToastService } from '../services/toast.service';
import { Service } from '../models/service';
import { Employee } from '../models/employee';
import { EmployeeSchedule, AvailableTimeSlots } from '../models/employee-schedule';

type FlowStep = 'service' | 'employee' | 'datetime' | 'customer';

@Component({
  selector: 'app-booking-flow',
  standalone: true,
  imports: [CommonModule, FormsModule, CalendarModule],
  templateUrl: './booking-flow.html',
  styleUrls: ['./booking-flow.scss']
})
export class BookingFlowComponent implements OnInit {
  @ViewChild('calendar') calendar?: CalendarComponent;

  private tenantService = inject(TenantService);
  private businessService = inject(BusinessService);
  private scheduleService = inject(EmployeeScheduleService);
  private bookingService = inject(BookingService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  business$ = this.tenantService.business$;
  schedules = signal<EmployeeSchedule[]>([]);

  currentStep = signal<FlowStep>('service');
  selectedService = signal<Service | null>(null);
  selectedEmployee = signal<Employee | null>(null);
  selectedDate = signal<Date | null>(null);
  selectedTime = signal<string | null>(null);

  availableSlots = signal<string[]>([]);
  availableDatesMap = signal<Map<string, boolean>>(new Map());
  isLoadingDates = signal(false);
  isLoadingSlots = signal(false);
  isSubmitting = signal(false);

  // Syncfusion calendar settings
  minDate = new Date();
  maxDate = computed(() => {
    const max = new Date();
    max.setDate(max.getDate() + 30);
    return max;
  });

  // Calendar locale settings
  calendarLocale = {
    firstDayOfWeek: 1, // Monday
    today: 'Ma'
  };

  // Customer form data
  customerFirstName = signal('');
  customerLastName = signal('');
  customerEmail = signal('');
  customerPhone = signal('');
  customerNotes = signal('');

  // Computed available services and employees
  services = computed(() => {
    const business = this.tenantService.getCurrentBusiness();
    return business?.services || [];
  });

  availableEmployees = computed(() => {
    const business = this.tenantService.getCurrentBusiness();
    const allEmployees = business?.employees || [];
    const currentSchedules = this.schedules();

    // Filter to employees with schedules who are not on vacation
    return allEmployees.filter(emp => {
      const schedule = currentSchedules.find(s => s.userId === emp.userId);
      if (!schedule) return false;

      // Check if employee is on vacation
      if (schedule.vacationFrom && schedule.vacationTo) {
        const now = new Date();
        const vacationStart = new Date(schedule.vacationFrom);
        const vacationEnd = new Date(schedule.vacationTo);
        if (now >= vacationStart && now <= vacationEnd) return false;
      }

      return true;
    });
  });

  // Generate next 30 days for date selection
  availableDates = computed(() => {
    const dates: Date[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    for (let i = 0; i < 30; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() + i);
      dates.push(date);
    }

    return dates;
  });

  constructor() {
    // Set Hungarian locale for calendar
    L10n.load({
      'hu': {
        'calendar': {
          'today': 'Ma'
        },
        'days': {
          'short': ['V', 'H', 'K', 'Sz', 'Cs', 'P', 'Sz']
        }
      }
    });

    // Load available dates when employee and service are selected
    effect(() => {
      const employee = this.selectedEmployee();
      const service = this.selectedService();

      if (employee && service) {
        this.loadAvailableDates();
      }
    });

    // Load slots when date is selected
    effect(() => {
      const date = this.selectedDate();
      const employee = this.selectedEmployee();
      const service = this.selectedService();

      if (date && employee && service) {
        this.loadAvailableSlots();
      }
    });
  }

  ngOnInit() {
    // Load schedules
    this.tenantService.business$.subscribe(business => {
      if (business?.id) {
        this.loadSchedules(business.id);
      }
    });

    // Check for pre-selected service or employee from query params
    this.route.queryParams.subscribe(params => {
      const serviceId = params['serviceId'];
      const employeeId = params['employeeId'];

      if (serviceId) {
        const service = this.services().find(s => s.id === serviceId);
        if (service) {
          this.selectedService.set(service);
          this.currentStep.set('employee');
        }
      }

      if (employeeId) {
        // Wait for employees to load before setting selected employee
        this.tenantService.business$.subscribe(business => {
          if (business?.employees) {
            const employee = business.employees.find(e => e.userId === employeeId);
            if (employee) {
              this.selectedEmployee.set(employee);
              console.log('[BookingFlow] Pre-selected employee:', employee);
            }
          }
        });
        // If employee is pre-selected, start with service selection
        this.currentStep.set('service');
      }
    });
  }

  private loadSchedules(businessId: string) {
    this.scheduleService.getSchedulesByBusiness(businessId).subscribe({
      next: (schedules) => {
        this.schedules.set(schedules);
      },
      error: (error) => {
        console.error('Error loading schedules:', error);
        this.toastService.error('Hiba történt az ütemezések betöltésekor');
      }
    });
  }

  private async loadAvailableDates() {
    const employee = this.selectedEmployee();
    const service = this.selectedService();
    const business = this.tenantService.getCurrentBusiness();

    if (!employee || !service || !business) return;

    this.isLoadingDates.set(true);
    const datesMap = new Map<string, boolean>();

    // Check availability for next 30 days
    const promises: Promise<void>[] = [];
    for (let i = 0; i < 30; i++) {
      const date = new Date();
      date.setDate(date.getDate() + i);
      date.setHours(0, 0, 0, 0);

      const utcDate = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
      const dateStr = utcDate.toISOString();
      const dateKey = date.toISOString().split('T')[0];

      const promise = this.scheduleService.getAvailableSlots(
        employee.userId,
        business.id,
        dateStr,
        service.duration
      ).toPromise().then(response => {
        datesMap.set(dateKey, response!.availableSlots.length > 0);
      }).catch(() => {
        datesMap.set(dateKey, false);
      });

      promises.push(promise);
    }

    await Promise.all(promises);
    this.availableDatesMap.set(datesMap);
    this.isLoadingDates.set(false);
  }

  private loadAvailableSlots() {
    const date = this.selectedDate();
    const employee = this.selectedEmployee();
    const service = this.selectedService();
    const business = this.tenantService.getCurrentBusiness();

    if (!date || !employee || !service || !business) return;

    this.isLoadingSlots.set(true);
    // Create UTC date string at midnight
    const utcDate = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
    const dateStr = utcDate.toISOString();

    this.scheduleService.getAvailableSlots(
      employee.userId,
      business.id,
      dateStr,
      service.duration
    ).subscribe({
      next: (response) => {
        this.availableSlots.set(response.availableSlots);
        this.isLoadingSlots.set(false);
      },
      error: (error) => {
        console.error('Error loading slots:', error);
        this.toastService.error('Hiba történt az időpontok betöltésekor');
        this.isLoadingSlots.set(false);
      }
    });
  }

  // Navigation methods
  selectService(service: Service) {
    this.selectedService.set(service);
    // If employee already selected, go to datetime, otherwise go to employee selection
    if (this.selectedEmployee()) {
      this.currentStep.set('datetime');
    } else {
      this.currentStep.set('employee');
    }
  }

  selectEmployee(employee: Employee) {
    this.selectedEmployee.set(employee);
    // If service already selected, go to datetime, otherwise go to service selection
    if (this.selectedService()) {
      this.currentStep.set('datetime');
    } else {
      this.currentStep.set('service');
    }
  }

  selectDate(date: Date) {
    this.selectedDate.set(date);
    this.selectedTime.set(null); // Reset time when date changes
  }

  selectTime(time: string) {
    this.selectedTime.set(time);
  }

  goToCustomerInfo() {
    if (!this.selectedTime()) {
      this.toastService.warning('Kérlek válassz időpontot');
      return;
    }
    this.currentStep.set('customer');
  }

  goBack() {
    const steps: FlowStep[] = ['service', 'employee', 'datetime', 'customer'];
    const currentIndex = steps.indexOf(this.currentStep());
    if (currentIndex > 0) {
      this.currentStep.set(steps[currentIndex - 1]);
    }
  }

  submitBooking() {
    const service = this.selectedService();
    const employee = this.selectedEmployee();
    const date = this.selectedDate();
    const time = this.selectedTime();
    const business = this.tenantService.getCurrentBusiness();

    if (!service || !employee || !date || !time) {
      this.toastService.error('Hiányzó adatok');
      return;
    }

    if (!this.customerFirstName() || !this.customerLastName() ||
        !this.customerPhone()) {
      this.toastService.warning('Kérlek töltsd ki az összes kötelező mezőt (név és telefonszám)');
      return;
    }

    // Combine date and time
    const bookingDateTime = new Date(date);
    const [hours, minutes] = time.split(':').map(Number);
    bookingDateTime.setHours(hours, minutes, 0, 0);

    const booking: CreateBooking = {
      bookingDate: bookingDateTime.toISOString(),
      notes: this.customerNotes() || null,
      firstName: this.customerFirstName(),
      lastName: this.customerLastName(),
      email: this.customerEmail() || 'noemail@swazy.hu',
      phoneNumber: this.customerPhone(),
      businessServiceId: service.id,
      employeeId: employee.userId,
      bookedByUserId: null
    };

    this.isSubmitting.set(true);

    this.bookingService.createBooking(booking).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);

        // Navigate to confirmation page with confirmation code
        this.router.navigate(['/confirmation', response.confirmationCode]);
      },
      error: (error) => {
        console.error('Error creating booking:', error);

        // More specific error messages
        let errorMessage = 'Hiba történt a foglalás létrehozásakor. ';

        if (error.status === 400) {
          errorMessage += 'Ellenőrizd az adatokat és próbáld újra.';
        } else if (error.status === 409) {
          errorMessage += 'Ez az időpont már foglalt. Válassz másik időpontot.';
        } else if (error.status === 0) {
          errorMessage += 'Nem sikerült kapcsolódni a szerverhez.';
        } else {
          errorMessage += 'Kérlek próbáld újra később.';
        }

        this.toastService.error(errorMessage);
        this.isSubmitting.set(false);
      }
    });
  }

  // Utility methods
  formatDate(date: Date): string {
    return new Intl.DateTimeFormat('hu-HU', {
      month: 'long',
      day: 'numeric',
      weekday: 'short'
    }).format(date);
  }

  formatTime(timeSlot: string): string {
    const date = new Date(timeSlot);
    return date.toLocaleTimeString('hu-HU', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  isDateToday(date: Date): boolean {
    const today = new Date();
    return date.getDate() === today.getDate() &&
           date.getMonth() === today.getMonth() &&
           date.getFullYear() === today.getFullYear();
  }

  // Syncfusion calendar methods
  onCalendarChange(args: any) {
    if (args.value) {
      const dateKey = args.value.toISOString().split('T')[0];
      const datesMap = this.availableDatesMap();

      // Check if date is available
      if (datesMap.has(dateKey) && !datesMap.get(dateKey)) {
        this.toastService.warning('Ezen a napon nincs szabad időpont');
        return;
      }

      this.selectDate(args.value);
    }
  }

  onRenderDayCell(args: RenderDayCellEventArgs): void {
    if (!args.date) return;

    const dateKey = args.date.toISOString().split('T')[0];
    const datesMap = this.availableDatesMap();

    if (datesMap.has(dateKey) && !datesMap.get(dateKey)) {
      args.isDisabled = true;
    }
  }

  getBookingEndTime(): string {
    const time = this.selectedTime();
    const service = this.selectedService();
    if (!time || !service) return '';

    const [hours, minutes] = time.split(':').map(Number);
    const date = new Date();
    date.setHours(hours, minutes, 0, 0);
    date.setMinutes(date.getMinutes() + service.duration);

    return date.toLocaleTimeString('hu-HU', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
