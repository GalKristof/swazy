import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { BusinessService } from '../../services/business.service';

@Component({
  selector: 'app-business-details',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './business-details.component.html',
  styleUrl: './business-details.component.scss'
})
export class BusinessDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private businessService = inject(BusinessService);

  businessId: string | null = null;
  business: any = null;
  isLoading = true;

  ngOnInit(): void {
    this.businessId = this.route.snapshot.paramMap.get('id');
    if (this.businessId) {
      this.loadBusinessDetails();
    }
  }

  loadBusinessDetails(): void {
    this.isLoading = true;
    this.businessService.getBusinessById(this.businessId!).subscribe({
      next: (business) => {
        console.log('[BusinessDetails] Loaded business:', business);
        console.log('[BusinessDetails] Services:', business.services);
        this.business = business;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading business details:', error);
        this.isLoading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/admin/businesses']);
  }

  openService(serviceId: string): void {
    this.router.navigate(['/admin/services'], {
      queryParams: { serviceId: serviceId }
    });
  }
}
