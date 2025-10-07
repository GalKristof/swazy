import { Injectable } from '@angular/core';

export type BusinessRole = 'Employee' | 'Manager' | 'Owner';

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {

  canRemoveEmployee(currentUserRole: BusinessRole, targetEmployeeRole: BusinessRole): boolean {
    if (currentUserRole === 'Owner') return true; // Owner can remove anyone
    return false; // Manager and Employee cannot remove anyone
  }

  canUpdateEmployeeRole(currentUserRole: BusinessRole): boolean {
    return currentUserRole === 'Owner'; // Only Owners can change roles
  }

  canInviteEmployee(currentUserRole: BusinessRole): boolean {
    return currentUserRole === 'Manager' || currentUserRole === 'Owner';
  }

  canUpdateBusinessSettings(currentUserRole: BusinessRole): boolean {
    return currentUserRole === 'Owner';
  }

  canManageServices(currentUserRole: BusinessRole): boolean {
    return currentUserRole === 'Manager' || currentUserRole === 'Owner';
  }

  canManageSchedules(currentUserRole: BusinessRole): boolean {
    return currentUserRole === 'Manager' || currentUserRole === 'Owner';
  }

  canViewAllBookings(currentUserRole: BusinessRole): boolean {
    return currentUserRole === 'Manager' || currentUserRole === 'Owner';
  }
}
