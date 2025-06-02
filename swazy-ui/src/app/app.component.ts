import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Existing Service-related imports
import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model';
import { BusinessType } from './models/business-type.enum';

// New BusinessService-related imports
import { BusinessServiceService } from './services/business-service.service';
import { Business as GetBusinessDto } from './models/business/business.model';
import {
  GetBusinessServiceDto,
  CreateBusinessServiceDto,
  UpdateBusinessServiceDto
} from './models/business/business.service.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule], // FormsModule is important for ngModel
  // BusinessServiceService is providedIn: 'root'
  providers: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  // Existing properties for ServiceService
  services: GetServiceDto[] = [];
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;
  businessTypes = Object.values(BusinessType);

  // New properties for BusinessServiceService
  businesses: GetBusinessDto[] = [];
  selectedBusinessId: string | null = null;
  businessServices: GetBusinessServiceDto[] = [];
  
  // Initialize newBusinessService based on CreateBusinessServiceDto structure
  // Assuming name, description, price, duration are primary fields. businessId is set on creation.
  newBusinessService: CreateBusinessServiceDto = {
    name: '',
    description: '',
    price: '0', // Default to string '0' or appropriate initial value
    duration: 0, // Default to 0 or appropriate initial value
    // serviceId: '', // Optional: if linking to a predefined generic service
    businessId: '' // Will be set when a business is selected
  };
  editingBusinessService: UpdateBusinessServiceDto | null = null;

  constructor(
    private serviceService: ServiceService, // Existing service
    private businessServiceService: BusinessServiceService // New service
  ) {}

  ngOnInit(): void {
    this.loadServices(); // Existing
    this.loadBusinesses(); // New
  }

  // --- Existing methods for ServiceService ---
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

  // --- New methods for BusinessServiceService ---
  loadBusinesses(): void {
    this.businessServiceService.getBusinesses().subscribe({
      next: (data) => {
        this.businesses = data;
      },
      error: (err) => console.error('Error loading businesses:', err)
    });
  }

  onBusinessSelected(event: any): void {
    // Assuming the event is directly the value, or from event.target.value
    const businessId = event?.target?.value || event; 
    this.selectedBusinessId = businessId;
    if (this.selectedBusinessId) {
      this.loadBusinessServices();
    } else {
      this.businessServices = [];
    }
  }

  loadBusinessServices(): void {
    if (!this.selectedBusinessId) {
      this.businessServices = []; // Clear services if no business is selected
      return;
    }
    this.businessServiceService.getBusinessServices(this.selectedBusinessId).subscribe({
      next: (data) => {
        // The service returns GetBusinessServiceDto which should match the component's expectation
        // If mapping was needed (e.g. from RawBusinessService), it would happen here or in the service
        this.businessServices = data.map(bs => ({
          ...bs,
          // Ensure name and description are top-level if they come from a nested 'service' object
          // This depends on the actual structure of GetBusinessServiceDto from the service vs model
          // Based on current DTO, name/description are already top-level.
          // Price and duration are also part of GetBusinessServiceDto
        }));
      },
      error: (err) => {
        console.error('Error loading business services:', err);
        this.businessServices = []; // Clear on error
      }
    });
  }

  createBusinessService(): void {
    if (!this.selectedBusinessId) {
      console.error('No business selected to create a service for.');
      return;
    }
    this.newBusinessService.businessId = this.selectedBusinessId;
    
    // Ensure all required fields for CreateBusinessServiceDto are present
    const serviceToCreate: CreateBusinessServiceDto = {
        name: this.newBusinessService.name,
        description: this.newBusinessService.description,
        price: this.newBusinessService.price,
        duration: this.newBusinessService.duration,
        businessId: this.selectedBusinessId,
        serviceId: this.newBusinessService.serviceId // if applicable
    };

    this.businessServiceService.createBusinessService(serviceToCreate).subscribe({
      next: () => {
        this.loadBusinessServices(); // Reload services for the current business
        this.newBusinessService = { // Reset form
          name: '',
          description: '',
          price: '0',
          duration: 0,
          businessId: this.selectedBusinessId, // Keep businessId for convenience
          serviceId: ''
        };
      },
      error: (err) => console.error('Error creating business service:', err)
    });
  }

  editBusinessService(service: GetBusinessServiceDto): void {
    // Create a copy for editing, matching UpdateBusinessServiceDto structure
    this.editingBusinessService = { 
      id: service.id,
      name: service.name,
      description: service.description,
      price: service.price,
      duration: service.duration,
      serviceId: service.serviceId
     };
  }

  updateBusinessService(): void {
    if (!this.editingBusinessService || !this.editingBusinessService.id) {
      console.error('No service selected for update or ID is missing.');
      return;
    }
    this.businessServiceService.updateBusinessService(this.editingBusinessService).subscribe({
      next: () => {
        this.loadBusinessServices();
        this.editingBusinessService = null;
      },
      error: (err) => console.error('Error updating business service:', err)
    });
  }

  deleteBusinessService(id: string): void {
    if (!id) {
      console.error('Service ID is required for deletion.');
      return;
    }
    this.businessServiceService.deleteBusinessService(id).subscribe({
      next: () => {
        this.loadBusinessServices();
      },
      error: (err) => console.error('Error deleting business service:', err)
    });
  }

  cancelEditBusinessService(): void {
    this.editingBusinessService = null;
  }
}
