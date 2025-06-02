import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceService } from './services/service.service';
import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from './models/service.model';
import { BusinessType } from './models/business-type.enum';
import { FormsModule } from '@angular/forms'; // Import FormsModule

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule], // Add CommonModule and FormsModule
  providers: [],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  services: GetServiceDto[] = [];
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;
  businessTypes = Object.values(BusinessType).filter(value => typeof value === 'number');
  // Helper to get the string value of BusinessType for the UI
  BusinessTypeLabel = BusinessType;

  constructor(private serviceService: ServiceService) {}

  ngOnInit(): void {
    this.loadServices();
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
}
