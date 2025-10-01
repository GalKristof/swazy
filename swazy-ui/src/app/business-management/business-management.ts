import { Component, signal, inject, OnInit, viewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TenantService } from '../services/tenant.service';
import { BusinessService } from '../services/business.service';
import { EmployeeScheduleService } from '../services/employee-schedule.service';
import { ToastService } from '../services/toast.service';
import { Business } from '../models/business';
import { Employee } from '../models/employee';
import { Service } from '../models/service';
import { BookingDetails } from '../models/booking.details';
import { ServiceDetails } from '../models/service.details';
import { EmployeeSchedule } from '../models/employee-schedule';
import { BusinessInfoComponent } from './business-info/business-info';
import { EmployeeManagementComponent } from './employee-management/employee-management';
import { ServiceManagementComponent } from './service-management/service-management';
import { BookingListComponent } from './booking-list/booking-list';
import { ScheduleManagementComponent } from './schedule-management/schedule-management';
import { BookingCalendarComponent } from './booking-calendar/booking-calendar';

@Component({
  selector: 'app-business-management',
  standalone: true,
  imports: [
    CommonModule,
    BusinessInfoComponent,
    EmployeeManagementComponent,
    ServiceManagementComponent,
    BookingListComponent,
    ScheduleManagementComponent,
    BookingCalendarComponent
  ],
  templateUrl: './business-management.html',
  styleUrls: ['./business-management.scss']
})
export class BusinessManagementComponent implements OnInit {
  private tenantService = inject(TenantService);
  private businessService = inject(BusinessService);
  private scheduleService = inject(EmployeeScheduleService);
  private toastService = inject(ToastService);

  businessInfoComponent = viewChild(BusinessInfoComponent);
  employeeComponent = viewChild(EmployeeManagementComponent);
  serviceComponent = viewChild(ServiceManagementComponent);
  bookingComponent = viewChild(BookingListComponent);
  scheduleComponent = viewChild(ScheduleManagementComponent);

  activeTab = signal<'info' | 'employees' | 'services' | 'bookings' | 'schedules' | 'calendar'>('info');

  business = signal<Business | null>(null);
  employees = signal<Employee[]>([]);
  services = signal<Service[]>([]);
  availableServices = signal<ServiceDetails[]>([]);
  bookings = signal<BookingDetails[]>([]);
  schedules = signal<EmployeeSchedule[]>([]);

  setActiveTab(tab: 'info' | 'employees' | 'services' | 'bookings' | 'schedules' | 'calendar') {
    this.activeTab.set(tab);

    if (tab === 'bookings' && this.bookings().length === 0) {
      this.loadBusinessBookings();
    }

    if (tab === 'schedules' && this.schedules().length === 0) {
      this.loadBusinessSchedules();
    }

    if (tab === 'calendar') {
      // Load both bookings and schedules for calendar
      if (this.bookings().length === 0) {
        this.loadBusinessBookings();
      }
      if (this.schedules().length === 0) {
        this.loadBusinessSchedules();
      }
    }
  }

  onBusinessUpdated(updatedBusiness: Business) {
    this.businessService.updateBusiness(updatedBusiness).subscribe({
      next: (business) => {
        this.business.set(business);
        this.employees.set(business.employees || []);
        this.services.set(business.services || []);
        this.businessInfoComponent()?.onSaveComplete();
        this.toastService.success('Üzlet adatai sikeresen frissítve!');
      },
      error: (error) => {
        console.error('Error updating business:', error);
        this.businessInfoComponent()?.onSaveError();
        this.toastService.error('Hiba történt az üzlet adatainak mentése során.');
      }
    });
  }

  onEmployeeAdded(data: { email: string; role: string }) {
    const businessId = this.business()?.id;
    if (businessId) {
      this.businessService.addEmployeeToBusiness(businessId, data.email, data.role).subscribe({
        next: (updatedBusiness) => {
          this.business.set(updatedBusiness);
          this.employees.set(updatedBusiness.employees || []);
          this.services.set(updatedBusiness.services || []);
          this.employeeComponent()?.onAddComplete();
          this.toastService.success('Dolgozó sikeresen hozzáadva!');
        },
        error: (error) => {
          console.error('Error adding employee:', error);
          this.employeeComponent()?.onAddError();
          this.toastService.error('Hiba történt a dolgozó hozzáadása során. Ellenőrizd, hogy az email cím helyes-e.');
        }
      });
    }
  }

  onEmployeeRoleUpdated(data: { userId: string; role: string }) {
    const businessId = this.business()?.id;
    if (businessId) {
      this.businessService.updateEmployeeRole(businessId, data.userId, data.role).subscribe({
        next: () => {
          this.employeeComponent()?.onUpdateComplete(data.userId);
          this.toastService.success('Szerepkör sikeresen frissítve!');
        },
        error: (error) => {
          console.error('Error updating employee role:', error);
          this.employeeComponent()?.onUpdateError(data.userId);
          this.toastService.error('Hiba történt a szerepkör frissítése során.');
          this.tenantService.loadBusinessData().subscribe();
        }
      });
    }
  }

  onEmployeeRemoved(userId: string) {
    if (confirm('Biztosan el szeretnéd távolítani ezt a dolgozót az üzletből?')) {
      const businessId = this.business()?.id;
      if (businessId) {
        this.businessService.removeEmployeeFromBusiness(businessId, userId).subscribe({
          next: () => {
            this.employeeComponent()?.onRemoveComplete(userId);
            this.toastService.success('Dolgozó sikeresen eltávolítva!');
            this.tenantService.loadBusinessData().subscribe();
          },
          error: (error) => {
            console.error('Error removing employee:', error);
            this.employeeComponent()?.onRemoveError(userId);
            this.toastService.error('Hiba történt a dolgozó eltávolítása során.');
          }
        });
      }
    }
  }

  onServiceAdded(data: { serviceId: string; price: number; duration: number }) {
    const businessId = this.business()?.id;
    if (businessId) {
      this.businessService.createBusinessService(businessId, data.serviceId, data.price, data.duration).subscribe({
        next: () => {
          this.loadBusinessServices();
          this.serviceComponent()?.onAddComplete();
          this.toastService.success('Szolgáltatás sikeresen hozzáadva!');
        },
        error: (error) => {
          console.error('Error creating service:', error);
          this.serviceComponent()?.onAddError();
          this.toastService.error('Hiba történt a szolgáltatás hozzáadása során.');
        }
      });
    }
  }

  onServiceUpdated(data: { id: string; price: number; duration: number }) {
    this.businessService.updateBusinessService(data.id, data.price, data.duration).subscribe({
      next: () => {
        this.loadBusinessServices();
        this.serviceComponent()?.onUpdateComplete(data.id);
        this.toastService.success('Szolgáltatás sikeresen frissítve!');
      },
      error: (error) => {
        console.error('Error updating service:', error);
        this.serviceComponent()?.onUpdateError(data.id);
        this.toastService.error('Hiba történt a szolgáltatás frissítése során.');
      }
    });
  }

  onServiceRemoved(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a szolgáltatást?')) {
      this.businessService.deleteBusinessService(id).subscribe({
        next: () => {
          this.loadBusinessServices();
          this.serviceComponent()?.onRemoveComplete(id);
          this.toastService.success('Szolgáltatás sikeresen törölve!');
        },
        error: (error) => {
          console.error('Error deleting service:', error);
          this.serviceComponent()?.onRemoveError(id);
          this.toastService.error('Hiba történt a szolgáltatás törlése során.');
        }
      });
    }
  }

  onBookingCancelled(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a foglalást?')) {
      this.businessService.cancelBooking(id).subscribe({
        next: () => {
          this.loadBusinessBookings();
          this.bookingComponent()?.onCancelComplete(id);
          this.toastService.success('Foglalás sikeresen lemondva!');
        },
        error: (error) => {
          console.error('Error canceling booking:', error);
          this.bookingComponent()?.onCancelError(id);
          this.toastService.error('Hiba történt a foglalás törlése során.');
        }
      });
    }
  }

  onEmployeeSelected(employee: Employee) {
    const businessId = this.business()?.id;
    if (businessId) {
      this.scheduleService.getScheduleByEmployee(employee.userId, businessId).subscribe({
        next: (schedule) => {
          this.scheduleComponent()?.setScheduleData(schedule);
        },
        error: () => {
          this.scheduleComponent()?.setScheduleData(null);
        }
      });
    }
  }

  onScheduleSaved(data: any) {
    const businessId = this.business()?.id;
    if (!businessId) return;

    const scheduleData = {
      bufferTimeMinutes: data.bufferTimeMinutes,
      isOnVacation: data.isOnVacation,
      daySchedules: data.daySchedules
    };

    if (data.currentSchedule) {
      this.scheduleService.updateSchedule({
        id: data.currentSchedule.id,
        ...scheduleData
      }).subscribe({
        next: (updated) => {
          this.scheduleComponent()?.setScheduleData(updated);
          this.loadBusinessSchedules();
          this.scheduleComponent()?.onSaveComplete();
          this.toastService.success('Munkaidő sikeresen frissítve!');
        },
        error: (error) => {
          console.error('Error updating schedule:', error);
          this.scheduleComponent()?.onSaveError();
          this.toastService.error('Hiba történt a munkaidő frissítése során.');
        }
      });
    } else {
      this.scheduleService.createSchedule({
        userId: data.employee.userId,
        businessId: businessId,
        ...scheduleData
      }).subscribe({
        next: (created) => {
          this.scheduleComponent()?.setScheduleData(created);
          this.loadBusinessSchedules();
          this.scheduleComponent()?.onSaveComplete();
          this.toastService.success('Munkaidő sikeresen létrehozva!');
        },
        error: (error) => {
          console.error('Error creating schedule:', error);
          this.scheduleComponent()?.onSaveError();
          this.toastService.error('Hiba történt a munkaidő létrehozása során.');
        }
      });
    }
  }

  onScheduleCopied(data: any) {
    if (!confirm('Biztosan szeretnéd az aktuális munkaidőt másolni az összes dolgozóra?')) {
      this.scheduleComponent()?.onCopyError();
      return;
    }

    const businessId = this.business()?.id;
    const employees = this.employees();
    if (!businessId) return;

    employees.forEach(employee => {
      const existingSchedule = this.schedules().find(s => s.userId === employee.userId);

      if (existingSchedule) {
        this.scheduleService.updateSchedule({
          id: existingSchedule.id,
          ...data
        }).subscribe({
          next: () => console.log(`Schedule updated for ${employee.firstName}`),
          error: (err) => console.error(`Error updating schedule for ${employee.firstName}:`, err)
        });
      } else {
        this.scheduleService.createSchedule({
          userId: employee.userId,
          businessId: businessId,
          ...data
        }).subscribe({
          next: () => console.log(`Schedule created for ${employee.firstName}`),
          error: (err) => console.error(`Error creating schedule for ${employee.firstName}:`, err)
        });
      }
    });

    setTimeout(() => {
      this.loadBusinessSchedules();
      this.scheduleComponent()?.onCopyComplete();
      this.toastService.success('Munkaidők sikeresen másolva!');
    }, 1000);
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

  ngOnInit() {
    this.tenantService.business$.subscribe(business => {
      if (business) {
        this.business.set(business as Business);
        this.employees.set(business.employees || []);
        this.services.set(business.services || []);
        this.loadAvailableServices();
      }
    });
  }
}
