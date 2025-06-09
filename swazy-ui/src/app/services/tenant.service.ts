import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { BusinessService } from './business.service'; // Assuming BusinessService is in the same directory
import { GetBusinessDto } from '../models/dto/business-dto.model'; // Adjust path as needed
import { environment } from '../../environments/environment'; // Adjust path as needed

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private tenantBusiness$: BehaviorSubject<GetBusinessDto | null> = new BehaviorSubject<GetBusinessDto | null>(null);
  public tenantBusinessObs$: Observable<GetBusinessDto | null> = this.tenantBusiness$.asObservable();

  constructor(private businessService: BusinessService) {
    this.loadTenantBusiness();
  }

  loadTenantBusiness(): void {
    if (!environment.tenantId) {
      console.warn('Tenant ID is not set in environment. TenantService will not load business details.');
      this.tenantBusiness$.next(null);
      return;
    }
    this.businessService.getBusinessById(environment.tenantId).subscribe(
      (businessDetails: GetBusinessDto) => {
        this.tenantBusiness$.next(businessDetails);
      },
      (error: any) => { // Added type for error
        console.error('Failed to load tenant business details:', error);
        // Optionally, handle the error more gracefully, e.g., by setting a default value or showing a user notification
        this.tenantBusiness$.next(null); // Or some error state object
      }
    );
  }
}
