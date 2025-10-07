import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Employee } from '../../models/employee';
import { InviteEmployeeRequest } from '../../models/auth.models';

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee-management.html'
})
export class EmployeeManagementComponent {
  employees = input.required<Employee[]>();

  employeeInvited = output<InviteEmployeeRequest>();
  employeeRoleUpdated = output<{ userId: string; role: string }>();
  employeeRemoved = output<string>();
  invitationResent = output<string>();

  isAddingEmployee = false;
  isAdding = false;
  isUpdating: { [key: string]: boolean } = {};
  isRemoving: { [key: string]: boolean } = {};
  isResending: { [key: string]: boolean } = {};
  invitationUrl: string | null = null;
  showInvitationModal = false;

  newEmployee = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    role: 'Employee' as 'Employee' | 'Manager' | 'Owner'
  };

  window = window;

  showAddForm() {
    this.newEmployee = {
      firstName: '',
      lastName: '',
      email: '',
      phoneNumber: '',
      role: 'Employee'
    };
    this.isAddingEmployee = true;
  }

  cancelAdd() {
    this.isAddingEmployee = false;
  }

  inviteEmployee() {
    if (this.newEmployee.email && this.newEmployee.firstName && this.newEmployee.lastName && !this.isAdding) {
      this.isAdding = true;
      this.employeeInvited.emit({
        firstName: this.newEmployee.firstName,
        lastName: this.newEmployee.lastName,
        email: this.newEmployee.email,
        phoneNumber: this.newEmployee.phoneNumber,
        role: this.newEmployee.role
      });
    }
  }

  onInviteComplete(invitationUrl: string) {
    this.isAdding = false;
    this.isAddingEmployee = false;
    this.invitationUrl = invitationUrl;
    this.showInvitationModal = true;
  }

  onAddError() {
    this.isAdding = false;
  }

  closeInvitationModal() {
    this.showInvitationModal = false;
    this.invitationUrl = null;
  }

  copyInvitationUrl() {
    if (this.invitationUrl) {
      const fullUrl = window.location.origin + this.invitationUrl;
      navigator.clipboard.writeText(fullUrl);
    }
  }

  resendInvitation(userId: string) {
    if (!this.isResending[userId]) {
      this.isResending[userId] = true;
      this.invitationResent.emit(userId);
    }
  }

  onResendComplete(userId: string, invitationUrl: string) {
    this.isResending[userId] = false;
    this.invitationUrl = invitationUrl;
    this.showInvitationModal = true;
  }

  onResendError(userId: string) {
    this.isResending[userId] = false;
  }

  getEmployeeStatus(employee: Employee): 'active' | 'pending' | 'expired' {
    if (employee.isPasswordSet) return 'active';

    if (employee.invitationExpiresAt) {
      const expiryDate = new Date(employee.invitationExpiresAt);
      return expiryDate > new Date() ? 'pending' : 'expired';
    }

    return 'expired';
  }

  isInvitationExpired(employee: Employee): boolean {
    return this.getEmployeeStatus(employee) === 'expired';
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
