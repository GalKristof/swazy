export interface Booking {
  id: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  serviceName: string;
  employeeName: string;
  bookingDate: Date;
  notes?: string;
}
