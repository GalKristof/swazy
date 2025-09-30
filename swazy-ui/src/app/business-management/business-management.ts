import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TenantService } from '../services/tenant.service';
import {Business} from '../models/business';
import {Employee} from '../models/employee';
import {Service} from '../models/service';
import {Booking} from '../models/booking';

@Component({
  selector: 'app-business-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './business-management.html',
  styleUrls: ['./business-management.scss']
})
export class BusinessManagementComponent implements OnInit {
  private tenantService = inject(TenantService);

  // Active tab
  activeTab = signal<'info' | 'employees' | 'services' | 'bookings'>('info');

  // Business info (single business - the logged-in user's business)
  // Set initial signal to null/default structure that will be overwritten
  // We keep the structure for type safety, but it will be overwritten by data load
  business = signal<Business | null>(null);

  // Edit mode for business info
  // Initialize with null or a fallback structure
  isEditingBusiness = signal(false);
  editBusinessForm = signal<Business | null>(null);

  // Employees
  // Set initial signal to empty array, to be overwritten by data load
  employees = signal<Employee[]>([]);

  isAddingEmployee = signal(false);
  newEmployee = signal<Partial<Employee>>({
    firstName: '',
    lastName: '',
    email: '',
    role: 'Employee'
  });

  // Services
  // Keep mock data or empty array for services, as per instruction to NOT use another endpoint
  // Services will likely need a dedicated GET call later, but for now we keep the mock or empty array
  services = signal<Service[]>([
    { id: '1', serviceId: 's1', serviceName: 'Férfi hajvágás', price: 4500, duration: 30 },
    { id: '2', serviceId: 's2', serviceName: 'Szakáll igazítás', price: 2500, duration: 20 },
    { id: '3', serviceId: 's3', serviceName: 'Borotválás', price: 3500, duration: 25 },
    { id: '4', serviceId: 's4', serviceName: 'Hajvágás + szakáll', price: 6000, duration: 45 }
  ]);

  isAddingService = signal(false);
  editingServiceId = signal<string | null>(null);
  newService = signal<Partial<Service>>({
    serviceName: '',
    price: 0,
    duration: 0
  });

  // Bookings
  // Keep mock data or empty array for bookings, as per instruction to NOT use another endpoint
  bookings = signal<Booking[]>([
    {
      id: '1',
      customerName: 'Kiss András',
      customerEmail: 'andras@example.com',
      customerPhone: '+36 30 111 2222',
      serviceName: 'Férfi hajvágás',
      employeeName: 'Nagy Péter',
      bookingDate: new Date('2025-10-02T10:00:00'),
      notes: 'Rövid oldalak'
    },
    {
      id: '2',
      customerName: 'Szabó Gábor',
      customerEmail: 'gabor@example.com',
      customerPhone: '+36 20 333 4444',
      serviceName: 'Hajvágás + szakáll',
      employeeName: 'Tóth Márk',
      bookingDate: new Date('2025-10-02T14:00:00')
    }
  ]);

  // Business Type options
  businessTypes = [
    'BarberSalon',
    'HairSalon',
    'MassageSalon',
    'BeautySalon',
    'NailSalon',
    'Other'
  ];

  // Tab switching
  setActiveTab(tab: 'info' | 'employees' | 'services' | 'bookings') {
    this.activeTab.set(tab);
  }

  // Business Info Methods
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
    if (this.editBusinessForm()) {
      this.business.set({ ...this.editBusinessForm() } as Business);
      this.isEditingBusiness.set(false);
      // TODO: Call backend API - PUT /api/business
    }
  }

  // Employee Methods
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
    const employee: Employee = {
      id: Date.now().toString(),
      firstName: this.newEmployee().firstName || '',
      lastName: this.newEmployee().lastName || '',
      email: this.newEmployee().email || '',
      role: this.newEmployee().role as 'Owner' | 'Manager' | 'Employee'
    };

    this.employees.update(list => [...list, employee]);
    this.isAddingEmployee.set(false);
    // TODO: Call backend API - PUT /api/business/add-employee
  }

  removeEmployee(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a dolgozót?')) {
      this.employees.update(list => list.filter(e => e.id !== id));
      // TODO: Call backend API - DELETE employee access
    }
  }

  // Service Methods
  showAddServiceForm() {
    this.newService.set({
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
    const service: Service = {
      id: Date.now().toString(),
      serviceId: 's' + Date.now(),
      serviceName: this.newService().serviceName || '',
      price: this.newService().price || 0,
      duration: this.newService().duration || 0
    };

    this.services.update(list => [...list, service]);
    this.isAddingService.set(false);
    // TODO: Call backend API - POST /api/businessservice
  }

  startEditingService(service: Service) {
    this.editingServiceId.set(service.id);
    this.newService.set({
      serviceName: service.serviceName,
      price: service.price,
      duration: service.duration
    });
  }

  saveServiceEdit(serviceId: string) {
    this.services.update(list =>
      list.map(s =>
        s.id === serviceId
          ? {
            ...s,
            serviceName: this.newService().serviceName || s.serviceName,
            price: this.newService().price || s.price,
            duration: this.newService().duration || s.duration
          }
          : s
      )
    );
    this.editingServiceId.set(null);
    // TODO: Call backend API - PUT /api/businessservice
  }

  cancelServiceEdit() {
    this.editingServiceId.set(null);
  }

  removeService(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a szolgáltatást?')) {
      this.services.update(list => list.filter(s => s.id !== id));
      // TODO: Call backend API - DELETE /api/businessservice/{id}
    }
  }

  // Booking Methods
  cancelBooking(id: string) {
    if (confirm('Biztosan törölni szeretnéd ezt a foglalást?')) {
      this.bookings.update(list => list.filter(b => b.id !== id));
      // TODO: Call backend API - DELETE /api/booking/{id}
    }
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleDateString('hu-HU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  ngOnInit() {
    // Subscribe to business data (already loaded in AppComponent)
    this.tenantService.business$.subscribe(business => {
      if (business) {
        // 1. Set the main business object
        this.business.set(business as Business);

        // 2. Set the employees array from the business object
        // Use an empty array fallback if employees is missing or null
        this.employees.set(business.employees || []);

        // 3. Update the edit form data
        this.editBusinessForm.set({ ...business } as Business);

        console.log('📄 [Management] Full Business data and Employees received and set.');
      }
    });
  }
}
