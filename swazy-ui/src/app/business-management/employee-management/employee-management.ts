import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Employee } from '../../models/employee';

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee-management.html'
})
export class EmployeeManagementComponent {
  employees = input.required<Employee[]>();

  employeeAdded = output<{ email: string; role: string }>();
  employeeRoleUpdated = output<{ userId: string; role: string }>();
  employeeRemoved = output<string>();

  isAddingEmployee = false;
  isAdding = false;
  isUpdating: { [key: string]: boolean } = {};
  isRemoving: { [key: string]: boolean } = {};
  newEmployee = {
    email: '',
    role: 'Employee'
  };

  showAddForm() {
    this.newEmployee = { email: '', role: 'Employee' };
    this.isAddingEmployee = true;
  }

  cancelAdd() {
    this.isAddingEmployee = false;
  }

  addEmployee() {
    if (this.newEmployee.email && !this.isAdding) {
      this.isAdding = true;
      this.employeeAdded.emit({
        email: this.newEmployee.email,
        role: this.newEmployee.role
      });
    }
  }

  onAddComplete() {
    this.isAdding = false;
    this.isAddingEmployee = false;
  }

  onAddError() {
    this.isAdding = false;
  }

  updateRole(userId: string, role: string) {
    if (!this.isUpdating[userId]) {
      this.isUpdating[userId] = true;
      this.employeeRoleUpdated.emit({ userId, role });
    }
  }

  onUpdateComplete(userId: string) {
    this.isUpdating[userId] = false;
  }

  onUpdateError(userId: string) {
    this.isUpdating[userId] = false;
  }

  removeEmployee(userId: string) {
    if (!this.isRemoving[userId]) {
      this.isRemoving[userId] = true;
      this.employeeRemoved.emit(userId);
    }
  }

  onRemoveComplete(userId: string) {
    this.isRemoving[userId] = false;
  }

  onRemoveError(userId: string) {
    this.isRemoving[userId] = false;
  }
}
