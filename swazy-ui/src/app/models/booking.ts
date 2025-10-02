export interface Booking {
  id: string;
  confirmationCode: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  serviceName: string;
  employeeName: string;
  bookingDate: Date;
  notes?: string;
}

export interface BookingDetailsResponse {
  id: string;
  confirmationCode: string;
  startTime: string;
  endTime: string;
  serviceName: string;
  serviceDuration: number;
  servicePrice: number;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  notes?: string;
  employeeName?: string;
  businessName: string;
  businessAddress?: string;
  businessPhone?: string;
  employeeId?: string;
  bookedByUserId?: string;
}
