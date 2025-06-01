import {BusinessService} from '../business/business.service.model';
import {User} from '../user/user.model';
export interface Booking {
  id: string;
  bookingDate: Date;
  notes?: string | undefined;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  businessServiceId: string;
  businessService: BusinessService;
  employeeId?: string | undefined;
  employee: User;
}
