import { Service } from '../service/service.model'; // Assuming Service has name/description
import { Business } from './business.model';

// Renamed original interface to avoid conflict and indicate it's a raw mapping
export interface RawBusinessService {
  id: string;
  price: string;
  duration: number;
  serviceId: string;
  service: Service; // This likely contains the name and description
  businessId: string;
  business: Business;
}

// DTOs as requested by the app.component.ts prompt

// This DTO is what the component expects.
// Mapping from RawBusinessService to GetBusinessServiceDto might be needed
// if the backend/service returns RawBusinessService.
// For now, assuming 'service.name' and 'service.description' can be used.
export interface GetBusinessServiceDto {
  id: string;
  name: string; // Potentially from RawBusinessService.service.name
  description: string; // Potentially from RawBusinessService.service.description
  businessId: string;
  // Include price and duration if they should also be displayed/used directly
  price?: string;
  duration?: number;
  serviceId?: string; // original serviceId
}

export interface CreateBusinessServiceDto {
  name: string;
  description: string;
  price: string; // Added as per original service DTO, can be made optional if needed
  duration: number; // Added as per original service DTO
  serviceId?: string; // If a pre-existing service is being linked
  businessId: string;
}

export interface UpdateBusinessServiceDto {
  id: string;
  name?: string;
  description?: string;
  price?: string;
  duration?: number;
  serviceId?: string;
}
