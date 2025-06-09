import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { BusinessService } from '../services/business.service'; // Adjusted path
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from '../models/dto/business-dto.model'; // Adjusted path
import { BusinessType } from '../models/business-type.enum'; // Adjusted path

@Component({
  selector: 'app-businesses',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './businesses.component.html', // Corrected template path
  styleUrl: './businesses.component.scss' // Corrected style path
})
export class BusinessesComponent implements OnInit {
  businesses: GetBusinessDto[] = [];
  newBusiness: CreateBusinessDto = {
    name: '',
    address: '',
    phoneNumber: '',
    email: '',
    businessType: BusinessType.None,
    websiteUrl: ''
  };
  editingBusiness: UpdateBusinessDto | null = null;
  businessTypes = Object.values(BusinessType);

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private businessService: BusinessService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadBusinesses();
    }
  }

  loadBusinesses(): void {
    this.businessService.getBusinesses().subscribe(data => {
      this.businesses = data;
    });
  }

  createBusiness(): void {
    this.businessService.createBusiness(this.newBusiness).subscribe(() => {
      this.loadBusinesses();
      this.newBusiness = {
        name: '',
        address: '',
        phoneNumber: '',
        email: '',
        businessType: BusinessType.None,
        websiteUrl: ''
      };
    });
  }

  editBusiness(business: GetBusinessDto): void {
    this.editingBusiness = { ...business };
  }

  updateBusiness(): void {
    if (this.editingBusiness && this.editingBusiness.id) {
      this.businessService.updateBusiness(this.editingBusiness).subscribe(() => {
        this.loadBusinesses();
        this.editingBusiness = null;
      });
    }
  }

  deleteBusiness(id: string): void {
    this.businessService.deleteBusiness(id).subscribe(() => {
      this.loadBusinesses();
    });
  }

  cancelEditBusiness(): void {
    this.editingBusiness = null;
  }
}
