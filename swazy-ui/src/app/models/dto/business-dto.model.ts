import { BusinessType } from '../business-type.enum';
import { BusinessRole } from '../business/business.role.type';
import { BusinessService } from '../business/business.service.model';

export interface GetBusinessDto {
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

export interface CreateBusinessDto {
  name: string;
  address: string;
  phoneNumber: string;
  email: string;
  businessType: BusinessType;
  websiteUrl: string;
}

export interface UpdateBusinessDto {
  id: string;
  name?: string;
  address?: string;
  phoneNumber?: string;
  email?: string;
  businessType?: BusinessType;
  websiteUrl?: string;
}
