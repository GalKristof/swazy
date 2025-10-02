import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { BookingService } from '../services/booking.service';
import { BookingDetailsResponse } from '../models/booking';

@Component({
  selector: 'app-booking-confirmation',
  imports: [CommonModule],
  templateUrl: './booking-confirmation.html',
  styleUrl: './booking-confirmation.scss'
})
export class BookingConfirmation implements OnInit {
  confirmationCode = '';
  businessName = '';
  businessAddress = '';
  businessPhone = '';
  serviceName = '';
  duration = 0;
  employeeName = '';
  bookingDate = '';
  bookingTime = '';
  customerName = '';
  customerEmail = '';
  customerPhone = '';
  price = 0;
  isLoading = true;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private bookingService: BookingService
  ) {}

  ngOnInit() {
    // Smooth scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });

    // Get confirmation code from route parameter
    const code = this.route.snapshot.paramMap.get('code');

    if (!code) {
      this.errorMessage = 'Nincs megerősítési kód megadva.';
      this.isLoading = false;
      return;
    }

    this.confirmationCode = code;

    // Fetch booking details from backend
    this.bookingService.getBookingByConfirmationCode(code).subscribe({
      next: (booking: BookingDetailsResponse) => {
        this.populateBookingData(booking);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching booking:', error);
        this.errorMessage = 'Nem található foglalás ezzel a megerősítési kóddal.';
        this.isLoading = false;
      }
    });
  }

  private populateBookingData(booking: BookingDetailsResponse) {
    this.businessName = booking.businessName;
    this.businessAddress = booking.businessAddress || '';
    this.businessPhone = booking.businessPhone || '';
    this.serviceName = booking.serviceName;
    this.duration = booking.serviceDuration;
    this.employeeName = booking.employeeName || '';
    this.customerName = `${booking.lastName} ${booking.firstName}`;
    this.customerEmail = booking.email;
    this.customerPhone = booking.phoneNumber;
    this.price = booking.servicePrice;

    // Format date and time
    const startDate = new Date(booking.startTime);
    const endDate = new Date(booking.endTime);

    this.bookingDate = new Intl.DateTimeFormat('hu-HU', {
      month: 'long',
      day: 'numeric',
      weekday: 'short'
    }).format(startDate);

    const startTime = startDate.toLocaleTimeString('hu-HU', {
      hour: '2-digit',
      minute: '2-digit'
    });

    const endTime = endDate.toLocaleTimeString('hu-HU', {
      hour: '2-digit',
      minute: '2-digit'
    });

    this.bookingTime = `${startTime} - ${endTime}`;
  }
}
