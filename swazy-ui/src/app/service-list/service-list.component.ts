import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms'; // Import FormsModule for ngModel

import { GetServiceDto, CreateServiceDto, UpdateServiceDto } from '../models/service.model';
import { BusinessType } from '../models/business-type.enum';
import { ServiceService } from '../services/service.service';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './service-list.component.html', // Corrected template path
  styleUrl: './service-list.component.scss' // Corrected style path
})
export class ServiceListComponent implements OnInit {
  services: GetServiceDto[] = [];
  newService: CreateServiceDto = { tag: '', businessType: BusinessType.None, value: '' };
  editingService: UpdateServiceDto | null = null;
  businessTypes = Object.values(BusinessType);

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private serviceService: ServiceService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadServices();
    }
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
