import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {CreateBookingDto, UpdateBookingDto} from '../models/dto/booking-dto.model';
import { Booking } from '../models/booking/booking.model';
import { BookingDetailsDto } from '../models/dto/booking-details-dto.model';

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private apiUrl = `${environment.apiUrl}/booking`;

  private bookingsSubject = new BehaviorSubject<Booking[]>([]);
  public bookings$ = this.bookingsSubject.asObservable();

  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadAllBookings();
  }

  public loadAllBookings(): void {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    this.http.get<Booking[]>(`${this.apiUrl}/all`)
      .pipe(
        tap(() => this.loadingSubject.next(false)),
        catchError(this.handleError.bind(this))
      )
      .subscribe({
        next: (bookings) => {
          this.bookingsSubject.next(bookings);
        },
        error: (error) => {
          this.loadingSubject.next(false);
          this.errorSubject.next(error);
        }
      });
  }

  public createBooking(booking: CreateBookingDto): Observable<Booking> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.post<Booking>(this.apiUrl, booking)
      .pipe(
        tap(() => this.loadingSubject.next(false)),
        tap(newBooking => {
          const currentBookings = this.bookingsSubject.value;
          this.bookingsSubject.next([...currentBookings, newBooking]);
        }),
        catchError(this.handleError.bind(this))
      );
  }

  public updateBooking(booking: UpdateBookingDto): Observable<Booking> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.put<Booking>(this.apiUrl, booking)
      .pipe(
        tap(() => this.loadingSubject.next(false)),
        tap(updatedBooking => {
          const bookings = this.bookingsSubject.value;
          const index = bookings.findIndex(b => b.id === updatedBooking.id);
          if (index !== -1) {
            bookings[index] = updatedBooking;
            this.bookingsSubject.next([...bookings]);
          }
        }),
        catchError(this.handleError.bind(this))
      );
  }

  public deleteBooking(id: string): Observable<Booking> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.delete<Booking>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => this.loadingSubject.next(false)),
        tap(deletedBooking => {
          const bookings = this.bookingsSubject.value;
          this.bookingsSubject.next(bookings.filter(b => b.id !== id));
        }),
        catchError(this.handleError.bind(this))
      );
  }

  public getBookingById(id: string): Observable<Booking | undefined> {
    return this.bookings$.pipe(
      map(bookings => bookings.find(booking => booking.id === id))
    );
  }

  public getBookingsByBusinessId(businessId: string): Observable<BookingDetailsDto[]> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    // Use the correct plural 'bookings' and full path for this specific endpoint
    const requestUrl = `${environment.apiUrl}/bookings/business/${businessId}`;

    return this.http.get<any[]>(requestUrl) // Get as any[] first for mapping
      .pipe(
        map((bookingsFromApi: any[]) =>
          bookingsFromApi.map(booking => ({
            ...booking,
            startTime: new Date(booking.startTime),
            endTime: new Date(booking.endTime)
          } as BookingDetailsDto))
        ),
        tap(() => {
          this.loadingSubject.next(false);
        }),
        catchError(this.handleError.bind(this))
      );
  }

  public clearError(): void {
    this.errorSubject.next(null);
  }

  private handleError(error: any): Observable<never> {
    let errorMessage = 'An error occurred';

    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.message}`;
    }

    this.errorSubject.next(errorMessage);
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
