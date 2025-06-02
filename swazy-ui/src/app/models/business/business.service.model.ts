import { Service } from '../service/service.model';
import {Business} from './business.model';

export interface BusinessService {
  id: string;
  price: string;
  duration: number;
  serviceId: string;
  service: Service;
  businessId: string;
  business: Business;
}
