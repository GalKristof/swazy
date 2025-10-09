import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BusinessService } from '../../services/business.service';
import { ToastService } from '../../services/toast.service';

interface ServiceItem {
  id: string;
  tag: string;
  businessType: string;
  value: string;
  createdAt: string;
  usageCount: number;
}

@Component({
  selector: 'app-service-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './service-list.component.html',
  styleUrl: './service-list.component.scss'
})
export class ServiceListComponent implements OnInit {
  private businessService = inject(BusinessService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);

  services = signal<ServiceItem[]>([]);
  isLoading = signal(true);

  // Create/Edit modal state
  showModal = signal(false);
  isEditMode = signal(false);
  currentService = signal<ServiceItem | null>(null);

  // Form fields
  formTag = signal('');
  formBusinessType = signal('BarberShop');
  formValue = signal('');

  businessTypes = ['BarberShop', 'BeautySalon', 'Spa', 'Gym', 'Other'];

  ngOnInit(): void {
    this.loadServices();

    // Check if there's a serviceId in query params to open automatically
    this.route.queryParams.subscribe(params => {
      const serviceId = params['serviceId'];
      if (serviceId) {
        // Find and open the service after loading
        const service = this.services().find(s => s.id === serviceId);
        if (service) {
          this.openEditModal(service);
        }
      }
    });
  }

  loadServices(): void {
    this.isLoading.set(true);
    this.businessService.loadAllServices().subscribe({
      next: (services: any[]) => {
        // Sort by usage count descending (most used first)
        const sortedServices = services.sort((a, b) => b.usageCount - a.usageCount);
        this.services.set(sortedServices);
        this.isLoading.set(false);

        // Check if there's a serviceId in query params to open automatically
        this.route.queryParams.subscribe(params => {
          const serviceId = params['serviceId'];
          if (serviceId) {
            const service = this.services().find(s => s.id === serviceId);
            if (service) {
              setTimeout(() => this.openEditModal(service), 100);
            }
          }
        });
      },
      error: (error) => {
        console.error('Error loading services:', error);
        this.toastService.error('Error loading services');
        this.isLoading.set(false);
      }
    });
  }

  openCreateModal(): void {
    this.isEditMode.set(false);
    this.currentService.set(null);
    this.formTag.set('');
    this.formBusinessType.set('BarberShop');
    this.formValue.set('');
    this.showModal.set(true);
  }

  openEditModal(service: ServiceItem): void {
    this.isEditMode.set(true);
    this.currentService.set(service);
    this.formTag.set(service.tag);
    this.formBusinessType.set(service.businessType);
    this.formValue.set(service.value);
    this.showModal.set(true);
  }

  closeModal(): void {
    this.showModal.set(false);
    this.currentService.set(null);
  }

  saveService(): void {
    if (!this.formTag() || !this.formBusinessType() || !this.formValue()) {
      this.toastService.error('Please fill all fields');
      return;
    }

    if (this.isEditMode()) {
      this.updateService();
    } else {
      this.createService();
    }
  }

  createService(): void {
    this.businessService.createService({
      tag: this.formTag(),
      businessType: this.formBusinessType(),
      value: this.formValue()
    }).subscribe({
      next: () => {
        this.toastService.success('Service created successfully');
        this.closeModal();
        this.loadServices();
      },
      error: (error) => {
        console.error('Error creating service:', error);
        this.toastService.error('Error creating service');
      }
    });
  }

  updateService(): void {
    const service = this.currentService();
    if (!service) return;

    this.businessService.updateService({
      id: service.id,
      tag: this.formTag(),
      businessType: this.formBusinessType(),
      value: this.formValue()
    }).subscribe({
      next: () => {
        this.toastService.success('Service updated successfully');
        this.closeModal();
        this.loadServices();
      },
      error: (error) => {
        console.error('Error updating service:', error);
        this.toastService.error('Error updating service');
      }
    });
  }

  deleteService(service: ServiceItem): void {
    if (service.usageCount >= 1) {
      this.toastService.error(`Cannot delete service "${service.value}" - it is in use by ${service.usageCount} ${service.usageCount === 1 ? 'business' : 'businesses'}`);
      return;
    }

    if (!confirm(`Are you sure you want to delete "${service.value}"?`)) {
      return;
    }

    this.businessService.deleteService(service.id).subscribe({
      next: () => {
        this.toastService.success('Service deleted successfully');
        this.loadServices();
      },
      error: (error) => {
        console.error('Error deleting service:', error);
        const errorMessage = error.error?.error || 'Error deleting service';
        this.toastService.error(errorMessage);
      }
    });
  }
}
