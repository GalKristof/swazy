import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { BusinessRole } from '../models/business/business.role.type';
import { TenantService } from '../services/tenant.service';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.scss']
})
export class EmployeesComponent implements OnInit {
  employees: Map<string, BusinessRole> = new Map<string, BusinessRole>();
  newEmployee = {
    userEmail: '',
    role: BusinessRole.Employee
  };
  selectedBusinessId: string = '';
  businessRoles = Object.values(BusinessRole);
  isLoading: boolean = false;

  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private http: HttpClient,
    private tenantService: TenantService
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.tenantService.tenantBusinessObs$.subscribe(tenantBusiness => {
        if (tenantBusiness && tenantBusiness.id) {
          this.selectedBusinessId = tenantBusiness.id;
          this.loadEmployees();
        } else {
          this.selectedBusinessId = '';
          this.employees = new Map<string, BusinessRole>();
        }
      });
    }
  }

  loadEmployees(): void {
    if (!this.selectedBusinessId) {
      return;
    }

    this.isLoading = true;
    this.http.get<any>(`${environment.apiUrl}/business/${this.selectedBusinessId}`)
      .subscribe({
        next: (business) => {
          this.employees = new Map(Object.entries(business.employees || {}));
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading employees:', error);
          this.isLoading = false;
        }
      });
  }

  inviteEmployee(): void {
    if (!this.selectedBusinessId || !this.newEmployee.userEmail) {
      return;
    }

    const payload = {
      businessId: this.selectedBusinessId,
      userEmail: this.newEmployee.userEmail,
      role: this.newEmployee.role
    };

    this.http.put(`${environment.apiUrl}/business/add-employee`, payload)
      .subscribe({
        next: () => {
          // Refresh employee list
          this.loadEmployees();
          // Reset form
          this.newEmployee = {
            userEmail: '',
            role: BusinessRole.Employee
          };
        },
        error: (error) => {
          console.error('Error inviting employee:', error);
        }
      });
  }

  getEmployeeEntries(): [string, BusinessRole][] {
    return Array.from(this.employees.entries());
  }
}
