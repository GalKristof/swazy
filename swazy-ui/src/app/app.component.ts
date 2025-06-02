import { Component, OnInit } from '@angular/core'; // Added OnInit
import { CommonModule } from '@angular/common';
import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model';
import { BusinessType } from './models/business-type.enum';
import { FormsModule } from '@angular/forms';
import { BusinessService } from './services/business.service'; // Added
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from './models/dto/business-dto.model'; // Added

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [], // BusinessService will be added in app.config.ts later
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit { // Implemented OnInit
  // Existing Service properties
  services: GetServiceDto[] = [];
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;
  businessTypes = Object.values(BusinessType); // Keep this as it's used by both services and businesses

  // New Business properties
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

  constructor(
    private serviceService: ServiceService,
    private businessService: BusinessService // Injected BusinessService
  ) {}

  ngOnInit(): void {
    this.loadServices();
    this.loadBusinesses(); // Added
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
    // For UpdateBusinessDto, we only need the id and the fields that can be updated.
    // We spread the original business object here. If some fields are not part of UpdateBusinessDto,
    // they will be ignored by the backend if the DTO is defined strictly.
    // Or, ensure UpdateBusinessDto includes all editable fields from GetBusinessDto.
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
