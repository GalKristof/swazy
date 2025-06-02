import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  GetBusinessServiceDto,
  CreateBusinessServiceDto,
  UpdateBusinessServiceDto
} from '../models/business/business.service.model';
import { Business as GetBusinessDto } from '../models/business/business.model';

@Injectable({
  providedIn: 'root'
})
export class BusinessServiceService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getBusinesses(): Observable<GetBusinessDto[]> {
    return this.http.get<GetBusinessDto[]>(`${this.apiUrl}/api/business/all`);
  }

  // Now returns Observable<GetBusinessServiceDto[]> as defined in the model file
  getBusinessServices(businessId: string): Observable<GetBusinessServiceDto[]> {
    return this.http.get<GetBusinessServiceDto[]>(`${this.apiUrl}/api/businessservice/business/${businessId}`);
  }

  // Parameter and return type updated to use DTOs from model file
  createBusinessService(service: CreateBusinessServiceDto): Observable<GetBusinessServiceDto> {
    return this.http.post<GetBusinessServiceDto>(`${this.apiUrl}/api/businessservice`, service);
  }

  // Parameter and return type updated to use DTOs from model file
  updateBusinessService(service: UpdateBusinessServiceDto): Observable<GetBusinessServiceDto> {
    return this.http.put<GetBusinessServiceDto>(`${this.apiUrl}/api/businessservice`, service);
  }

  deleteBusinessService(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/businessservice/${id}`);
  }
}
