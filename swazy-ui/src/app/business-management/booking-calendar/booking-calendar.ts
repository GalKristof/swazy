import { Component, input, OnInit, ViewChild, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScheduleModule, DayService, TimelineViewsService, ResizeService, DragAndDropService, ScheduleComponent, EventSettingsModel, ResourceDetails } from '@syncfusion/ej2-angular-schedule';
import { BookingDetails } from '../../models/booking.details';
import { Employee } from '../../models/employee';
import { EmployeeSchedule } from '../../models/employee-schedule';

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

  bookings = input.required<BookingDetails[]>();
  employees = input.required<Employee[]>();
  schedules = input.required<EmployeeSchedule[]>();

  selectedDate = signal<Date>(new Date());
  eventSettings = signal<EventSettingsModel>({ dataSource: [] });

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

  // Calendar events from bookings
  calendarEvents = computed<CalendarEvent[]>(() => {
    const date = this.selectedDate();
    const startOfDay = new Date(date);
    startOfDay.setHours(0, 0, 0, 0);
    const endOfDay = new Date(date);
    endOfDay.setHours(23, 59, 59, 999);

    const workingEmployeeIds = new Set(this.workingEmployees().map(e => e.userId));
    const allBookings = this.bookings();

    console.log('All bookings:', allBookings.length);
    console.log('Working employee IDs:', Array.from(workingEmployeeIds));

    const filtered = allBookings.filter(booking => {
      const bookingDate = new Date(booking.startTime);
      const isInRange = bookingDate >= startOfDay && bookingDate <= endOfDay;
      const hasEmployee = booking.employeeId && workingEmployeeIds.has(booking.employeeId);

      if (!hasEmployee && booking.employeeId) {
        console.log('Booking has employeeId but not in working employees:', booking.employeeId);
      }

      return isInRange && hasEmployee;
    });

    console.log('Filtered bookings for calendar:', filtered.length);

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
    // Update event settings when calendar events change
    effect(() => {
      const events = this.calendarEvents();
      console.log('Calendar events changed:', events.length);
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
  }

  // Set 24-hour format for the calendar
  private get timeFormat(): string {
    return 'HH:mm';
  }

  ngOnInit() {
    console.log('Calendar initialized with bookings:', this.bookings().length);
    console.log('Employees:', this.employees().length);
    console.log('Schedules:', this.schedules().length);
  }

  onDateChange(target: any) {
    if (target && target.value) {
      const newDate = new Date(target.value);
      console.log('Date changed to:', newDate);
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
