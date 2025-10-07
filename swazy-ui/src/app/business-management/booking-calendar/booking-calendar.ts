import { Component, input, OnInit, ViewChild, signal, computed, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleModule, DayService, TimelineViewsService, ResizeService, DragAndDropService, ScheduleComponent, EventSettingsModel, ResourceDetails } from '@syncfusion/ej2-angular-schedule';
import { BookingDetails } from '../../models/booking.details';
import { Employee } from '../../models/employee';
import { EmployeeSchedule } from '../../models/employee-schedule';
import { AuthService } from '../../services/auth.service';

interface CalendarEvent {
  Id: string;
  Subject: string;
  StartTime: Date;
  EndTime: Date;
  EmployeeId: string;
  Description?: string;
  IsReadonly?: boolean;
}

interface EmployeeResource {
  Id: string;
  Text: string;
  Color: string;
}

@Component({
  selector: 'app-booking-calendar',
  standalone: true,
  imports: [CommonModule, ScheduleModule],
  providers: [DayService, TimelineViewsService, ResizeService, DragAndDropService],
  templateUrl: './booking-calendar.html',
  styleUrls: ['./booking-calendar.scss']
})
export class BookingCalendarComponent implements OnInit {
  @ViewChild('scheduleObj') scheduleObj?: ScheduleComponent;

  private authService = inject(AuthService);

  bookings = input.required<BookingDetails[]>();
  employees = input.required<Employee[]>();
  schedules = input.required<EmployeeSchedule[]>();

  selectedDate = signal<Date>(new Date());
  eventSettings = signal<EventSettingsModel>({ dataSource: [] });
  personalEventSettings = signal<EventSettingsModel>({ dataSource: [] });
  viewMode = signal<'personal' | 'full'>('personal');
  currentUserId = signal<string | null>(null);

  // Employee colors for visual distinction
  private employeeColors = [
    '#1aaa55', '#357cd2', '#e8a838', '#d93030', '#7d3c98',
    '#28b4c8', '#ff6b6b', '#4ecdc4', '#f39c12', '#9b59b6'
  ];

  // Computed properties for working employees on selected date
  workingEmployees = computed(() => {
    const date = this.selectedDate();
    const dayOfWeek = date.getDay();
    const allEmployees = this.employees();
    const allSchedules = this.schedules();

    console.log('Computing working employees for day:', dayOfWeek, 'Total employees:', allEmployees.length);

    return allEmployees.filter(emp => {
      const schedule = allSchedules.find(s => s.userId === emp.userId);
      if (!schedule) return false;

      // Check if employee is on vacation for the selected date
      if (schedule.vacationFrom && schedule.vacationTo) {
        const selectedDate = date;
        const vacationStart = new Date(schedule.vacationFrom);
        const vacationEnd = new Date(schedule.vacationTo);
        if (selectedDate >= vacationStart && selectedDate <= vacationEnd) return false;
      }

      const daySchedule = schedule.daySchedules.find(d => d.dayOfWeek === dayOfWeek);
      return daySchedule?.isWorkingDay || false;
    });
  });

  // Employee resources for the calendar
  employeeResources = computed<EmployeeResource[]>(() => {
    return this.workingEmployees().map((emp, index) => ({
      Id: emp.userId,
      Text: `${emp.firstName} ${emp.lastName}`,
      Color: this.employeeColors[index % this.employeeColors.length]
    }));
  });

  // Calendar events from bookings (for Full view - all employees)
  calendarEvents = computed<CalendarEvent[]>(() => {
    const date = this.selectedDate();
    const startOfDay = new Date(date);
    startOfDay.setHours(0, 0, 0, 0);
    const endOfDay = new Date(date);
    endOfDay.setHours(23, 59, 59, 999);

    const workingEmployeeIds = new Set(this.workingEmployees().map(e => e.userId));
    const allBookings = this.bookings();

    const filtered = allBookings.filter(booking => {
      const bookingDate = new Date(booking.startTime);
      const isInRange = bookingDate >= startOfDay && bookingDate <= endOfDay;
      const hasEmployee = booking.employeeId && workingEmployeeIds.has(booking.employeeId);
      return isInRange && hasEmployee;
    });

    return filtered.map(booking => ({
      Id: booking.id,
      Subject: `${booking.serviceName} - ${booking.firstName} ${booking.lastName}`,
      StartTime: new Date(booking.startTime),
      EndTime: new Date(booking.endTime),
      EmployeeId: booking.employeeId!,
      Description: booking.notes || '',
      IsReadonly: true
    }));
  });

  // Personal calendar events (for logged-in user only)
  personalCalendarEvents = computed<CalendarEvent[]>(() => {
    const userId = this.currentUserId();
    if (!userId) return [];

    const date = this.selectedDate();
    const startOfDay = new Date(date);
    startOfDay.setHours(0, 0, 0, 0);
    const endOfDay = new Date(date);
    endOfDay.setHours(23, 59, 59, 999);

    const allBookings = this.bookings();

    const filtered = allBookings.filter(booking => {
      const bookingDate = new Date(booking.startTime);
      const isInRange = bookingDate >= startOfDay && bookingDate <= endOfDay;
      const isMyBooking = booking.employeeId === userId;
      return isInRange && isMyBooking;
    });

    return filtered.map(booking => ({
      Id: booking.id,
      Subject: `${booking.serviceName} - ${booking.firstName} ${booking.lastName}`,
      StartTime: new Date(booking.startTime),
      EndTime: new Date(booking.endTime),
      EmployeeId: booking.employeeId!,
      Description: booking.notes || '',
      IsReadonly: true
    }));
  });

  constructor() {
    // Update event settings for Full view
    effect(() => {
      const events = this.calendarEvents();
      this.eventSettings.set({
        dataSource: events,
        fields: {
          id: 'Id',
          subject: { name: 'Subject' },
          startTime: { name: 'StartTime' },
          endTime: { name: 'EndTime' },
          description: { name: 'Description' }
        }
      });
    });

    // Update event settings for Personal view
    effect(() => {
      const events = this.personalCalendarEvents();
      this.personalEventSettings.set({
        dataSource: events,
        fields: {
          id: 'Id',
          subject: { name: 'Subject' },
          startTime: { name: 'StartTime' },
          endTime: { name: 'EndTime' },
          description: { name: 'Description' }
        }
      });
    });
  }

  // Set 24-hour format for the calendar
  private get timeFormat(): string {
    return 'HH:mm';
  }

  ngOnInit() {
    // Get current user ID
    const user = this.authService.getCurrentUser();
    if (user) {
      this.currentUserId.set(user.id);
    }
  }

  onDateChange(target: any) {
    if (target && target.value) {
      const newDate = new Date(target.value);
      this.selectedDate.set(newDate);
    }
  }

  onEventRendered(args: any) {
    const employeeId = args.data.EmployeeId;
    const resource = this.employeeResources().find(r => r.Id === employeeId);
    if (resource) {
      args.element.style.backgroundColor = resource.Color;
    }
  }

  // Get resource header template data
  getResourceHeaderContent(data: ResourceDetails): string {
    return data.resourceData['Text'] as string;
  }
}
