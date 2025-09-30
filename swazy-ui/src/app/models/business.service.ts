export interface BusinessService {
  id: string;
  businessId: string;
  serviceId: string;
  price: number;
  duration: number;
  createdAt: string;
  serviceName?: string;
}
