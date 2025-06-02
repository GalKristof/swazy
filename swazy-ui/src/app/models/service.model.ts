import { BusinessType } from './business-type.enum';

export interface GetServiceDto {
  id: string;
  tag: string;
  businessType: BusinessType;
  value: string;
}

export interface CreateServiceDto {
  tag: string;
  businessType: BusinessType;
  value: string;
}

export interface UpdateServiceDto {
  id: string;
  tag: string;
  businessType: BusinessType;
  value: string;
}
