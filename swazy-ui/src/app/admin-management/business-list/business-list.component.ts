import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { BusinessService } from '../../services/business.service';

@Component({
  selector: 'app-business-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './business-list.component.html',
  styleUrl: './business-list.component.scss'
})
export class BusinessListComponent implements OnInit {
  private businessService = inject(BusinessService);
  private router = inject(Router);

  businesses: any[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadBusinesses();
  }

  loadBusinesses(): void {
    this.isLoading = true;
    this.businessService.getAllBusinesses().subscribe({
      next: (businesses) => {
        this.businesses = businesses;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading businesses:', error);
        this.isLoading = false;
      }
    });
  }

  viewDetails(businessId: string): void {
    this.router.navigate(['/admin/businesses', businessId]);
  }
}
