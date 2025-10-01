import {Employee} from './employee';
import {Service} from './service';

export interface Business {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  email: string;
  businessType: string;
  websiteUrl: string;
  employees?: Employee[];
  services?: Service[];
}
