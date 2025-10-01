import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Service } from '../../models/service';
import { ServiceDetails } from '../../models/service.details';

@Component({
  selector: 'app-service-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './service-management.html'
})
export class ServiceManagementComponent {
  services = input.required<Service[]>();
  availableServices = input.required<ServiceDetails[]>();

  serviceAdded = output<{ serviceId: string; price: number; duration: number }>();
  serviceUpdated = output<{ id: string; price: number; duration: number }>();
  serviceRemoved = output<string>();

  isAddingService = false;
  isAdding = false;
  editingServiceId: string | null = null;
  isUpdating: { [key: string]: boolean } = {};
  isRemoving: { [key: string]: boolean } = {};
  serviceForm = {
    serviceId: '',
    serviceName: '',
    price: 0,
    duration: 0
  };

  showAddForm() {
    this.serviceForm = { serviceId: '', serviceName: '', price: 0, duration: 0 };
    this.isAddingService = true;
  }

  cancelAdd() {
    this.isAddingService = false;
    this.editingServiceId = null;
  }

  onServiceSelect(serviceId: string) {
    this.serviceForm.serviceId = serviceId;
    this.serviceForm.serviceName = this.getServiceName(serviceId);
  }

  addService() {
    if (this.serviceForm.serviceId && !this.isAdding) {
      this.isAdding = true;
      this.serviceAdded.emit({
        serviceId: this.serviceForm.serviceId,
        price: this.serviceForm.price || 0,
        duration: this.serviceForm.duration || 0
      });
    }
  }

  onAddComplete() {
    this.isAdding = false;
    this.isAddingService = false;
  }

  onAddError() {
    this.isAdding = false;
  }

  startEditing(service: Service) {
    this.editingServiceId = service.id;
    this.serviceForm = {
      serviceId: service.serviceId,
      serviceName: service.serviceName,
      price: service.price,
      duration: service.duration
    };
  }

  saveEdit(serviceId: string) {
    if (!this.isUpdating[serviceId]) {
      this.isUpdating[serviceId] = true;
      this.serviceUpdated.emit({
        id: serviceId,
        price: this.serviceForm.price,
        duration: this.serviceForm.duration
      });
    }
  }

  onUpdateComplete(serviceId: string) {
    this.isUpdating[serviceId] = false;
    this.editingServiceId = null;
  }

  onUpdateError(serviceId: string) {
    this.isUpdating[serviceId] = false;
  }

  cancelEdit() {
    this.editingServiceId = null;
  }

  removeService(id: string) {
    if (!this.isRemoving[id]) {
      this.isRemoving[id] = true;
      this.serviceRemoved.emit(id);
    }
  }

  onRemoveComplete(id: string) {
    this.isRemoving[id] = false;
  }

  onRemoveError(id: string) {
    this.isRemoving[id] = false;
  }

  getServiceName(serviceId: string): string {
    const service = this.availableServices().find(s => s.id === serviceId);
    return service?.value || serviceId;
  }
}
