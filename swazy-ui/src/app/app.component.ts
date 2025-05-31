import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BookingComponent } from './components/booking/booking.component';
import { BookingsComponent } from './components/bookings/bookings.component'; // Import BookingsComponent

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, BookingComponent, BookingsComponent], // Add BookingsComponent here
  providers: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
}
