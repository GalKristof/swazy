export interface BookingDetails {
  id: string;
  startTime: string;
  endTime: string;
  serviceName: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  notes?: string;
  employeeId?: string;
  bookedByUserId?: string;
}
