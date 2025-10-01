import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingDetails } from '../../models/booking.details';

@Component({
  selector: 'app-booking-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './booking-list.html'
})
export class BookingListComponent {
  bookings = input.required<BookingDetails[]>();

  bookingCancelled = output<string>();

  isCancelling: { [key: string]: boolean } = {};

  cancelBooking(id: string) {
    if (!this.isCancelling[id]) {
      this.isCancelling[id] = true;
      this.bookingCancelled.emit(id);
    }
  }

  onCancelComplete(id: string) {
    this.isCancelling[id] = false;
  }

  onCancelError(id: string) {
    this.isCancelling[id] = false;
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('hu-HU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
