import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { BusinessService } from '../services/business.service'; // Adjusted path
import { BusinessServiceApiService } from '../services/business-service-api.service'; // Adjusted path
import { ServiceService } from '../services/service.service'; // For loading generic services

import { GetBusinessDto } from '../models/dto/business-dto.model'; // Adjusted path
import { GetBusinessServiceDto, CreateBusinessServiceDto, UpdateBusinessServiceDto } from '../models/business-service/business-service.dtos'; // Adjusted path
import { GetServiceDto } from '../models/service.model'; // For generic services list

@Component({
  selector: 'app-business-services',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './business-services.component.html',
  styleUrl: './business-services.component.scss'
})
export class BusinessServicesComponent implements OnInit {
  allBusinessesForDropdown: GetBusinessDto[] = [];
  selectedBusinessIdForServices: string = '';
  businessServicesForSelectedBusiness: GetBusinessServiceDto[] = [];

  services: GetServiceDto[] = []; // To hold generic services for dropdown and getServiceNameById

  newBusinessService: CreateBusinessServiceDto = {
    serviceId: '',
    businessId: '', // Will be set when a business is selected
    price: 0,
    duration: 0
  };
  editingBusinessService: UpdateBusinessServiceDto | null = null;

  isLoadingBusinessServices: boolean = false;
  showCreateBusinessServiceForm: boolean = false;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private businessService: BusinessService,
    private businessServiceApiService: BusinessServiceApiService,
    private serviceService: ServiceService // Injected to load generic services
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadAllBusinessesForDropdown();
      this.loadGenericServices(); // Load all generic services for dropdown/naming
    }
  }

  loadAllBusinessesForDropdown(): void {
    this.businessService.getBusinesses().subscribe(
      data => {
        this.allBusinessesForDropdown = data;
      },
      error => {
        console.error('Error loading businesses for dropdown:', error);
      }
    );
  }

  loadGenericServices(): void {
    this.serviceService.getServices().subscribe(data => {
      this.services = data;
    });
  }

  onBusinessSelectedForServices(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const businessId = selectElement.value;
    this.selectedBusinessIdForServices = businessId;

    this.businessServicesForSelectedBusiness = [];
    this.editingBusinessService = null;
    this.showCreateBusinessServiceForm = false;
    // Reset newBusinessService when business changes
    this.newBusinessService = {
      serviceId: '',
      businessId: businessId, // Set current business ID
      price: 0,
      duration: 0
    };


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
        this.businessServicesForSelectedBusiness = [];
        this.isLoadingBusinessServices = false;
      }
    );
  }

  toggleCreateBusinessServiceForm(): void {
    this.showCreateBusinessServiceForm = !this.showCreateBusinessServiceForm;
    if (this.showCreateBusinessServiceForm) {
      this.editingBusinessService = null; // Cancel edit mode if switching to create
      // Ensure newBusinessService has the currently selected businessId
      this.newBusinessService = {
        serviceId: '', // Reset serviceId
        businessId: this.selectedBusinessIdForServices,
        price: 0,
        duration: 0
      };
    }
  }

  createBusinessService(): void {
    if (!this.newBusinessService.serviceId || !this.selectedBusinessIdForServices) {
      console.error('Service ID and Business ID are required to create a business service.');
      // Potentially show a user-facing error
      return;
    }
    this.newBusinessService.businessId = this.selectedBusinessIdForServices; // Ensure businessId is correctly set

    this.businessServiceApiService.createBusinessService(this.newBusinessService).subscribe(
      () => {
        this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
        this.showCreateBusinessServiceForm = false;
        this.newBusinessService = { serviceId: '', businessId: this.selectedBusinessIdForServices, price: 0, duration: 0 };
      },
      error => {
        console.error('Error creating business service:', error);
        // Potentially show a user-facing error
      }
    );
  }

  editBusinessService(bs: GetBusinessServiceDto): void {
    // For UpdateBusinessServiceDto, the 'id' is the BusinessService ID itself.
    // The 'serviceId' (generic service ID) is not part of UpdateBusinessServiceDto as it's not updatable.
    this.editingBusinessService = {
      id: bs.id, // This is the BusinessService ID
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
          this.editingBusinessService = null; // Exit edit mode
        },
        error => {
          console.error('Error updating business service:', error);
          // Potentially show a user-facing error
        }
      );
    }
  }

  deleteBusinessService(bsId: string): void {
    if (!this.selectedBusinessIdForServices) {
      console.error("Cannot delete: No business selected or business service ID is missing.");
      return;
    }
    this.businessServiceApiService.deleteBusinessService(bsId).subscribe(
      () => {
        this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
      },
      error => {
        console.error(`Error deleting business service ID ${bsId}:`, error);
      }
    );
  }

  cancelEditBusinessService(): void {
    this.editingBusinessService = null;
  }

  cancelCreateBusinessService(): void {
    this.showCreateBusinessServiceForm = false;
    this.newBusinessService = { // Reset form
      serviceId: '',
      businessId: this.selectedBusinessIdForServices,
      price: 0,
      duration: 0
    };
  }

  getServiceNameById(serviceId: string): string {
    const service = this.services.find(s => s.id === serviceId);
    return service ? service.value : 'Unknown Service';
  }
}
