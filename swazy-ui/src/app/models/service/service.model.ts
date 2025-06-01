import { BusinessType } from '../business/business.type';

export interface Service {
  id: string;
  tag: string;
  businessType: BusinessType;
  value: string;
}
