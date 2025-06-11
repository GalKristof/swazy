import { Component, OnInit } from '@angular/core';
import {
  AgendaService,
  DayService,
  EventSettingsModel, MonthAgendaService, MonthService,
  RecurrenceEditorModule,
  ScheduleModule,
  View,
  WeekService,
  WorkHoursModel,
  WorkWeekService
} from '@syncfusion/ej2-angular-schedule';

@Component({
  selector: 'app-employee-scheduler',
  standalone: true,
  templateUrl: './employee-scheduler.component.html',
  styleUrls: ['./employee-scheduler.component.scss'],
  imports: [
    ScheduleModule,
    RecurrenceEditorModule
  ],
  providers: [
    DayService,
    WeekService,
    WorkWeekService,
    MonthService,
    AgendaService,
    MonthAgendaService
  ]
})
export class EmployeeSchedulerComponent implements OnInit {

  public currentView: View = 'Week';
  public views: View[] = ['Week'];

  public eventSettings: EventSettingsModel = {
    dataSource: [],
    fields: {
      id: 'Id',
      subject: { name: 'EmployeeName' },
      startTime: { name: 'StartTime' },
      endTime: { name: 'EndTime' },
      description: { name: 'Description' }
    }
  };

  public workHours: WorkHoursModel = {
    highlight: true,
    start: '06:00',
    end: '22:00'
  };

  // Configure 24-hour time format
  public timeFormat: string = 'HH:mm';

  // Set Monday as first day of week (European standard)
  public firstDayOfWeek: number = 1;

  ngOnInit(): void {
    // Initialize data after component loads
    this.eventSettings.dataSource = this.getEmployeeScheduleData();
  }

  // Helper method to get day names without dates (for template)
  public getDayName(date: Date): string {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    return days[date.getDay()];
  }

  // John Doe schedule data
  private getEmployeeScheduleData(): any[] {
    const baseDate = new Date();
    const startOfWeek = new Date(baseDate.setDate(baseDate.getDate() - baseDate.getDay()));

    return [
      // Monday: 9-17 (9 AM - 5 PM)
      {
        Id: 1,
        EmployeeName: 'John Doe',
        StartTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 1, 9, 0), // Monday
        EndTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 1, 17, 0),
        Description: 'Regular shift'
      },
      // Tuesday: 9-17 (9 AM - 5 PM)
      {
        Id: 2,
        EmployeeName: 'John Doe',
        StartTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 2, 9, 0), // Tuesday
        EndTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 2, 17, 0),
        Description: 'Regular shift'
      },
      // Wednesday: OFF DAY (no entry)

      // Thursday: 10-19 (10 AM - 7 PM)
      {
        Id: 3,
        EmployeeName: 'John Doe',
        StartTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 4, 10, 0), // Thursday
        EndTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 4, 19, 0),
        Description: 'Regular shift'
      },
      // Friday: 10-19 (10 AM - 7 PM)
      {
        Id: 4,
        EmployeeName: 'John Doe',
        StartTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 5, 10, 0), // Friday
        EndTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + 5, 19, 0),
        Description: 'Regular shift'
      },
      // Sunday: 10-16 (10 AM - 4 PM)
      {
        Id: 5,
        EmployeeName: 'John Doe',
        StartTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate(), 10, 0), // Sunday
        EndTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate(), 16, 0),
        Description: 'Weekend shift'
      }
    ];
  }

  // Method to add new employee schedule
  public addEmployeeShift(employeeName: string, dayOfWeek: number, startHour: number, endHour: number): void {
    const baseDate = new Date();
    const startOfWeek = new Date(baseDate.setDate(baseDate.getDate() - baseDate.getDay()));

    const newShift = {
      Id: Date.now(), // Simple ID generation
      EmployeeName: employeeName,
      StartTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + dayOfWeek, startHour, 0),
      EndTime: new Date(startOfWeek.getFullYear(), startOfWeek.getMonth(), startOfWeek.getDate() + dayOfWeek, endHour, 0),
      Description: 'Scheduled shift'
    };

    // Add to data source and refresh
    this.eventSettings.dataSource = [...(this.eventSettings.dataSource as any[]), newShift];
  }
}
