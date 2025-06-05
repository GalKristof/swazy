export interface BookingDetailsDto {
  id: string; // Guid from backend
  startTime: Date; // Mapped from DateTimeOffset
  endTime: Date; // Mapped from DateTimeOffset
  serviceName: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  notes?: string;
  employeeId?: string; // Guid from backend
}
