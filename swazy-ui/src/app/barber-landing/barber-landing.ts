import { Component, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantService } from '../services/tenant.service';
import { BusinessService } from '../services/business.service';
import { Service } from '../models/service';
import { ServiceDetails } from '../models/service.details';

@Component({
  selector: 'app-barber-landing',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './barber-landing.html',
  styleUrls: ['./barber-landing.scss']
})
export class BarberLandingComponent {
  private tenantService = inject(TenantService);
  private businessService = inject(BusinessService);

  business$ = this.tenantService.business$;

  // Computed properties to get real data from business
  services = computed(() => {
    const business = this.tenantService.getCurrentBusiness();
    return business?.services || [];
  });

  employees = computed(() => {
    const business = this.tenantService.getCurrentBusiness();
    return business?.employees || [];
  });

  // Mock images for employees (can be replaced with real images later)
  getEmployeeImage(index: number): string {
    const images = [
      'https://images.unsplash.com/photo-1621605815971-fbc98d665033?w=400&h=400&fit=crop',
      'https://images.unsplash.com/photo-1622286342621-4bd786c2447c?w=400&h=400&fit=crop',
      'https://images.unsplash.com/photo-1605497788044-5a32c7078486?w=400&h=400&fit=crop',
      'https://images.unsplash.com/photo-1633332755192-727a05c4013d?w=400&h=400&fit=crop'
    ];
    return images[index % images.length];
  }

  // Helper to get service name from serviceDetails
  getServiceName(serviceId: string): string {
    // This will be improved later when we have service details
    return serviceId;
  }
}
