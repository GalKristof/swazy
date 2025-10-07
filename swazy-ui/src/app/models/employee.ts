export interface Employee {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Owner' | 'Manager' | 'Employee';
  isPasswordSet: boolean;
  invitationExpiresAt: string | null;
}
