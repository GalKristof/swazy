import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';


import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model';

import { BusinessService } from './services/business.service';
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from './models/dto/business-dto.model';

import { BusinessServiceApiService } from './services/business-service-api.service';
import { GetBusinessServiceDto, CreateBusinessServiceDto, UpdateBusinessServiceDto } from './models/business-service/business-service.dtos';

import { BookingService } from './services/booking.service';
import { BookingDetailsDto } from './models/dto/booking-details-dto.model';

import { View, EventSettingsModel, DayService, WeekService, MonthService, AgendaService, ScheduleAllModule } from '@syncfusion/ej2-angular-schedule';
import {CalendarModule, DatePickerModule, TimePickerModule} from '@syncfusion/ej2-angular-calendars';

import { CreateBookingDto } from './models/dto/booking-dto.model';

import { BusinessType } from './models/business-type.enum';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ScheduleAllModule,
    DatePickerModule,
    TimePickerModule,
    CalendarModule
  ],
  providers: [DayService, WeekService, MonthService, AgendaService],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  services: GetServiceDto[] = [];
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;

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

  servicesSectionOpen: boolean = false;
  businessesSectionOpen: boolean = false;
  businessServicesSectionOpen: boolean = false;

  allBusinessesForDropdown: GetBusinessDto[] = [];
  selectedBusinessIdForServices: string = '';
  businessServicesForSelectedBusiness: GetBusinessServiceDto[] = [];

  newBusinessService: CreateBusinessServiceDto = {
    serviceId: '',
    businessId: '',
    price: 0,
    duration: 0
  };
  editingBusinessService: UpdateBusinessServiceDto | null = null;

  isLoadingBusinessServices: boolean = false;
  showCreateBusinessServiceForm: boolean = false;

  bookingsSectionOpen: boolean = false;
  selectedBusinessIdForBookings: string = '';
  bookingsForSelectedBusiness: BookingDetailsDto[] = [];
  isLoadingBookings: boolean = false;
  schedulerView: View = 'Week';
  schedulerEventSettings: EventSettingsModel = { dataSource: [] };
  currentSchedulerDate: Date = new Date();

  createBookingForm!: FormGroup;
  availableServicesForBooking: GetBusinessServiceDto[] = [];
  availableEmployeesForBooking: any[] = [];
  minBookingDate: Date = new Date();
  selectedBookingDateForForm: Date | null = null;
  selectedBookingTimeForForm: Date | null = null;
  isSubmittingBooking: boolean = false;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private fb: FormBuilder,
    private serviceService: ServiceService,
    private businessService: BusinessService,
    private businessServiceApiService: BusinessServiceApiService,
    private bookingService: BookingService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadServices();
      this.loadBusinesses();
      this.loadAllBusinessesForDropdown();
    }

    this.minBookingDate.setHours(0, 0, 0, 0);

    this.createBookingForm = this.fb.group({
      businessServiceId: ['', Validators.required],
      selectedDate: [null, Validators.required],
      selectedTime: [null, Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      notes: [''],
      employeeId: [null]
    });
  }

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

  onBusinessSelectedForServices(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const businessId = selectElement.value;
    this.selectedBusinessIdForServices = businessId;

    this.businessServicesForSelectedBusiness = [];
    this.editingBusinessService = null;
    this.showCreateBusinessServiceForm = false;

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
      this.editingBusinessService = null;
      this.newBusinessService = {
        serviceId: '',
        businessId: this.selectedBusinessIdForServices,
        price: 0,
        duration: 0
      };
    }
  }

  createBusinessService(): void {
    if (!this.newBusinessService.serviceId || !this.selectedBusinessIdForServices) {
      console.error('Service ID and Business ID are required to create a business service.');
      return;
    }
    this.newBusinessService.businessId = this.selectedBusinessIdForServices;

    this.businessServiceApiService.createBusinessService(this.newBusinessService).subscribe(
      () => {
        this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
        this.showCreateBusinessServiceForm = false; // Hide form
        this.newBusinessService = { serviceId: '', businessId: this.selectedBusinessIdForServices, price: 0, duration: 0 }; // Reset form
      },
      error => {
        console.error('Error creating business service:', error);
      }
    );
  }

  editBusinessService(bs: GetBusinessServiceDto): void {
    this.editingBusinessService = {
      id: bs.id,
      price: bs.price,
      duration: bs.duration
    };
    this.showCreateBusinessServiceForm = false;
  }

  updateBusinessService(): void {
    if (this.editingBusinessService && this.editingBusinessService.id) {
      this.businessServiceApiService.updateBusinessService(this.editingBusinessService).subscribe(
        () => {
          this.loadBusinessServicesForBusiness(this.selectedBusinessIdForServices);
          this.editingBusinessService = null;
        },
        error => {
          console.error('Error updating business service:', error);
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
      businessId: this.selectedBusinessIdForServices,
      price: 0,
      duration: 0
    };
  }

  toggleBookingsSection(): void {
    this.bookingsSectionOpen = !this.bookingsSectionOpen;
  }

  onBusinessSelectedForBookings(event: Event): void {
    const selectElement = event.target as HTMLSelectElement;
    const businessId = selectElement.value;
    this.selectedBusinessIdForBookings = businessId;
    this.bookingsForSelectedBusiness = [];
    this.schedulerEventSettings = { dataSource: [] };
    // Reset services for booking form as well when business changes
    this.availableServicesForBooking = [];

    if (businessId) {
      this.loadBookingsForBusiness(businessId);
      this.prepareBookingForm();
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
            Id: booking.id,
            Subject: `${booking.serviceName} - ${booking.firstName} ${booking.lastName}`,
            StartTime: booking.startTime,
            EndTime: booking.endTime,
          }))
        };
        this.isLoadingBookings = false;
      },
      error: (err) => {
        console.error('Error loading bookings for business:', businessId, err);
        this.isLoadingBookings = false;
        this.bookingsForSelectedBusiness = [];
        this.schedulerEventSettings = { dataSource: [] };
      }
    });
  }

  setSchedulerView(view: View): void {
    this.schedulerView = view;
  }

  getServiceNameById(serviceId: string): string {
    const service = this.services.find(s => s.id === serviceId);
    return service ? service.value : 'Unknown Service';
  }

  prepareBookingForm(): void {
    this.createBookingForm.reset();

    this.availableServicesForBooking = []; // Initialize as empty

    if (this.selectedBusinessIdForBookings) {
      this.businessServiceApiService.getBusinessServicesByBusinessId(this.selectedBusinessIdForBookings).subscribe({
        next: (services) => {
          this.availableServicesForBooking = services;
        },
        error: (err) => {
          console.error('Error loading services for booking:', err);
          this.availableServicesForBooking = []; // Ensure it's empty on error
        }
      });
    }

    const initialDate = this.currentSchedulerDate ? new Date(this.currentSchedulerDate) : new Date();
    initialDate.setHours(0,0,0,0);
    this.selectedBookingDateForForm = initialDate;

    const initialTime = new Date();
    initialTime.setHours(9, 0, 0, 0);
    this.selectedBookingTimeForForm = initialTime;

    this.createBookingForm.patchValue({
      selectedDate: this.selectedBookingDateForForm,
      selectedTime: this.selectedBookingTimeForForm
    });

    this.availableEmployeesForBooking = [];
  }

  clearBookingSubmissionState(): void {
    this.isSubmittingBooking = false;
  }

  onBookingDateChange(args: any): void {
    if (args && args.value) {
      this.selectedBookingDateForForm = new Date(args.value);
      this.selectedBookingDateForForm.setHours(0,0,0,0);

      this.currentSchedulerDate = new Date(this.selectedBookingDateForForm);
      this.schedulerView = 'Day';
    } else {
      this.selectedBookingDateForForm = null;
    }
    this.createBookingForm.controls['selectedDate'].setValue(this.selectedBookingDateForForm);
  }

  onBookingTimeChange(args: any): void {
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
      return;
    }

    const formValues = this.createBookingForm.value;

    if (!formValues.selectedDate || !formValues.selectedTime) {
      console.error("Selected date or time is missing.");
      this.isSubmittingBooking = false;
      return;
    }

    const bookingDateTime = new Date(formValues.selectedDate);
    bookingDateTime.setHours(
      formValues.selectedTime.getHours(),
      formValues.selectedTime.getMinutes(),
      formValues.selectedTime.getSeconds()
    );

    const bookingDto: CreateBookingDto = {
      businessServiceId: formValues.businessServiceId,
      bookingDate: bookingDateTime.toISOString(),
      firstName: formValues.firstName,
      lastName: formValues.lastName,
      email: formValues.email,
      phoneNumber: formValues.phoneNumber,
      notes: formValues.notes || '',
      employeeId: formValues.employeeId || null
    };

    this.bookingService.createBooking(bookingDto).subscribe({
      next: (newBooking) => {
        console.log('Booking created successfully!', newBooking);
        if (this.selectedBusinessIdForBookings && newBooking) {
             this.loadBookingsForBusiness(this.selectedBusinessIdForBookings);
        }
        this.clearBookingSubmissionState(); // Renamed method
        this.prepareBookingForm(); // Reset form and reload services
      },
      error: (err) => {
        console.error('Error creating booking:', err);
        this.clearBookingSubmissionState(); // Also clear submission state on error
      }
    });
  }
}
