import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ScheduleModule,
  DayService,
  WeekService,
  WorkWeekService,
  MonthService,
  AgendaService,
  EventSettingsModel,
  View
} from '@syncfusion/ej2-angular-schedule';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, ScheduleModule],
  providers: [DayService, WeekService, WorkWeekService, MonthService, AgendaService], // Services for different views
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit {

  public eventSettings: EventSettingsModel = {};
  public currentView: View = 'Month'; // Default view
  public views: View[] = ['Day', 'Week', 'WorkWeek', 'Month', 'Agenda'];
  public selectedDate: Date = new Date(); // Default to today

  private mockBookings: Record<string, any>[] = [
    {
      Id: 1,
      Subject: 'Hajvágás - Kiss János',
      StartTime: new Date(2024, 6, 28, 10, 0), // Note: Month is 0-indexed (6 = July)
      EndTime: new Date(2024, 6, 28, 10, 45),
      EmployeeID: 1, // Example: Corresponds to 'Márk'
      Description: 'Regular haircut for Mr. Kiss.',
      CategoryColor: '#1aaa55'
    },
    {
      Id: 2,
      Subject: 'Szakáll igazítás - Nagy Béla',
      StartTime: new Date(2024, 6, 29, 14, 0),
      EndTime: new Date(2024, 6, 29, 14, 30),
      EmployeeID: 2, // Example: Corresponds to 'Eszter'
      Description: 'Beard trim and shaping.',
      CategoryColor: '#357cd2'
    },
    {
      Id: 3,
      Subject: 'Gyerek hajvágás - Kovács Pisti',
      StartTime: new Date(2024, 6, 30, 9, 0),
      EndTime: new Date(2024, 6, 30, 9, 30),
      EmployeeID: 1,
      CategoryColor: '#7fa900'
    },
    {
      Id: 4,
      Subject: 'Festés - Horváth Anna',
      StartTime: new Date(2024, 7, 1, 11, 0), // August 1st
      EndTime: new Date(2024, 7, 1, 13, 0),
      EmployeeID: 3, // Example: Corresponds to 'Laura'
      Description: 'Full hair coloring service.',
      IsAllDay: false,
      CategoryColor: '#ea7a57'
    },
     {
      Id: 5,
      Subject: 'Hajvágás - Varga Dávid',
      // Using current year and month for relevance
      StartTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 2, 15, 0),
      EndTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 2, 15, 45),
      EmployeeID: 2,
      CategoryColor: '#1aaa55'
    },
    {
      Id: 6,
      Subject: 'Szakáll igazítás - Fekete László',
      StartTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 3, 16, 0),
      EndTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 3, 16, 30),
      EmployeeID: 1,
      CategoryColor: '#357cd2'
    }
  ];

  constructor() { }

  ngOnInit(): void {
    this.eventSettings = {
      dataSource: this.mockBookings,
      fields: {
        id: 'Id',
        subject: { name: 'Subject', title: 'Service & Client' },
        startTime: { name: 'StartTime', title: 'From' },
        endTime: { name: 'EndTime', title: 'To' },
        description: { name: 'Description', title: 'Notes'},
        // You can add more fields like IsAllDay, RecurrenceRule, etc.
      }
    };
    // Adjust selectedDate to be near some bookings if needed, or just use current date
    if (this.mockBookings.length > 0) {
        // this.selectedDate = this.mockBookings[0].StartTime as Date;
    }
  }
}
