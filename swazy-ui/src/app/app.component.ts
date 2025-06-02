import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Existing Service imports
import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model'; // This DTO is for generic services

// Existing Business imports (for managing Business entities)
import { BusinessService } from './services/business.service';
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from './models/dto/business-dto.model';

// Added: BusinessService (linking Service to Business) imports
import { BusinessServiceApiService } from './services/business-service-api.service';
import { GetBusinessServiceDto, CreateBusinessServiceDto, UpdateBusinessServiceDto } from './models/business-service/business-service.dtos';

import { BusinessType } from './models/business-type.enum';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  providers: [], // Services are typically providedIn: 'root' or in modules
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  // Existing properties for generic Services
  services: GetServiceDto[] = []; // Generic services available in the system
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;
  
  // Existing properties for Businesses
  businesses: GetBusinessDto[] = []; // List of businesses (for the Business management section)
  newBusiness: CreateBusinessDto = {
    name: '',
    address: '',
    phoneNumber: '',
    email: '',
    businessType: BusinessType.None,
    websiteUrl: ''
  };
  editingBusiness: UpdateBusinessDto | null = null;

  // Common
  businessTypes = Object.values(BusinessType); // Used by both Business and generic Service forms

  // Section visibility toggles
  servicesSectionOpen: boolean = false; // For generic services
  businessesSectionOpen: boolean = false; // For managing businesses
  businessServicesSectionOpen: boolean = false; // For managing services offered by a specific business

  // --- New Properties for BusinessService Management ---
  allBusinessesForDropdown: GetBusinessDto[] = []; // To populate the business selection dropdown
  selectedBusinessIdForServices: string = ''; // ID of the business selected to view/manage its services
  businessServicesForSelectedBusiness: GetBusinessServiceDto[] = []; // Services offered by the selected business
  
  newBusinessService: CreateBusinessServiceDto = { 
    serviceId: '', // This will need to be selected from 'services' (GetServiceDto[])
    businessId: '', 
    price: 0, 
    duration: 0 
  };
  editingBusinessService: UpdateBusinessServiceDto | null = null;
  
  isLoadingBusinessServices: boolean = false;
  showCreateBusinessServiceForm: boolean = false;
  // --- End of New Properties ---

  constructor(
    private serviceService: ServiceService, // For generic services
    private businessService: BusinessService, // For managing Business entities
    private businessServiceApiService: BusinessServiceApiService // For BusinessService entities (linking Service to Business)
  ) {}

  ngOnInit(): void {
    this.loadServices(); // Load generic services
    this.loadBusinesses(); // Load businesses for the business management section
    this.loadAllBusinessesForDropdown(); // Load all businesses for the dropdown in BusinessServices section
  }

  // --- Generic Service Methods (No changes, existing) ---
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

  // --- Business Entity Management Methods (No changes, existing) ---
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

  // --- Section Toggle Methods ---
  toggleServicesSection(): void {
    this.servicesSectionOpen = !this.servicesSectionOpen;
  }

  toggleBusinessesSection(): void {
    this.businessesSectionOpen = !this.businessesSectionOpen;
  }

  toggleBusinessServicesSection(): void { // New
    this.businessServicesSectionOpen = !this.businessServicesSectionOpen;
  }

  // --- New Methods for BusinessService Management ---

  loadAllBusinessesForDropdown(): void {
    this.businessService.getBusinesses().subscribe(
      data => {
        this.allBusinessesForDropdown = data;
      },
      error => {
        console.error('Error loading businesses for dropdown:', error);
        // Potentially set an error message for the UI
      }
    );
  }

  onBusinessSelectedForServices(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const businessId = selectElement.value;
    this.selectedBusinessIdForServices = businessId;
    
    this.businessServicesForSelectedBusiness = []; // Clear previous services
    this.editingBusinessService = null; // Clear any ongoing edit
    this.showCreateBusinessServiceForm = false; // Hide create form

    if (businessId) {
      this.loadBusinessServicesForBusiness(businessId);
    }
  }

  loadBusinessServicesForBusiness(businessId: string): void {
    if (!businessId) {
      this.businessServicesForSelectedBusiness = [];
      return;
    }
    this.isLoadingBusinessServices = true;
    this.businessServiceApiService.getBusinessServicesByBusinessId(businessId).subscribe(
      data => {
        this.businessServicesForSelectedBusiness = data;
        this.isLoadingBusinessServices = false;
      },
      error => {
        console.error(`Error loading business services for business ID ${businessId}:`, error);
        this.businessServicesForSelectedBusiness = []; // Clear on error
        this.isLoadingBusinessServices = false;
        // Potentially set an error message for the UI
      }
    );
  }

  toggleCreateBusinessServiceForm(): void {
    this.showCreateBusinessServiceForm = !this.showCreateBusinessServiceForm;
    if (this.showCreateBusinessServiceForm) {
      this.editingBusinessService = null; // Cancel any edit if opening create form
      this.newBusinessService = { 
        serviceId: '', // User will need to select a generic service
        businessId: this.selectedBusinessIdForServices, // Pre-fill selected business
        price: 0, 
        duration: 0 
      };
    }
  }

  createBusinessService(): void {
    if (!this.newBusinessService.serviceId || !this.selectedBusinessIdForServices) {
      console.error('Service ID and Business ID are required to create a business service.');
      // Potentially show user error
      return;
    }
    // Ensure businessId is correctly set from the selection
    this.newBusinessService.businessId = this.selectedBusinessIdForServices;

    this.businessServiceApiService.createBusinessService(this.newBusinessService).subscribe(
      () => {
        this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
        this.showCreateBusinessServiceForm = false; // Hide form
        this.newBusinessService = { serviceId: '', businessId: this.selectedBusinessIdForServices, price: 0, duration: 0 }; // Reset form
      },
      error => {
        console.error('Error creating business service:', error);
        // Potentially set an error message for the UI
      }
    );
  }

  editBusinessService(bs: GetBusinessServiceDto): void {
    // The UpdateBusinessServiceDto only needs id, price, duration.
    // ServiceId and BusinessId are not updatable for an existing BusinessService.
    this.editingBusinessService = { 
      id: bs.id, 
      price: bs.price, 
      duration: bs.duration 
    };
    this.showCreateBusinessServiceForm = false; // Hide create form if editing
  }

  updateBusinessService(): void {
    if (this.editingBusinessService && this.editingBusinessService.id) {
      this.businessServiceApiService.updateBusinessService(this.editingBusinessService).subscribe(
        () => {
          this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
          this.editingBusinessService = null; // Clear editing model
        },
        error => {
          console.error('Error updating business service:', error);
          // Potentially set an error message for the UI
        }
      );
    }
  }

  deleteBusinessService(bsId: string): void {
    if (!this.selectedBusinessIdForServices) {
      console.error("Cannot delete: No business selected.");
      return;
    }
    this.businessServiceApiService.deleteBusinessService(bsId).subscribe(
      () => {
        this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
      },
      error => {
        console.error(`Error deleting business service ID ${bsId}:`, error);
        // Potentially set an error message for the UI
      }
    );
  }

  cancelEditBusinessService(): void {
    this.editingBusinessService = null;
  }

  cancelCreateBusinessService(): void {
    this.showCreateBusinessServiceForm = false;
    this.newBusinessService = { 
      serviceId: '', 
      businessId: this.selectedBusinessIdForServices, // Keep context of selected business
      price: 0, 
      duration: 0 
    };
  }
}
