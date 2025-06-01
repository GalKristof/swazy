import { Component, OnInit, OnDestroy } from '@angular/core'; // Import OnDestroy
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs'; // Import Subscription
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
import {Booking} from '../../models/booking/booking.model';
import {BookingService} from '../../services/booking.service';

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
    dataSource: [],
    fields: {
      id: 'Id',
      subject: { name: 'Subject', title: 'Service & Client' },
      startTime: { name: 'StartTime', title: 'From' },
      endTime: { name: 'EndTime', title: 'To' },
      description: { name: 'Description', title: 'Notes'},
    }
  };
  public currentView: View = 'Month';
  public views: View[] = ['Day', 'Week', 'WorkWeek', 'Month', 'Agenda'];
  public selectedDate: Date = new Date();

  private bookingsSubscription!: Subscription; // Definite assignment assertion or initialize in constructor

  constructor(private bookingService: BookingService) { }

  ngOnInit(): void {
    this.bookingsSubscription = this.bookingService.bookings$.subscribe(
      (bookings: Booking[]) => {
        console.log('Bookings received in BookingsComponent:', bookings);
        this.eventSettings = {
          ...this.eventSettings,
          dataSource: [...bookings]
        };

        // If the first booking has a date, set the calendar to it for better UX
        if (bookings.length > 0 && bookings[0].bookingDate) {
           this.selectedDate = new Date(bookings[0].bookingDate);
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
