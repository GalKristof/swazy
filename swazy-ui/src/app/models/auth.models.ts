export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  systemRole?: string;
  businessRole?: 'Employee' | 'Manager' | 'Owner';
  businesses?: UserBusinessResponse[];
  createdAt?: string;
}

export interface UserBusinessResponse {
  businessId: string;
  businessName: string;
  role: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface SetupPasswordRequest {
  invitationToken: string;
  password: string;
}

export interface InviteEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  role: 'Employee' | 'Manager' | 'Owner';
}

export interface InvitationResponse {
  invitationUrl: string;
  invitationToken: string;
  expiresAt: string;
  employee: EmployeeInfo;
}

export interface EmployeeInfo {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isPasswordSet: boolean;
  invitationExpiresAt: string | null;
}
