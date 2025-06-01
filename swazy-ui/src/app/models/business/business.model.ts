import {BusinessRole} from './business.role.type';
import {BusinessType} from './business.type';
import {BusinessService} from './business.service.model';

export interface Business {
  id: string;
  name: string;
  address: string;
  phoneNumber: string;
  email: string;
  businessType: BusinessType;
  employees: Map<string, BusinessRole>;
  websiteUrl: string;
  services: BusinessService[];
}
