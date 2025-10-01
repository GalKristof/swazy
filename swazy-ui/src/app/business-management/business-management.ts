import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TenantService } from '../services/tenant.service';
import { BusinessService } from '../services/business.service';
import { Business } from '../models/business';
import { Employee } from '../models/employee';
import { Service } from '../models/service';
import { BookingDetails } from '../models/booking.details';
import { ServiceDetails } from '../models/service.details';

@Component({
  selector: 'app-business-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './business-management.html',
  styleUrls: ['./business-management.scss']
})
export class BusinessManagementComponent implements OnInit {
  private tenantService = inject(TenantService);
  private businessService = inject(BusinessService);

  activeTab = signal<'info' | 'employees' | 'services' | 'bookings'>('info');

  business = signal<Business | null>(null);
  isEditingBusiness = signal(false);
  editBusinessForm = signal<Business | null>(null);

  employees = signal<Employee[]>([]);
  isAddingEmployee = signal(false);
  newEmployee = signal<Partial<Employee>>({
    firstName: '',
    lastName: '',
    email: '',
    role: 'Employee'
  });

  services = signal<Service[]>([]);
  availableServices = signal<ServiceDetails[]>([]);
  isAddingService = signal(false);
  editingServiceId = signal<string | null>(null);
  newService = signal<Partial<Service>>({
    serviceId: '',
    serviceName: '',
    price: 0,
    duration: 0
  });

  bookings = signal<BookingDetails[]>([]);

  businessTypes = [
    'BarberSalon',
    'HairSalon',
    'MassageSalon',
    'BeautySalon',
    'NailSalon',
    'Trainer',
    'ServiceProvider',
    'Restaurant',
    'Other'
  ];

  setActiveTab(tab: 'info' | 'employees' | 'services' | 'bookings') {
    this.activeTab.set(tab);

    // Lazy load bookings only when the bookings tab is opened
    if (tab === 'bookings' && this.bookings().length === 0) {
      this.loadBusinessBookings();
    }
  }

  startEditingBusiness() {
    if (this.business()) {
      this.editBusinessForm.set({ ...this.business() } as Business);
      this.isEditingBusiness.set(true);
    }
  }

  cancelEditingBusiness() {
    this.isEditingBusiness.set(false);
  }

  saveBusinessInfo() {
    const formData = this.editBusinessForm();
    if (formData && this.business()) {
      this.businessService.updateBusiness(formData).subscribe({
        next: (updatedBusiness) => {
          this.business.set(updatedBusiness);
          this.employees.set(updatedBusiness.employees || []);
          this.services.set(updatedBusiness.services || []);
          this.isEditingBusiness.set(false);
        },
        error: (error) => {
          console.error('Error updating business:', error);
          alert('Hiba történt az üzlet adatainak mentése során.');
        }
      });
    }
  }

  showAddEmployeeForm() {
    this.newEmployee.set({
      firstName: '',
      lastName: '',
      email: '',
      role: 'Employee'
    });
    this.isAddingEmployee.set(true);
  }

  cancelAddEmployee() {
    this.isAddingEmployee.set(false);
  }

  addEmployee() {
    const employee = this.newEmployee();
    const businessId = this.business()?.id;

    if (employee.email && businessId) {
      this.businessService.addEmployeeToBusiness(
        businessId,
        employee.email,
        employee.role as string
      ).subscribe({
        next: (updatedBusiness) => {
          this.business.set(updatedBusiness);
          this.employees.set(updatedBusiness.employees || []);
          this.services.set(updatedBusiness.services || []);
          this.isAddingEmployee.set(false);
        },
        error: (error) => {
          console.error('Error adding employee:', error);
          alert('Hiba történt a dolgozó hozzáadása során. Ellenőrizd, hogy az email cím helyes-e.');
        }
      });
    }
  }

  updateEmployeeRole(userId: string, role: string) {
    const businessId = this.business()?.id;
    if (businessId) {
      this.businessService.updateEmployeeRole(businessId, userId, role).subscribe({
        next: () => {
          console.log('Employee role updated successfully');
        },
        error: (error) => {
          console.error('Error updating employee role:', error);
          alert('Hiba történt a szerepkör frissítése során.');
          // Reload to revert the change
          this.tenantService.loadBusinessData().subscribe();
        }
      });
    }
  }

  removeEmployee(userId: string) {
    if (confirm('Biztosan el szeretnéd távolítani ezt a dolgozót az üzletből?')) {
      const businessId = this.business()?.id;
      if (businessId) {
        this.businessService.removeEmployeeFromBusiness(businessId, userId).subscribe({
          next: () => {
            console.log('Employee removed successfully');
            // Reload business data to refresh employee list
            this.tenantService.loadBusinessData().subscribe();
          },
          error: (error) => {
            console.error('Error removing employee:', error);
            alert('Hiba történt a dolgozó eltávolítása során.');
          }
        });
      }
    }
  }

  showAddServiceForm() {
    this.newService.set({
      serviceId: '',
      serviceName: '',
      price: 0,
      duration: 0
    });
    this.isAddingService.set(true);
  }

  cancelAddService() {
    this.isAddingService.set(false);
    this.editingServiceId.set(null);
  }

  addService() {
    const service = this.newService();
    const businessId = this.business()?.id;

    if (service.serviceId && businessId) {
      this.businessService.createBusinessService(
        businessId,
        service.serviceId,
        service.price || 0,
        service.duration || 0
      ).subscribe({
        next: () => {
          this.loadBusinessServices();
          this.isAddingService.set(false);
        },
        error: (error) => {
          console.error('Error creating service:', error);
          alert('Hiba történt a szolgáltatás hozzáadása során.');
        }
      });
    }
  }

  startEditingService(service: Service) {
    this.editingServiceId.set(service.id);
    this.newService.set({
      serviceId: service.serviceId,
      serviceName: service.serviceName,
      price: service.price,
      duration: service.duration
    });
  }

  saveServiceEdit(serviceId: string) {
    const service = this.newService();

    this.businessService.updateBusinessService(
      serviceId,
      service.price,
      service.duration
    ).subscribe({
      next: () => {
        this.loadBusinessServices();
        this.editingServiceId.set(null);
      },
      error: (error) => {
        console.error('Error updating service:', error);
        alert('Hiba történt a szolgáltatás frissítése során.');
      }
    });
  }

  cancelServiceEdit() {
    this.editingServiceId.set(null);
  }

  removeService(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a szolgáltatást?')) {
      this.businessService.deleteBusinessService(id).subscribe({
        next: () => {
          this.loadBusinessServices();
        },
        error: (error) => {
          console.error('Error deleting service:', error);
          alert('Hiba történt a szolgáltatás törlése során.');
        }
      });
    }
  }

  cancelBooking(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a foglalást?')) {
      this.businessService.cancelBooking(id).subscribe({
        next: () => {
          this.loadBusinessBookings();
        },
        error: (error) => {
          console.error('Error canceling booking:', error);
          alert('Hiba történt a foglalás törlése során.');
        }
      });
    }
  }

  formatDate(date: string | Date): string {
    return new Date(date).toLocaleDateString('hu-HU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getServiceName(serviceId: string): string {
    const service = this.availableServices().find(s => s.id === serviceId);
    return service?.value || serviceId;
  }

  onServiceSelect(serviceId: string) {
    const current = this.newService();
    this.newService.set({
      ...current,
      serviceId: serviceId,
      serviceName: this.getServiceName(serviceId)
    });
  }

  private loadBusinessServices() {
    const businessId = this.business()?.id;
    if (businessId) {
      this.businessService.loadBusinessServices(businessId).subscribe({
        next: (services) => {
          this.services.set(services);
        },
        error: (error) => {
          console.error('Error loading services:', error);
        }
      });
    }
  }

  private loadBusinessBookings() {
    const businessId = this.business()?.id;
    if (businessId) {
      this.businessService.loadBusinessBookings(businessId).subscribe({
        next: (bookings) => {
          this.bookings.set(bookings);
        },
        error: (error) => {
          console.error('Error loading bookings:', error);
        }
      });
    }
  }

  private loadAvailableServices() {
    this.businessService.loadAllServices().subscribe({
      next: (services) => {
        this.availableServices.set(services);
      },
      error: (error) => {
        console.error('Error loading available services:', error);
      }
    });
  }

  ngOnInit() {
    this.tenantService.business$.subscribe(business => {
      if (business) {
        this.business.set(business as Business);
        this.employees.set(business.employees || []);
        this.services.set(business.services || []);
        this.editBusinessForm.set({ ...business } as Business);

        // Load only available services catalog and bookings will be lazy loaded
        this.loadAvailableServices();
      }
    });
  }
}
