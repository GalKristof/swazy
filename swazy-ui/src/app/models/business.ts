import {Employee} from './employee';

export interface Business {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  email: string;
  businessType: string;
  websiteUrl: string;
  employees?: Employee[];
}
