import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser, isPlatformServer } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { Business } from '../models/business';
import { TransferState, makeStateKey } from '@angular/core';

const BUSINESS_KEY = makeStateKey<Business>('BUSINESS_DATA');

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  private transferState = inject(TransferState);

  private businessSubject = new BehaviorSubject<Business | null>(null);
  public business$ = this.businessSubject.asObservable();

  loadBusinessData(): Observable<Business> {
    if (isPlatformBrowser(this.platformId)) {
      const transferredBusiness = this.transferState.get(BUSINESS_KEY, null);
      if (transferredBusiness) {
        this.transferState.remove(BUSINESS_KEY);
        this.businessSubject.next(transferredBusiness);
        return of(transferredBusiness);
      }
    }

    let url: string;

    if (environment.fromDomain && isPlatformBrowser(this.platformId)) {
      const domain = window.location.hostname;
      url = `${environment.apiUrl}/business/by-domain/${domain}`;
    } else {
      url = `${environment.apiUrl}/business/${environment.tenantId}`;
    }

    return this.http.get<Business>(url).pipe(
      tap(business => {
        if (isPlatformServer(this.platformId)) {
          this.transferState.set(BUSINESS_KEY, business);
        }
        this.businessSubject.next(business);
      })
    );
  }

  getCurrentBusiness(): Business | null {
    return this.businessSubject.value;
  }
}
