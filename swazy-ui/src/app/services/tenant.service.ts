import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {Business} from '../models/business';
import {Service} from '../models/service';
import {BookingDetails} from '../models/booking.details';
import {ServiceDetails} from '../models/service.details';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private http = inject(HttpClient);

  private businessSubject = new BehaviorSubject<Business | null>(null);
  public business$ = this.businessSubject.asObservable();

  /**
   * Load business data based on environment configuration
   * - Development: Uses hardcoded tenantId
   * - Production: Extracts domain from URL
   */
  loadBusinessData(): Observable<Business> {
    let url: string;

    if (environment.fromDomain) {
      // PRODUCTION: Get domain from URL (not implemented in backend yet)
      const domain = window.location.hostname;
      console.log('[TenantService] Loading business by domain:', domain);
      url = `${environment.apiUrl}/business/by-domain/${domain}`;
    } else {
      // DEVELOPMENT: Use hardcoded tenant ID - calls GET /api/business/{id}
      console.log('[TenantService] Loading business by ID:', environment.tenantId);
      url = `${environment.apiUrl}/business/${environment.tenantId}`;
    }

    return this.http.get<Business>(url).pipe(
      tap(business => {
        console.log('[TenantService] Business data loaded:', business);
        this.businessSubject.next(business);
      })
    );
  }

  /**
   * Get current business data (synchronous)
   */
  getCurrentBusiness(): Business | null {
    return this.businessSubject.value;
  }

  /**
   * Load services for the current business
   * Calls GET /api/businessservice/business/{businessId}
   */
  loadBusinessServices(businessId: string): Observable<Service[]> {
    const url = `${environment.apiUrl}/businessservice/business/${businessId}`;
    console.log('[TenantService] Loading services for business:', businessId);
    return this.http.get<Service[]>(url);
  }

  /**
   * Load bookings for the current business
   * Calls GET /api/booking/business/{businessId}
   * Returns BookingDetailsResponse from backend
   */
  loadBusinessBookings(businessId: string): Observable<BookingDetails[]> {
    const url = `${environment.apiUrl}/booking/business/${businessId}`;
    console.log('[TenantService] Loading bookings for business:', businessId);
    return this.http.get<BookingDetails[]>(url);
  }

  /**
   * Load all available services (not business-specific)
   * Calls GET /api/service/all
   */
  loadAllServices(): Observable<ServiceDetails[]> {
    const url = `${environment.apiUrl}/service/all`;
    console.log('[TenantService] Loading all available services');
    return this.http.get<ServiceDetails[]>(url);
  }

  /**
   * Update business information
   * Calls PUT /api/business
   */
  updateBusiness(business: Business): Observable<Business> {
    const url = `${environment.apiUrl}/business`;
    console.log('[TenantService] Updating business:', business);
    return this.http.put<Business>(url, business).pipe(
      tap(updatedBusiness => {
        this.businessSubject.next(updatedBusiness);
      })
    );
  }

  /**
   * Add employee to business
   * Calls PUT /api/business/add-employee
   */
  addEmployeeToBusiness(businessId: string, userEmail: string, role: string): Observable<Business> {
    const url = `${environment.apiUrl}/business/add-employee`;
    const payload = {
      businessId,
      userEmail,
      role
    };
    console.log('[TenantService] Adding employee to business:', payload);
    return this.http.put<Business>(url, payload).pipe(
      tap(updatedBusiness => {
        this.businessSubject.next(updatedBusiness);
      })
    );
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
    console.log('[TenantService] Creating business service:', payload);
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
    console.log('[TenantService] Updating business service:', payload);
    return this.http.put<Service>(url, payload);
  }

  /**
   * Delete business service
   * Calls DELETE /api/businessservice/{id}
   */
  deleteBusinessService(id: string): Observable<Service> {
    const url = `${environment.apiUrl}/businessservice/${id}`;
    console.log('[TenantService] Deleting business service:', id);
    return this.http.delete<Service>(url);
  }

  /**
   * Cancel booking
   * Calls DELETE /api/booking/{id}
   */
  cancelBooking(id: string): Observable<any> {
    const url = `${environment.apiUrl}/booking/${id}`;
    console.log('[TenantService] Canceling booking:', id);
    return this.http.delete(url);
  }
}
