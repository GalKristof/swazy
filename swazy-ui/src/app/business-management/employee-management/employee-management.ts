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

  showRoleChangeModal = false;
  roleChangeData: { userId: string; employee: Employee; oldRole: string; newRole: string } | null = null;

  showRemoveModal = false;
  removeEmployeeData: { userId: string; employee: Employee } | null = null;

  currentRoles: { [key: string]: string } = {};
  pendingRoleChange: { userId: string; newRole: string } | null = null;

  newEmployee = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    role: 'Employee' as 'Employee' | 'Manager' | 'Owner'
  };

  window = window;

  roleDescriptions = {
    'Employee': {
      title: 'Dolgozó',
      description: 'A dolgozó a következőket teheti:',
      permissions: [
        'Saját munkaidő megtekintése és módosítása',
        'Saját foglalások kezelése',
        'Ügyfelek kiszolgálása',
        'Naptár megtekintése'
      ],
      warning: null
    },
    'Manager': {
      title: 'Menedzser',
      description: 'A menedzser mindent tehet, amit a dolgozó, plusz:',
      permissions: [
        'Új dolgozók meghívása',
        'Dolgozók munkaidejének kezelése',
        'Összes foglalás megtekintése és módosítása',
        'Szolgáltatások kezelése',
        'Üzleti statisztikák megtekintése'
      ],
      warning: 'A menedzser nem tud más menedzsereket vagy tulajdonosokat eltávolítani.'
    },
    'Owner': {
      title: 'Tulajdonos',
      description: 'A tulajdonos teljes hozzáféréssel rendelkezik:',
      permissions: [
        'Mindent tehet, amit a menedzser',
        'Új tulajdonosok hozzáadása',
        'Bármely dolgozó szerepkörének módosítása',
        'Bármely dolgozó eltávolítása (beleértve más tulajdonosokat is)',
        'Üzleti beállítások teljes körű módosítása',
        'Pénzügyi adatok megtekintése és kezelése'
      ],
      warning: '⚠️ FIGYELEM: A tulajdonos veled egyenrangú lesz! Képes lesz téged is eltávolítani vagy módosítani a szerepkörödet. Csak megbízható személyeknek adj tulajdonosi jogokat!'
    }
  };

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

  getCurrentRole(userId: string): string {
    if (!this.currentRoles[userId]) {
      const employee = this.employees().find(e => e.userId === userId);
      if (employee) {
        this.currentRoles[userId] = employee.role;
      }
    }
    return this.currentRoles[userId] || 'Employee';
  }

  getDisplayRole(userId: string): string {
    // If there's a pending change for this user, show that temporarily
    if (this.pendingRoleChange && this.pendingRoleChange.userId === userId) {
      return this.pendingRoleChange.newRole;
    }
    // Otherwise show the current role
    return this.getCurrentRole(userId);
  }

  updateRole(userId: string, newRole: string) {
    const employee = this.employees().find(e => e.userId === userId);
    const currentRole = this.getCurrentRole(userId);

    if (employee && currentRole !== newRole && !this.isUpdating[userId]) {
      this.pendingRoleChange = { userId, newRole };
      this.roleChangeData = {
        userId,
        employee,
        oldRole: currentRole,
        newRole
      };
      this.showRoleChangeModal = true;
    }
  }

  confirmRoleChange() {
    if (this.roleChangeData && !this.isUpdating[this.roleChangeData.userId]) {
      this.isUpdating[this.roleChangeData.userId] = true;
      this.currentRoles[this.roleChangeData.userId] = this.roleChangeData.newRole;
      this.employeeRoleUpdated.emit({
        userId: this.roleChangeData.userId,
        role: this.roleChangeData.newRole
      });
      this.showRoleChangeModal = false;
      this.roleChangeData = null;
      this.pendingRoleChange = null;
    }
  }

  cancelRoleChange() {
    // Clear pending change so select reverts to current role
    this.pendingRoleChange = null;
    this.showRoleChangeModal = false;
    this.roleChangeData = null;
  }

  onUpdateComplete(userId: string) {
    this.isUpdating[userId] = false;
  }

  onUpdateError(userId: string) {
    this.isUpdating[userId] = false;
  }

  removeEmployee(userId: string) {
    const employee = this.employees().find(e => e.userId === userId);
    if (employee && !this.isRemoving[userId]) {
      this.removeEmployeeData = { userId, employee };
      this.showRemoveModal = true;
    }
  }

  confirmRemove() {
    if (this.removeEmployeeData && !this.isRemoving[this.removeEmployeeData.userId]) {
      this.isRemoving[this.removeEmployeeData.userId] = true;
      this.employeeRemoved.emit(this.removeEmployeeData.userId);
      this.showRemoveModal = false;
      this.removeEmployeeData = null;
    }
  }

  cancelRemove() {
    this.showRemoveModal = false;
    this.removeEmployeeData = null;
  }

  onRemoveComplete(userId: string) {
    this.isRemoving[userId] = false;
  }

  onRemoveError(userId: string) {
    this.isRemoving[userId] = false;
  }

  getRoleDescription(role: string) {
    return this.roleDescriptions[role as 'Employee' | 'Manager' | 'Owner'];
  }
}
