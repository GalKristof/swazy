import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Business } from '../models/business';

/**
 * TenantService handles tenant-specific data loading
 * This service manages the global business/tenant context
 */
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
}
