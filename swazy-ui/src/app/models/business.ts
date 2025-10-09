import {Employee} from './employee';
import {Service} from './service';

export interface Business {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  email: string;
  title: string;
  subtitle: string;
  description: string;
  footer: string;
  theme: string;
  businessType: string;
  websiteUrl: string;
  employees?: Employee[];
  services?: BusinessService[];
  createdAt?: string;
}

export interface BusinessService {
  id: string;
  businessId: string;
  serviceId: string;
  serviceName: string;
  price: number;
  duration: number;
  createdAt: string;
}
