export interface GetBusinessServiceDto {
  id: string;
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
  price?: number;
  duration?: number;
}
