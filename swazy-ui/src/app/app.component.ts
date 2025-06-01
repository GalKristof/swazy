import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingComponent } from './components/booking/booking.component';
import { BookingsComponent } from './components/bookings/bookings.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, BookingComponent, BookingsComponent],
  providers: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
}
