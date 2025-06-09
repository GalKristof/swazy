import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { View, EventSettingsModel, DayService, WeekService, MonthService, AgendaService, ScheduleAllModule } from '@syncfusion/ej2-angular-schedule';

import { BusinessService } from '../services/business.service';
import { BookingService } from '../services/booking.service';
import { GetBusinessDto } from '../models/dto/business-dto.model';
import { BookingDetailsDto } from '../models/dto/booking-details-dto.model';
import { BookingComponent } from '../booking/booking.component';


@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ScheduleAllModule, // Syncfusion Schedule
    BookingComponent   // Import standalone BookingComponent here
  ],
  providers: [DayService, WeekService, MonthService, AgendaService], // For Syncfusion Scheduler
  templateUrl: './bookings.component.html',
  styleUrl: './bookings.component.scss'
})
export class BookingsComponent implements OnInit {
  allBusinessesForDropdown: GetBusinessDto[] = [];
  selectedBusinessIdForBookings: string = '';
  bookingsForSelectedBusiness: BookingDetailsDto[] = [];
  isLoadingBookings: boolean = false;

  schedulerView: View = 'Week';
  schedulerEventSettings: EventSettingsModel = { dataSource: [] };
  currentSchedulerDate: Date = new Date();

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private businessService: BusinessService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadAllBusinessesForDropdown();
    }
    this.currentSchedulerDate = new Date(); // Initialize current date for scheduler
  }

  loadAllBusinessesForDropdown(): void {
    this.businessService.getBusinesses().subscribe(
      data => {
        this.allBusinessesForDropdown = data;
      },
      error => {
        console.error('Error loading businesses for dropdown:', error);
      }
    );
  }

  onBusinessSelectedForBookings(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const businessId = selectElement.value;
    this.selectedBusinessIdForBookings = businessId; // This will be passed to app-booking

    this.bookingsForSelectedBusiness = [];
    this.schedulerEventSettings = { dataSource: [] };

    if (businessId) {
      this.loadBookingsForBusiness(businessId);
    }
  }

  loadBookingsForBusiness(businessId: string): void {
    if (!businessId) {
      this.bookingsForSelectedBusiness = [];
      this.schedulerEventSettings = { dataSource: [] };
      return;
    }
    this.isLoadingBookings = true;
    this.bookingService.getBookingsByBusinessId(businessId).subscribe({
      next: (bookings) => {
        this.bookingsForSelectedBusiness = bookings;
        this.schedulerEventSettings = {
          dataSource: bookings.map(booking => ({
            Id: booking.id,
            Subject: `${booking.serviceName} - ${booking.firstName} ${booking.lastName}`,
            StartTime: new Date(booking.startTime), // Ensure dates are Date objects
            EndTime: new Date(booking.endTime),     // Ensure dates are Date objects
          }))
        };
        this.isLoadingBookings = false;
      },
      error: (err) => {
        console.error('Error loading bookings for business:', businessId, err);
        this.isLoadingBookings = false;
        this.bookingsForSelectedBusiness = [];
        this.schedulerEventSettings = { dataSource: [] };
      }
    });
  }

  setSchedulerView(view: View): void {
    this.schedulerView = view;
  }

  // Handler for the (bookingCreated) event from BookingComponent
  handleBookingCreated(): void {
    if (this.selectedBusinessIdForBookings) {
      this.loadBookingsForBusiness(this.selectedBusinessIdForBookings);
    }
  }

  // Handler for the (bookingDateChanged) event from BookingComponent
  handleBookingDateChanged(newDate: Date): void {
    this.currentSchedulerDate = new Date(newDate); // Update scheduler date
    // Optional: if you want to switch view or refresh bookings when date changes from form
    // this.schedulerView = 'Day';
    // this.loadBookingsForBusiness(this.selectedBusinessIdForBookings);
  }
}
