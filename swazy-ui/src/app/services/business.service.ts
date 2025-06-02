import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetBusinessDto, CreateBusinessDto, UpdateBusinessDto } from '../models/dto/business-dto.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BusinessService {
  private apiUrl = `${environment.apiUrl}/business`;

  constructor(private http: HttpClient) { }

  getBusinesses(): Observable<GetBusinessDto[]> {
    return this.http.get<GetBusinessDto[]>(`${this.apiUrl}/all`);
  }

  getBusinessById(id: string): Observable<GetBusinessDto> {
    return this.http.get<GetBusinessDto>(`${this.apiUrl}/${id}`);
  }

  createBusiness(business: CreateBusinessDto): Observable<GetBusinessDto> {
    return this.http.post<GetBusinessDto>(this.apiUrl, business);
  }

  updateBusiness(business: UpdateBusinessDto): Observable<GetBusinessDto> {
    return this.http.put<GetBusinessDto>(`${this.apiUrl}/${business.id}`, business);
  }

  deleteBusiness(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
