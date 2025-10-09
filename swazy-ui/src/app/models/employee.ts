export interface Employee {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: 'Owner' | 'Manager' | 'Employee' | string;
  isPasswordSet: boolean;
  invitationExpiresAt: string | null;
}
