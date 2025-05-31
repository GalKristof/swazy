import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // For form handling
import { CalendarModule } from '@syncfusion/ej2-angular-calendars'; // Syncfusion Calendar (DatePickerModule might be unused now)
import { BookingDataService, Booking } from '../../services/booking-data.service'; // Import service and interface

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
    // This mock data for employees might need to be centralized or made more dynamic in a real app
  };

  // Employee name to ID mapping (example)
  private employeeNameToIdMap: { [name: string]: number } = {
    'Márk': 1,
    'Eszter': 2,
    'Laura': 3,
  };

  constructor(private bookingDataService: BookingDataService) { }

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
      // Using non-null assertion operator as args.value ensures selectedDate is not null here
      const dateString = this.formatDate(this.selectedDate!);
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
      // const dateString = this.formatDate(this.selectedDate!); // Keep for alert if needed, or use formatted StartTime

      // 1. Create StartTime
      const [hours, minutes] = this.selectedTimeSlot.split(':').map(Number);
      const startTime = new Date(this.selectedDate!);
      startTime.setHours(hours, minutes, 0, 0);

      // 2. Calculate EndTime (e.g., fixed 45 minutes for now)
      const endTime = new Date(startTime.getTime() + 45 * 60000); // 45 minutes later

      // 3. Get EmployeeID
      let employeeId: number | undefined = undefined;
      if (this.assignedEmployee && this.employeeNameToIdMap[this.assignedEmployee]) {
        employeeId = this.employeeNameToIdMap[this.assignedEmployee];
      }

      // 4. Create Booking Object
      const newBooking: Booking = {
        Id: 0, // Service will assign ID
        Subject: `${this.selectedService} - ${this.fullName}`,
        StartTime: startTime,
        EndTime: endTime,
        EmployeeID: employeeId,
        Description: `Booked by ${this.fullName} via booking form. Employee: ${this.assignedEmployee || 'N/A'}`,
        CategoryColor: '#547597' // A default color for new bookings from this form
      };

      // 5. Add booking via service
      this.bookingDataService.addBooking(newBooking);

      // Comment out direct manipulation of local availableTimeSlots as per requirement
      // The source of truth for bookings is now the service.
      // Availability should ideally be derived from the service's data in a more advanced scenario.
      /*
      const dateStringForSlotRemoval = this.formatDate(this.selectedDate!);
      const index = this.availableTimeSlots[dateStringForSlotRemoval]?.indexOf(this.selectedTimeSlot);
      if (index !== undefined && index > -1) {
        this.availableTimeSlots[dateStringForSlotRemoval].splice(index, 1);
        // Update UI for immediate feedback (optional, as service should drive this)
        this.timeSlotsForSelectedDate = [...this.availableTimeSlots[dateStringForSlotRemoval]];
      }
      */

      // For immediate UI feedback, we might still want to refresh the local view of slots,
      // or better, have availableTimeSlots react to changes in bookingDataService.
      // For now, just clear the selected slot.
      const formattedStartTime = startTime.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit'});
      alert(`Booking for ${this.fullName} for service ${this.selectedService} on ${startTime.toLocaleDateString()} at ${formattedStartTime} has been requested and sent to the system.`);

      // Reset selection
      this.selectedTimeSlot = null;
      this.assignedEmployee = null;
      // Consider resetting fullName and selectedService, or navigating away,
      // or refreshing available slots based on the new booking.
      // this.fullName = '';
      // this.selectedDate = null; // This would clear dependent UI parts

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
