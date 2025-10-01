import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TenantService } from '../services/tenant.service';
import { BusinessService } from '../services/business.service';
import { EmployeeScheduleService } from '../services/employee-schedule.service';
import { Business } from '../models/business';
import { Employee } from '../models/employee';
import { Service } from '../models/service';
import { BookingDetails } from '../models/booking.details';
import { ServiceDetails } from '../models/service.details';
import { EmployeeSchedule, DaySchedule } from '../models/employee-schedule';

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
  private scheduleService = inject(EmployeeScheduleService);

  activeTab = signal<'info' | 'employees' | 'services' | 'bookings' | 'schedules'>('info');

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

  // Schedule management
  schedules = signal<EmployeeSchedule[]>([]);
  selectedEmployeeForSchedule = signal<Employee | null>(null);
  currentSchedule = signal<EmployeeSchedule | null>(null);
  isEditingSchedule = signal(false);
  editScheduleForm = signal<DaySchedule[]>([]);
  bufferTimeMinutes = signal(15);
  bufferTimeMinutesValue = 15; // For two-way binding
  isOnVacation = false; // For two-way binding

  dayNames = ['Vasárnap', 'Hétfő', 'Kedd', 'Szerda', 'Csütörtök', 'Péntek', 'Szombat'];
  dayOrder = [1, 2, 3, 4, 5, 6, 0]; // Monday to Sunday

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

  setActiveTab(tab: 'info' | 'employees' | 'services' | 'bookings' | 'schedules') {
    this.activeTab.set(tab);

    // Lazy load bookings only when the bookings tab is opened
    if (tab === 'bookings' && this.bookings().length === 0) {
      this.loadBusinessBookings();
    }

    // Lazy load schedules only when the schedules tab is opened
    if (tab === 'schedules' && this.schedules().length === 0) {
      this.loadBusinessSchedules();
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

  private loadBusinessSchedules() {
    const businessId = this.business()?.id;
    if (businessId) {
      this.scheduleService.getSchedulesByBusiness(businessId).subscribe({
        next: (schedules) => {
          this.schedules.set(schedules);
        },
        error: (error) => {
          console.error('Error loading schedules:', error);
        }
      });
    }
  }

  selectEmployeeForSchedule(employee: Employee) {
    this.selectedEmployeeForSchedule.set(employee);
    const businessId = this.business()?.id;

    if (businessId) {
      this.scheduleService.getScheduleByEmployee(employee.userId, businessId).subscribe({
        next: (schedule) => {
          this.currentSchedule.set(schedule);
          this.bufferTimeMinutes.set(schedule.bufferTimeMinutes);
          this.bufferTimeMinutesValue = schedule.bufferTimeMinutes;
          this.isOnVacation = schedule.isOnVacation;
          this.editScheduleForm.set([...schedule.daySchedules]);
        },
        error: (error) => {
          // Schedule doesn't exist, create a default one
          this.currentSchedule.set(null);
          this.bufferTimeMinutes.set(15);
          this.bufferTimeMinutesValue = 15;
          this.isOnVacation = false;
          this.editScheduleForm.set(this.createDefaultWeekSchedule());
        }
      });
    }
  }

  hasSchedule(userId: string): boolean {
    return this.schedules().some(s => s.userId === userId);
  }

  isEmployeeOnVacation(userId: string): boolean {
    return this.schedules().find(s => s.userId === userId)?.isOnVacation || false;
  }

  calculateAvailableMinutes(userId: string): number {
    const schedule = this.schedules().find(s => s.userId === userId);
    if (!schedule || schedule.isOnVacation) return 0;

    return schedule.daySchedules
      .filter(day => day.isWorkingDay)
      .reduce((total, day) => {
        const start = this.parseTime(day.startTime);
        const end = this.parseTime(day.endTime);
        return total + (end - start);
      }, 0);
  }

  private parseTime(timeStr: string | null): number {
    if (!timeStr) return 0;
    const [hours, minutes] = timeStr.split(':').map(Number);
    return hours * 60 + minutes;
  }

  getSortedEmployees(): Employee[] {
    const employees = this.employees();
    return [...employees].sort((a, b) => {
      const aOnVacation = this.isEmployeeOnVacation(a.userId);
      const bOnVacation = this.isEmployeeOnVacation(b.userId);

      // Vacation employees go last
      if (aOnVacation && !bOnVacation) return 1;
      if (!aOnVacation && bOnVacation) return -1;

      // For non-vacation employees, sort by available time (most first)
      const aMinutes = this.calculateAvailableMinutes(a.userId);
      const bMinutes = this.calculateAvailableMinutes(b.userId);
      return bMinutes - aMinutes;
    });
  }

  createDefaultWeekSchedule(): DaySchedule[] {
    return Array.from({ length: 7 }, (_, i) => ({
      dayOfWeek: i,
      isWorkingDay: i >= 1 && i <= 5, // Monday to Friday
      startTime: '09:00:00',
      endTime: '17:00:00'
    }));
  }

  toggleWorkingDay(daySchedule: DaySchedule) {
    // Toggle is already handled by ngModel, no need to manually toggle
    // Just ensure the form is updated
    const form = this.editScheduleForm();
    this.editScheduleForm.set([...form]);
  }

  getOrderedDaySchedules(): DaySchedule[] {
    const schedules = this.editScheduleForm();
    return this.dayOrder.map(dayIndex =>
      schedules.find(s => s.dayOfWeek === dayIndex)!
    ).filter(s => s !== undefined);
  }

  saveSchedule() {
    const employee = this.selectedEmployeeForSchedule();
    const businessId = this.business()?.id;
    const currentSched = this.currentSchedule();

    if (!employee || !businessId) return;

    const scheduleData = {
      bufferTimeMinutes: this.bufferTimeMinutesValue,
      isOnVacation: this.isOnVacation,
      daySchedules: this.editScheduleForm()
    };

    if (currentSched) {
      // Update existing schedule
      this.scheduleService.updateSchedule({
        id: currentSched.id,
        ...scheduleData
      }).subscribe({
        next: (updated) => {
          this.currentSchedule.set(updated);
          this.loadBusinessSchedules();
          alert('Munkaidő sikeresen frissítve!');
        },
        error: (error) => {
          console.error('Error updating schedule:', error);
          alert('Hiba történt a munkaidő frissítése során.');
        }
      });
    } else {
      // Create new schedule
      this.scheduleService.createSchedule({
        userId: employee.userId,
        businessId: businessId,
        ...scheduleData
      }).subscribe({
        next: (created) => {
          this.currentSchedule.set(created);
          this.loadBusinessSchedules();
          alert('Munkaidő sikeresen létrehozva!');
        },
        error: (error) => {
          console.error('Error creating schedule:', error);
          alert('Hiba történt a munkaidő létrehozása során.');
        }
      });
    }
  }

  copyToAllEmployees() {
    if (!confirm('Biztosan szeretnéd az aktuális munkaidőt másolni az összes dolgozóra?')) {
      return;
    }

    const businessId = this.business()?.id;
    const employees = this.employees();
    const scheduleData = {
      bufferTimeMinutes: this.bufferTimeMinutesValue,
      isOnVacation: this.isOnVacation,
      daySchedules: this.editScheduleForm()
    };

    if (!businessId) return;

    // Create/update schedules for all employees
    employees.forEach(employee => {
      const existingSchedule = this.schedules().find(s => s.userId === employee.userId);

      if (existingSchedule) {
        this.scheduleService.updateSchedule({
          id: existingSchedule.id,
          ...scheduleData
        }).subscribe({
          next: () => console.log(`Schedule updated for ${employee.firstName}`),
          error: (err) => console.error(`Error updating schedule for ${employee.firstName}:`, err)
        });
      } else {
        this.scheduleService.createSchedule({
          userId: employee.userId,
          businessId: businessId,
          ...scheduleData
        }).subscribe({
          next: () => console.log(`Schedule created for ${employee.firstName}`),
          error: (err) => console.error(`Error creating schedule for ${employee.firstName}:`, err)
        });
      }
    });

    setTimeout(() => {
      this.loadBusinessSchedules();
      alert('Munkaidők sikeresen másolva!');
    }, 1000);
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
