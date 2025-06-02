// swazy-ui/src/app/models/business-service/business-service.dtos.ts
export interface GetBusinessServiceDto {
  id: string; // GUIDs are strings in TypeScript/JSON
  businessId: string;
  serviceId: string;
  price: number;
  duration: number;
}

export interface CreateBusinessServiceDto {
  businessId: string;
  serviceId: string;
  price: number;
  duration: number;
}

export interface UpdateBusinessServiceDto {
  id: string;
  price?: number; // Optional for partial updates
  duration?: number; // Optional for partial updates
}
