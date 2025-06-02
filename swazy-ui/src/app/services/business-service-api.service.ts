import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateBusinessServiceDto, GetBusinessServiceDto, UpdateBusinessServiceDto } from '../models/business-service/business-service.dtos';

@Injectable({
  providedIn: 'root'
})
export class BusinessServiceApiService {
  // From SwazyConstants.cs, BusinessServiceModuleApi is "businessservice"
  private apiUrl = `${environment.apiUrl}/businessservice`; 

  constructor(private http: HttpClient) { }

  getBusinessServicesByBusinessId(businessId: string): Observable<GetBusinessServiceDto[]> {
    return this.http.get<GetBusinessServiceDto[]>(`${this.apiUrl}/business/${businessId}`);
  }

  createBusinessService(data: CreateBusinessServiceDto): Observable<GetBusinessServiceDto> {
    return this.http.post<GetBusinessServiceDto>(this.apiUrl, data);
  }

  // The backend UpdateBusinessServiceDto has 'Id' in the body.
  // The standard PUT endpoint for business service in BusinessServiceModule is just /api/businessservice
  // and expects the whole DTO (including ID) in the body for updates.
  updateBusinessService(data: UpdateBusinessServiceDto): Observable<GetBusinessServiceDto> {
    // Assuming the API expects PUT to the base URL for updates, and ID is in the DTO.
    // If it expected PUT to /api/businessservice/{id}, this would be:
    // return this.http.put<GetBusinessServiceDto>(`${this.apiUrl}/${data.id}`, data);
    // But current BusinessServiceModule.cs for PUT is: endpoints.MapPut($"api/{SwazyConstants.BusinessServiceModuleApi}"...
    // This means it does not take ID in the URL for PUT.
    return this.http.put<GetBusinessServiceDto>(this.apiUrl, data);
  }

  deleteBusinessService(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
