import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

// Define a Booking interface for more structured data
export interface Booking {
  Id: number;
  Subject: string;
  StartTime: Date;
  EndTime: Date;
  EmployeeID?: number; // Optional: if you want to track who is assigned
  Description?: string; // Optional: for additional notes
  CategoryColor?: string; // Optional: for UI theming in the scheduler
  [key: string]: any; // Allow other properties if needed by Syncfusion components
}

@Injectable({
  providedIn: 'root'
})
export class BookingDataService {

  private initialBookings: Booking[] = [
    {
      Id: 1,
      Subject: 'Hajvágás - Kiss János (Service)',
      StartTime: new Date(2024, 6, 28, 10, 0), // Month is 0-indexed (6 = July)
      EndTime: new Date(2024, 6, 28, 10, 45),
      EmployeeID: 1,
      Description: 'Regular haircut for Mr. Kiss from service.',
      CategoryColor: '#8FBC8F' // DarkSeaGreen
    },
    {
      Id: 2,
      Subject: 'Szakáll igazítás - Nagy Béla (Service)',
      StartTime: new Date(2024, 6, 29, 14, 0),
      EndTime: new Date(2024, 6, 29, 14, 30),
      EmployeeID: 2,
      Description: 'Beard trim and shaping from service.',
      CategoryColor: '#ADD8E6' // LightBlue
    },
    {
      Id: 3,
      Subject: 'Gyerek hajvágás - Kovács Pisti (Service)',
      StartTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 4, 9, 0),
      EndTime: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate() + 4, 9, 30),
      EmployeeID: 1,
      CategoryColor: '#90EE90' // LightGreen
    },
  ];

  private bookingsSubject = new BehaviorSubject<Booking[]>(this.initialBookings);
  public bookings$: Observable<Booking[]> = this.bookingsSubject.asObservable();

  constructor() { }

  public addBooking(booking: Booking): void {
    const currentBookings = this.bookingsSubject.value;
    // Simple ID generation for new bookings; in a real app, this would be more robust (e.g., UUID or backend generated)
    const newId = currentBookings.length > 0 ? Math.max(...currentBookings.map(b => b.Id)) + 1 : 1;
    const newBookingWithId = { ...booking, Id: newId };

    this.bookingsSubject.next([...currentBookings, newBookingWithId]);
    console.log('Booking added via service:', newBookingWithId);
    console.log('All bookings in service:', this.bookingsSubject.value);
  }

  public getBookings(): Booking[] {
    return this.bookingsSubject.value;
  }

  // Optional: Method to get a booking by ID
  public getBookingById(id: number): Booking | undefined {
    return this.bookingsSubject.value.find(b => b.Id === id);
  }

  // Optional: Method to update a booking
  public updateBooking(updatedBooking: Booking): void {
    const currentBookings = this.bookingsSubject.value;
    const index = currentBookings.findIndex(b => b.Id === updatedBooking.Id);
    if (index !== -1) {
      const bookings = [...currentBookings];
      bookings[index] = updatedBooking;
      this.bookingsSubject.next(bookings);
    }
  }

  // Optional: Method to remove a booking
  public removeBooking(id: number): void {
    const currentBookings = this.bookingsSubject.value;
    this.bookingsSubject.next(currentBookings.filter(b => b.Id !== id));
  }
}
