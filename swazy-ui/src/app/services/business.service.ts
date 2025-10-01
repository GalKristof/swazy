import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Business } from '../models/business';
import { Service } from '../models/service';
import { BookingDetails } from '../models/booking.details';
import { ServiceDetails } from '../models/service.details';

@Injectable({
  providedIn: 'root'
})
export class BusinessService {
  private http = inject(HttpClient);

  /**
   * Update business information
   * Calls PUT /api/business
   */
  updateBusiness(business: Business): Observable<Business> {
    const url = `${environment.apiUrl}/business`;
    console.log('[BusinessService] Updating business:', business);
    return this.http.put<Business>(url, business);
  }

  /**
   * Add employee to business
   * Calls POST /api/business/{businessId}/employee
   */
  addEmployeeToBusiness(businessId: string, userEmail: string, role: string): Observable<Business> {
    const url = `${environment.apiUrl}/business/${businessId}/employee`;
    const payload = {
      userEmail,
      role
    };
    console.log('[BusinessService] Adding employee to business:', payload);
    return this.http.post<Business>(url, payload);
  }

  /**
   * Update employee role
   * Calls PATCH /api/business/{businessId}/employee/{userId}
   */
  updateEmployeeRole(businessId: string, userId: string, role: string): Observable<any> {
    const url = `${environment.apiUrl}/business/${businessId}/employee/${userId}`;
    const payload = { role };
    console.log('[BusinessService] Updating employee role:', { businessId, userId, role });
    return this.http.patch(url, payload);
  }

  /**
   * Remove employee from business
   * Calls DELETE /api/business/{businessId}/employee/{userId}
   */
  removeEmployeeFromBusiness(businessId: string, userId: string): Observable<any> {
    const url = `${environment.apiUrl}/business/${businessId}/employee/${userId}`;
    console.log('[BusinessService] Removing employee from business:', { businessId, userId });
    return this.http.delete(url);
  }

  /**
   * Load services for the current business
   * Calls GET /api/businessservice/business/{businessId}
   */
  loadBusinessServices(businessId: string): Observable<Service[]> {
    const url = `${environment.apiUrl}/businessservice/business/${businessId}`;
    console.log('[BusinessService] Loading services for business:', businessId);
    return this.http.get<Service[]>(url);
  }

  /**
   * Create new business service
   * Calls POST /api/businessservice
   */
  createBusinessService(businessId: string, serviceId: string, price: number, duration: number): Observable<Service> {
    const url = `${environment.apiUrl}/businessservice`;
    const payload = {
      businessId,
      serviceId,
      price,
      duration
    };
    console.log('[BusinessService] Creating business service:', payload);
    return this.http.post<Service>(url, payload);
  }

  /**
   * Update business service
   * Calls PUT /api/businessservice
   */
  updateBusinessService(id: string, price?: number, duration?: number): Observable<Service> {
    const url = `${environment.apiUrl}/businessservice`;
    const payload = {
      id,
      ...(price !== undefined && { price }),
      ...(duration !== undefined && { duration })
    };
    console.log('[BusinessService] Updating business service:', payload);
    return this.http.put<Service>(url, payload);
  }

  /**
   * Delete business service
   * Calls DELETE /api/businessservice/{id}
   */
  deleteBusinessService(id: string): Observable<Service> {
    const url = `${environment.apiUrl}/businessservice/${id}`;
    console.log('[BusinessService] Deleting business service:', id);
    return this.http.delete<Service>(url);
  }

  /**
   * Load bookings for the current business
   * Calls GET /api/booking/business/{businessId}
   * Returns BookingDetailsResponse from backend
   */
  loadBusinessBookings(businessId: string): Observable<BookingDetails[]> {
    const url = `${environment.apiUrl}/booking/business/${businessId}`;
    console.log('[BusinessService] Loading bookings for business:', businessId);
    return this.http.get<BookingDetails[]>(url);
  }

  /**
   * Cancel booking
   * Calls DELETE /api/booking/{id}
   */
  cancelBooking(id: string): Observable<any> {
    const url = `${environment.apiUrl}/booking/${id}`;
    console.log('[BusinessService] Canceling booking:', id);
    return this.http.delete(url);
  }

  /**
   * Load all available services (not business-specific)
   * Calls GET /api/service/all
   */
  loadAllServices(): Observable<ServiceDetails[]> {
    const url = `${environment.apiUrl}/service/all`;
    console.log('[BusinessService] Loading all available services');
    return this.http.get<ServiceDetails[]>(url);
  }
}
