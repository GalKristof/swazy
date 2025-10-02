import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TenantService } from '../services/tenant.service';
import { BusinessService } from '../services/business.service';
import { EmployeeScheduleService } from '../services/employee-schedule.service';
import { Service } from '../models/service';
import { ServiceDetails } from '../models/service.details';
import { EmployeeSchedule } from '../models/employee-schedule';

@Component({
  selector: 'app-barber-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './barber-landing.html',
  styleUrls: ['./barber-landing.scss']
})
export class BarberLandingComponent implements OnInit {
  private tenantService = inject(TenantService);
  private businessService = inject(BusinessService);
  private scheduleService = inject(EmployeeScheduleService);

  business$ = this.tenantService.business$;
  schedules = signal<EmployeeSchedule[]>([]);

  // Computed properties to get real data from business
  services = computed(() => {
    const business = this.tenantService.getCurrentBusiness();
    return business?.services || [];
  });

  // Only show employees with schedules, sorted by availability
  employees = computed(() => {
    const business = this.tenantService.getCurrentBusiness();
    const allEmployees = business?.employees || [];
    const currentSchedules = this.schedules();

    // Filter to only include employees who have schedules
    const employeesWithSchedules = allEmployees.filter(emp =>
      currentSchedules.some(sched => sched.userId === emp.userId)
    );

    // Sort by availability (vacation employees last)
    return this.sortEmployeesByAvailability(employeesWithSchedules);
  });

  ngOnInit() {
    // Load schedules when business is available
    this.tenantService.business$.subscribe(business => {
      if (business?.id) {
        this.loadSchedules(business.id);
      }
    });
  }

  private loadSchedules(businessId: string) {
    this.scheduleService.getSchedulesByBusiness(businessId).subscribe({
      next: (schedules) => {
        this.schedules.set(schedules);
      },
      error: (error) => {
        console.error('Error loading schedules:', error);
      }
    });
  }

  isEmployeeOnVacation(employeeUserId: string): boolean {
    const schedule = this.schedules().find(s => s.userId === employeeUserId);
    if (!schedule || !schedule.vacationFrom || !schedule.vacationTo) return false;

    const now = new Date();
    const vacationStart = new Date(schedule.vacationFrom);
    const vacationEnd = new Date(schedule.vacationTo);

    return now >= vacationStart && now <= vacationEnd;
  }

  private calculateAvailableMinutes(userId: string): number {
    const schedule = this.schedules().find(s => s.userId === userId);
    if (!schedule || this.isEmployeeOnVacation(userId)) return 0;

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

  private sortEmployeesByAvailability(employees: any[]): any[] {
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

  // Mock images for employees (can be replaced with real images later)
  getEmployeeImage(index: number): string {
    const images = [
      'https://images.unsplash.com/photo-1621605815971-fbc98d665033?w=400&h=400&fit=crop',
      'https://images.unsplash.com/photo-1622286342621-4bd786c2447c?w=400&h=400&fit=crop',
      'https://images.unsplash.com/photo-1605497788044-5a32c7078486?w=400&h=400&fit=crop',
      'https://images.unsplash.com/photo-1633332755192-727a05c4013d?w=400&h=400&fit=crop'
    ];
    return images[index % images.length];
  }

  // Helper to get service name from serviceDetails
  getServiceName(serviceId: string): string {
    // This will be improved later when we have service details
    return serviceId;
  }
}
