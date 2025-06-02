import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetServiceDto } from '../models/service.model';
import { CreateServiceDto } from '../models/service.model';
import { UpdateServiceDto } from '../models/service.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ServiceService {
  private apiUrl = `${environment.apiUrl}/service`;

  constructor(private http: HttpClient) { }

  getServices(): Observable<GetServiceDto[]> {
    return this.http.get<GetServiceDto[]>(`${this.apiUrl}/all`);
  }

  createService(service: CreateServiceDto): Observable<GetServiceDto> {
    return this.http.post<GetServiceDto>(this.apiUrl, service);
  }

  updateService(service: UpdateServiceDto): Observable<GetServiceDto> {
    return this.http.put<GetServiceDto>(this.apiUrl, service);
  }

  deleteService(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
