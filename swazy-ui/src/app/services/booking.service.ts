import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BookingDetailsResponse } from '../models/booking';

export interface CreateBooking {
  bookingDate: string; // ISO DateTime string
  notes: string | null;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  businessServiceId: string;
  employeeId: string | null;
  bookedByUserId: string | null;
}

export interface BookingResponse {
  id: string;
  confirmationCode: string;
  bookingDate: string;
  notes: string | null;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  businessServiceId: string;
  employeeId: string | null;
  bookedByUserId: string | null;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class BookingService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/booking`;

  /**
   * Create a new booking
   * POST /api/booking
   */
  createBooking(booking: CreateBooking): Observable<BookingResponse> {
    console.log('[BookingService] Creating booking:', booking);
    return this.http.post<BookingResponse>(this.baseUrl, booking);
  }

  /**
   * Get booking by confirmation code
   * GET /api/booking/confirmation/{code}
   */
  getBookingByConfirmationCode(code: string): Observable<BookingDetailsResponse> {
    console.log('[BookingService] Fetching booking with code:', code);
    return this.http.get<BookingDetailsResponse>(`${this.baseUrl}/confirmation/${code}`);
  }
}
