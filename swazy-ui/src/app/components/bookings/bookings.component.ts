import { Component, OnInit, OnDestroy } from '@angular/core'; // Import OnDestroy
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs'; // Import Subscription
import { BookingDataService, Booking } from '../../services/booking-data.service'; // Import service and interface
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
  providers: [DayService, WeekService, WorkWeekService, MonthService, AgendaService],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit, OnDestroy { // Implement OnDestroy

  public eventSettings: EventSettingsModel = {
    dataSource: [], // Initialize with empty array, will be populated by the service
    fields: { // Ensure this mapping matches the Booking interface properties
      id: 'Id',
      subject: { name: 'Subject', title: 'Service & Client' },
      startTime: { name: 'StartTime', title: 'From' },
      endTime: { name: 'EndTime', title: 'To' },
      description: { name: 'Description', title: 'Notes'},
      // Add other field mappings if they are part of your Booking interface and used by the schedule
      // e.g., isAllDay: 'IsAllDay', recurrenceRule: 'RecurrenceRule', categoryColor: 'CategoryColor'
    }
  };
  public currentView: View = 'Month';
  public views: View[] = ['Day', 'Week', 'WorkWeek', 'Month', 'Agenda'];
  public selectedDate: Date = new Date();

  private bookingsSubscription!: Subscription; // Definite assignment assertion or initialize in constructor

  constructor(private bookingDataService: BookingDataService) { }

  ngOnInit(): void {
    this.bookingsSubscription = this.bookingDataService.bookings$.subscribe(
      (bookings: Booking[]) => {
        console.log('Bookings received in BookingsComponent:', bookings);
        // The dataSource needs to be an array of objects.
        // If the Syncfusion schedule component is not updating,
        // it might be due to change detection. Re-assigning the eventSettings object
        // or its dataSource property usually triggers it.
        this.eventSettings = {
          ...this.eventSettings, // Preserve other settings like 'fields'
          dataSource: [...bookings] // Create a new array reference
        };

        // If the first booking has a date, set the calendar to it for better UX
        if (bookings.length > 0 && bookings[0].StartTime) {
           this.selectedDate = new Date(bookings[0].StartTime);
        }
      }
    );
  }

  ngOnDestroy(): void {
    if (this.bookingsSubscription) {
      this.bookingsSubscription.unsubscribe();
    }
  }
}
