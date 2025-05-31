import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // For form handling
import { CalendarModule, DatePickerModule } from '@syncfusion/ej2-angular-calendars'; // Syncfusion Calendar

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CalendarModule,
    // DatePickerModule might also be useful for date inputs if needed later
  ],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.scss']
})
export class BookingComponent implements OnInit {

  public fullName: string = '';
  public services: string[] = ['Hajvágás', 'Szakáll igazítás', 'Gyerek hajvágás', 'Festés'];
  public selectedService: string = '';

  public selectedDate: Date | null = null;
  public today: Date = new Date();
  public minDate: Date = new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate()); // Today
  public maxDate: Date = new Date(this.today.getFullYear(), this.today.getMonth() + 2, this.today.getDate()); // Approx 2 months out

  // Mock unavailable dates - e.g., tomorrow is fully booked
  public unavailableDates: Date[] = [
    new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate() + 1)
  ];

  public availableTimeSlots: { [key: string]: string[] } = {
    // Default empty, to be populated when a date is selected
    // Example structure:
    // '2024-07-30': ['10:00', '10:30', '14:00']
  };

  public timeSlotsForSelectedDate: string[] = [];
  public selectedTimeSlot: string | null = null;
  public assignedEmployee: string | null = null;

  // Mock employees for time slots
  private employeeAssignments: { [key: string]: { [time: string]: string } } = {
    // Example: '2024-07-30': { '10:00': 'Anna', '10:30': 'Péter', '14:00': 'Anna' }
  };

  constructor() { }

  ngOnInit(): void {
    this.selectedService = this.services[0]; // Default to first service
    this.generateInitialTimeSlotsAndAssignments();
  }

  private generateInitialTimeSlotsAndAssignments(): void {
    // Generate some mock available slots and employee assignments for the next few available days
    for (let i = 0; i < 7; i++) { // Generate for the next 7 days
      const date = new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate() + i);
      const dateString = this.formatDate(date);

      // Skip if it's an "unavailable day" like tomorrow
      if (this.isDateUnavailable(date)) continue;

      // Skip weekends (Sunday = 0, Saturday = 6)
      if (date.getDay() === 0 || date.getDay() === 6) {
        this.availableTimeSlots[dateString] = [];
        this.employeeAssignments[dateString] = {};
        continue;
      }

      this.availableTimeSlots[dateString] = ['09:00', '09:30', '10:00', '10:30', '11:00', '14:00', '14:30', '15:00'];
      this.employeeAssignments[dateString] = {
        '09:00': 'Márk',
        '09:30': 'Eszter',
        '10:00': 'Márk',
        '10:30': 'Laura',
        '11:00': 'Eszter',
        '14:00': 'Márk',
        '14:30': 'Laura',
        '15:00': 'Eszter',
      };
    }
  }

  public onDateChange(args: any): void {
    if (args.value) {
      this.selectedDate = args.value;
      const dateString = this.formatDate(this.selectedDate);
      this.timeSlotsForSelectedDate = this.availableTimeSlots[dateString] || [];
      this.selectedTimeSlot = null;
      this.assignedEmployee = null;
    } else {
      this.selectedDate = null;
      this.timeSlotsForSelectedDate = [];
      this.selectedTimeSlot = null;
      this.assignedEmployee = null;
    }
  }

  public isDateUnavailable(date: Date): boolean {
    return this.unavailableDates.some(
      unavailableDate => unavailableDate.toDateString() === date.toDateString()
    );
  }

  // Custom renderDayCell event for Syncfusion Calendar
  public onRenderDayCell(args: any): void {
    const date: Date = args.date;
    // Disable past dates (redundant if minDate is set, but good practice)
    if (date < this.minDate) {
      args.isDisabled = true;
    }
    // Disable specific "unavailable" dates
    if (this.isDateUnavailable(date)) {
      args.isDisabled = true;
      // Optionally add a class to style it differently
      // args.element.classList.add('e-disabled-date');
    }
    // Disable weekends
    if (date.getDay() === 0 || date.getDay() === 6) { // Sunday or Saturday
        args.isDisabled = true;
        // args.element.classList.add('e-weekend-disabled');
    }
  }

  public selectTimeSlot(time: string): void {
    this.selectedTimeSlot = time;
    if (this.selectedDate) {
      const dateString = this.formatDate(this.selectedDate);
      this.assignedEmployee = this.employeeAssignments[dateString]?.[time] || 'N/A';
    }
  }

  public bookAppointment(): void {
    if (this.selectedDate && this.selectedTimeSlot && this.fullName && this.selectedService) {
      const dateString = this.formatDate(this.selectedDate);

      // Remove the booked time slot
      const index = this.availableTimeSlots[dateString].indexOf(this.selectedTimeSlot);
      if (index > -1) {
        this.availableTimeSlots[dateString].splice(index, 1);
      }

      // Update UI
      this.timeSlotsForSelectedDate = [...this.availableTimeSlots[dateString]];

      alert(`Booking confirmed for ${this.fullName} for service ${this.selectedService} on ${dateString} at ${this.selectedTimeSlot} with ${this.assignedEmployee}!`);

      // Reset selection
      this.selectedTimeSlot = null;
      this.assignedEmployee = null;
      // Potentially reset date or fullName depending on desired UX
      // this.selectedDate = null;
      // this.fullName = '';


    } else {
      alert('Please fill in all details and select a date and time slot.');
    }
  }

  private formatDate(date: Date): string {
    // Simple YYYY-MM-DD format
    const year = date.getFullYear();
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const day = date.getDate().toString().padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
