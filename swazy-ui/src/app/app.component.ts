import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms'; // Added ReactiveFormsModule here

// Existing Service imports
import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model'; // This DTO is for generic services

// Existing Business imports (for managing Business entities)
import { BusinessService } from './services/business.service';
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from './models/dto/business-dto.model';

// Added: BusinessService (linking Service to Business) imports
import { BusinessServiceApiService } from './services/business-service-api.service';
import { GetBusinessServiceDto, CreateBusinessServiceDto, UpdateBusinessServiceDto } from './models/business-service/business-service.dtos';

// Booking related imports
import { BookingService } from './services/booking.service';
import { BookingDetailsDto } from './models/dto/booking-details-dto.model';

// Syncfusion Schedule imports
import { View, EventSettingsModel, DayService, WeekService, MonthService, AgendaService, ScheduleAllModule } from '@syncfusion/ej2-angular-schedule'; // Corrected name
import { DatePickerModule, TimePickerModule } from '@syncfusion/ej2-angular-calendars'; // Added Calendar imports

// DTO for creating bookings
import { CreateBookingDto } from './models/dto/booking-dto.model'; // Added CreateBookingDto

import { BusinessType } from './models/business-type.enum';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule, // Added
    ScheduleAllModule,
    DatePickerModule,  // Added
    TimePickerModule   // Added
  ],
  providers: [DayService, WeekService, MonthService, AgendaService],
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

  // --- New Properties for Booking Management ---
  bookingsSectionOpen: boolean = false;
  selectedBusinessIdForBookings: string = '';
  bookingsForSelectedBusiness: BookingDetailsDto[] = [];
  isLoadingBookings: boolean = false;
  schedulerView: View = 'Week';
  schedulerEventSettings: EventSettingsModel = { dataSource: [] };
  currentSchedulerDate: Date = new Date(); // Added property

  // --- New Properties for Create Booking Form ---
  showCreateBookingModal: boolean = false;
  createBookingForm!: FormGroup;
  availableServicesForBooking: GetBusinessServiceDto[] = [];
  availableEmployeesForBooking: any[] = []; // Keep as any[] for now
  minBookingDate: Date = new Date();
  selectedBookingDateForForm: Date | null = null;
  selectedBookingTimeForForm: Date | null = null; // This can be a Date object where only time part is relevant
  isSubmittingBooking: boolean = false;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private fb: FormBuilder, // Added FormBuilder
    private serviceService: ServiceService, // For generic services
    private businessService: BusinessService, // For managing Business entities
    private businessServiceApiService: BusinessServiceApiService, // For BusinessService entities (linking Service to Business)
    private bookingService: BookingService // Added BookingService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadServices(); // Load generic services
      this.loadBusinesses(); // Load businesses for the business management section
      this.loadAllBusinessesForDropdown(); // Load all businesses for the dropdown in BusinessServices section
    }

    this.minBookingDate.setHours(0, 0, 0, 0); // Set min date for date picker

    this.createBookingForm = this.fb.group({
      businessServiceId: ['', Validators.required],
      selectedDate: [null, Validators.required], // For DatePicker
      selectedTime: [null, Validators.required], // For TimePicker
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      notes: [''],
      employeeId: [null] // Optional
    });
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

  // --- New Methods for Booking Management and Scheduler ---
  toggleBookingsSection(): void {
    this.bookingsSectionOpen = !this.bookingsSectionOpen;
  }

  onBusinessSelectedForBookings(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const businessId = selectElement.value;
    this.selectedBusinessIdForBookings = businessId;
    this.bookingsForSelectedBusiness = [];
    this.schedulerEventSettings = { dataSource: [] }; // Clear previous events

    if (businessId) {
      this.loadBookingsForBusiness(businessId);
    }
  }

  loadBookingsForBusiness(businessId: string): void {
    if (!businessId) {
      this.bookingsForSelectedBusiness = [];
      this.schedulerEventSettings = { dataSource: [] };
      return;
    }
    this.isLoadingBookings = true;
    this.bookingService.getBookingsByBusinessId(businessId).subscribe({
      next: (bookings) => {
        this.bookingsForSelectedBusiness = bookings;
        this.schedulerEventSettings = {
          dataSource: bookings.map(booking => ({
            Id: booking.id, // Ensure Id matches your EventSettingsModel field for unique ID
            Subject: `${booking.serviceName} - ${booking.firstName} ${booking.lastName}`,
            StartTime: booking.startTime,
            EndTime: booking.endTime,
            // You can add more fields here to match EventSettingsModel properties
            // e.g., IsAllDay, RecurrenceRule, IsReadonly etc.
          }))
        };
        this.isLoadingBookings = false;
      },
      error: (err) => {
        console.error('Error loading bookings for business:', businessId, err);
        this.isLoadingBookings = false;
        this.bookingsForSelectedBusiness = [];
        this.schedulerEventSettings = { dataSource: [] }; // Clear on error
        // TODO: Optionally set a user-facing error message for this section
      }
    });
  }

  setSchedulerView(view: View): void {
    this.schedulerView = view;
  }

  // Helper method to get service name for display
  getServiceNameById(serviceId: string): string {
    const service = this.services.find(s => s.id === serviceId);
    return service ? service.value : 'Unknown Service';
  }

  // --- Methods for Create Booking Modal and Form ---
  openCreateBookingModal(): void {
    this.showCreateBookingModal = true;
    this.createBookingForm.reset();

    // Pre-fill with current business context if available
    // Ensure businessServicesForSelectedBusiness contains services for the *currently selected business for bookings*
    // This might need adjustment if selectedBusinessIdForBookings and selectedBusinessIdForServices are different
    if (this.selectedBusinessIdForBookings) {
        // Assuming businessServicesForSelectedBusiness is loaded when a business is selected for service management
        // If not, or if the context is different, this list might need to be re-fetched or filtered.
        // For now, let's use what's available if selectedBusinessIdForServices matches selectedBusinessIdForBookings.
        // A more robust approach might be to fetch services for selectedBusinessIdForBookings specifically.
        if (this.selectedBusinessIdForBookings === this.selectedBusinessIdForServices) {
            this.availableServicesForBooking = this.businessServicesForSelectedBusiness;
        } else {
            // If the contexts are different, or services for the booking business haven't been loaded,
            // you might need to fetch them here or ensure they are loaded when onBusinessSelectedForBookings is called.
            // For simplicity now, we'll clear it if not matching, implying a reload or different logic might be needed.
            this.availableServicesForBooking = [];
            // TODO: Consider fetching this.businessServiceApiService.getBusinessServicesByBusinessId(this.selectedBusinessIdForBookings)
            // and then populating this.availableServicesForBooking.
            // This example assumes services are already available or will be handled by another mechanism for now.
            // If businessServicesForSelectedBusiness is specific to the "Business Services Management" context,
            // then this.availableServicesForBooking should be populated based on this.selectedBusinessIdForBookings.
            // A simple way is to re-use loadBusinessServicesForBusiness if it stores its result in a way this can use,
            // or call a specific method to get services for the booking form's selected business.
            // For now, if selectedBusinessIdForBookings is set, we assume the user wants to book for that business,
            // and we need its services.
             if(this.selectedBusinessIdForServices === this.selectedBusinessIdForBookings) {
               this.availableServicesForBooking = this.businessServicesForSelectedBusiness;
             } else {
                // This implies a need to fetch services for this.selectedBusinessIdForBookings
                // For now, we'll just log a TODO or leave it empty and assume it's handled elsewhere or by user selection.
                console.warn("Service list for booking might not be for the correct business. Consider fetching.");
                this.availableServicesForBooking = []; // Or trigger a load
             }
        }
    } else {
        this.availableServicesForBooking = [];
    }

    // Default date to today, or selected scheduler date if available
    const initialDate = this.currentSchedulerDate ? new Date(this.currentSchedulerDate) : new Date();
    initialDate.setHours(0,0,0,0); // Ensure it's just the date part for comparison/initialization
    this.selectedBookingDateForForm = initialDate;

    // Default time (e.g., 9 AM), or could be more dynamic based on scheduler interaction later
    const initialTime = new Date();
    initialTime.setHours(9, 0, 0, 0);
    this.selectedBookingTimeForForm = initialTime;

    this.createBookingForm.patchValue({
      selectedDate: this.selectedBookingDateForForm,
      selectedTime: this.selectedBookingTimeForForm
      // businessServiceId will be empty, user needs to select
    });

    // Reset employee list or load if applicable
    this.availableEmployeesForBooking = [];
  }

  closeCreateBookingModal(): void {
    this.showCreateBookingModal = false;
    this.isSubmittingBooking = false; // Ensure submitting flag is reset
  }

  onBookingDateChange(args: any): void { // Type for args can be more specific if known e.g. ChangedEventArgs
    if (args && args.value) {
      this.selectedBookingDateForForm = new Date(args.value);
      this.selectedBookingDateForForm.setHours(0,0,0,0); // Normalize

      // Navigate scheduler to this date and change to Day view
      this.currentSchedulerDate = new Date(this.selectedBookingDateForForm);
      this.schedulerView = 'Day';
    } else {
      this.selectedBookingDateForForm = null;
    }
    this.createBookingForm.controls['selectedDate'].setValue(this.selectedBookingDateForForm);
  }

  onBookingTimeChange(args: any): void { // Type for args can be more specific
    if (args && args.value) {
      this.selectedBookingTimeForForm = new Date(args.value);
    } else {
      this.selectedBookingTimeForForm = null;
    }
    this.createBookingForm.controls['selectedTime'].setValue(this.selectedBookingTimeForForm);
  }

  onSubmitBooking(): void {
    this.isSubmittingBooking = true;
    this.createBookingForm.markAllAsTouched();

    if (this.createBookingForm.invalid) {
      this.isSubmittingBooking = false;
      // Optionally: Add a generic "form invalid" user notification
      return;
    }

    const formValues = this.createBookingForm.value;

    if (!formValues.selectedDate || !formValues.selectedTime) {
      console.error("Selected date or time is missing.");
      // Optionally: Add user notification
      this.isSubmittingBooking = false;
      return;
    }

    // Combine date and time
    const bookingDateTime = new Date(formValues.selectedDate);
    bookingDateTime.setHours(
      formValues.selectedTime.getHours(),
      formValues.selectedTime.getMinutes(),
      formValues.selectedTime.getSeconds()
    );

    const bookingDto: CreateBookingDto = {
      businessServiceId: formValues.businessServiceId,
      bookingDate: bookingDateTime.toISOString(), // Convert to ISO string for API
      firstName: formValues.firstName,
      lastName: formValues.lastName,
      email: formValues.email,
      phoneNumber: formValues.phoneNumber,
      notes: formValues.notes || '',
      employeeId: formValues.employeeId || null
    };

    this.bookingService.createBooking(bookingDto).subscribe({
      next: (newBooking) => {
        // TODO: Implement actual success notification based on existing patterns
        console.log('Booking created successfully!', newBooking);
        // Refresh scheduler only if the created booking is for the currently selected business for display
        if (this.selectedBusinessIdForBookings && newBooking) { // Assuming newBooking contains enough info or we reload all
             this.loadBookingsForBusiness(this.selectedBusinessIdForBookings);
        }
        this.closeCreateBookingModal();
        this.isSubmittingBooking = false;
      },
      error: (err) => {
        console.error('Error creating booking:', err);
        // TODO: Implement actual error notification
        this.isSubmittingBooking = false;
      }
    });
  }
}
