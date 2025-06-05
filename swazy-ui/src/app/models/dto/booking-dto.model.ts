export interface GetBookingDto {
  id: string;
  bookingDate: Date;
  notes?: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  businessServiceId: string;
  employeeId?: string;
}

export interface CreateBookingDto {
  bookingDate: string; // Changed from Date to string
  notes?: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  businessServiceId: string;
  employeeId?: string;
}

export interface UpdateBookingDto extends CreateBookingDto {
  id: string;
}
