import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model';
import { BusinessType } from './models/business-type.enum';
import { FormsModule } from '@angular/forms';
import { BusinessService } from './services/business.service';
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from './models/dto/business-dto.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  services: GetServiceDto[] = [];
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;
  businessTypes = Object.values(BusinessType);

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

  servicesSectionOpen: boolean = false;
  businessesSectionOpen: boolean = false;

  constructor(
    private serviceService: ServiceService,
    private businessService: BusinessService
  ) {}

  ngOnInit(): void {
    this.loadServices();
    this.loadBusinesses();
  }

  // Existing Service methods
  loadServices(): void {
    this.serviceService.getServices().subscribe(data => {
      this.services = data;
    });
  }

  createService(): void {
    this.serviceService.createService(this.newService).subscribe(() => {
      this.loadServices();
      this.newService = { tag: '', businessType: BusinessType.None, value: '' };
    });
  }

  editService(service: GetServiceDto): void {
    this.editingService = { ...service };
  }

  updateService(): void {
    if (this.editingService) {
      this.serviceService.updateService(this.editingService).subscribe(() => {
        this.loadServices();
        this.editingService = null;
      });
    }
  }

  deleteService(id: string): void {
    this.serviceService.deleteService(id).subscribe(() => {
      this.loadServices();
    });
  }

  cancelEdit(): void {
    this.editingService = null;
  }

  // New Business methods
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

  toggleServicesSection(): void {
    this.servicesSectionOpen = !this.servicesSectionOpen;
  }

  toggleBusinessesSection(): void {
    this.businessesSectionOpen = !this.businessesSectionOpen;
  }
}
