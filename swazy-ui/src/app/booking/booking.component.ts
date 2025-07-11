import { Component, OnInit, Inject, PLATFORM_ID, Output, EventEmitter } from '@angular/core'; // Removed Input, OnChanges, SimpleChanges
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { CalendarModule, DatePickerModule, TimePickerModule } from '@syncfusion/ej2-angular-calendars';

import { BookingService } from '../services/booking.service';
import { BusinessServiceApiService } from '../services/business-service-api.service';
import { ServiceService } from '../services/service.service'; // For getServiceNameById
import { TenantService } from '../services/tenant.service'; // Import TenantService

import { CreateBookingDto } from '../models/dto/booking-dto.model';
import { GetBusinessServiceDto } from '../models/business-service/business-service.dtos';
import { GetServiceDto } from '../models/service.model'; // For holding generic services

@Component({
  selector: 'app-booking',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    CalendarModule,
    DatePickerModule,
    TimePickerModule
  ],
  templateUrl: './booking.component.html',
  styleUrl: './booking.component.scss'
})
export class BookingComponent implements OnInit { // Removed OnChanges
  // @Input() selectedBusinessIdForBookings: string = ''; // Removed
  @Output() bookingCreated = new EventEmitter<void>();
  @Output() bookingDateChanged = new EventEmitter<Date>();


  createBookingForm!: FormGroup;
  availableServicesForBooking: GetBusinessServiceDto[] = [];
  minBookingDate: Date = new Date();
  selectedBookingDateForForm: Date | null = null;
  selectedBookingTimeForForm: Date | null = null;
  isSubmittingBooking: boolean = false;
  private currentTenantId: string | null = null; // Added to store current tenant ID

  services: GetServiceDto[] = []; // For getServiceNameById

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private fb: FormBuilder,
    private bookingService: BookingService,
    private businessServiceApiService: BusinessServiceApiService,
    private serviceService: ServiceService, // To load generic services
    private tenantService: TenantService // Injected TenantService
  ) {}

  ngOnInit(): void {
    this.minBookingDate.setHours(0, 0, 0, 0);
    this.initializeForm();

    if (isPlatformBrowser(this.platformId)) {
      this.loadGenericServices(); // Load generic services for names
      this.tenantService.tenantBusinessObs$.subscribe(tenantBusiness => {
        if (tenantBusiness && tenantBusiness.id) {
          this.currentTenantId = tenantBusiness.id; // Store tenant ID
          this.prepareBookingForm(tenantBusiness.id);
        } else {
          this.currentTenantId = null; // Clear tenant ID
          // No tenant business ID, clear services or handle as needed
          this.availableServicesForBooking = [];
          this.createBookingForm.reset(); // Optionally reset form if no business context
        }
      });
    }
  }

  // ngOnChanges(changes: SimpleChanges): void { // Removed
  //   if (changes['selectedBusinessIdForBookings'] && !changes['selectedBusinessIdForBookings'].firstChange) {
  //     // If business ID changes, reload services for that business
  //     if (isPlatformBrowser(this.platformId)) {
  //       this.prepareBookingForm();
  //     }
  //   }
  // }

  initializeForm(): void {
    this.createBookingForm = this.fb.group({
      businessServiceId: ['', Validators.required],
      selectedDate: [null, Validators.required],
      selectedTime: [null, Validators.required],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', Validators.required],
      notes: [''],
      employeeId: [null] // Assuming employee selection might be added later
    });
  }

  loadGenericServices(): void {
    this.serviceService.getServices().subscribe(data => {
      this.services = data;
      // After generic services are loaded, if we have business services, refresh names or form
      // This is important if business services were loaded before generic ones
      if (this.availableServicesForBooking.length > 0) {
        // Potentially update display if needed, though ngFor in template handles it
      }
    });
  }

  prepareBookingForm(businessId: string | null): void { // Added businessId parameter
    this.createBookingForm.reset();
    this.availableServicesForBooking = [];

    if (businessId) {
      this.businessServiceApiService.getBusinessServicesByBusinessId(businessId).subscribe({
        next: (services) => {
          this.availableServicesForBooking = services;
        },
        error: (err) => {
          console.error('Error loading services for booking:', err);
          this.availableServicesForBooking = [];
        }
      });
    }

    // Set initial date and time for the form (e.g., today, 9 AM)
    const initialDate = new Date(); // Or based on current scheduler date if available
    initialDate.setHours(0,0,0,0);
    this.selectedBookingDateForForm = initialDate;

    const initialTime = new Date();
    initialTime.setHours(9, 0, 0, 0); // Default to 9 AM
    this.selectedBookingTimeForForm = initialTime;

    this.createBookingForm.patchValue({
      selectedDate: this.selectedBookingDateForForm,
      selectedTime: this.selectedBookingTimeForForm
    });
  }

  getServiceNameById(serviceId: string): string {
    const service = this.services.find(s => s.id === serviceId);
    return service ? service.value : 'Loading...'; // Or 'Unknown Service'
  }

  onBookingDateChange(args: any): void {
    if (args && args.value) {
      this.selectedBookingDateForForm = new Date(args.value);
      this.selectedBookingDateForForm.setHours(0,0,0,0);
      this.bookingDateChanged.emit(this.selectedBookingDateForForm); // Emit date change
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

  clearBookingSubmissionState(): void {
    this.isSubmittingBooking = false;
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
      employeeId: formValues.employeeId || null // Ensure employeeId is handled
    };

    this.bookingService.createBooking(bookingDto).subscribe({
      next: (newBooking) => {
        console.log('Booking created successfully!', newBooking);
        this.bookingCreated.emit(); // Emit event
        this.clearBookingSubmissionState();
        // Reset form, ensuring it uses the current tenant's business ID if available
        this.prepareBookingForm(this.currentTenantId); // Use stored tenant ID
      },
      error: (err) => {
        console.error('Error creating booking:', err);
        this.clearBookingSubmissionState();
        // Potentially show a user-facing error message
      }
    });
  }
}
