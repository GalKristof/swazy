import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  EmployeeSchedule,
  CreateEmployeeSchedule,
  UpdateEmployeeSchedule,
  AvailableTimeSlots
} from '../models/employee-schedule';

@Injectable({
  providedIn: 'root'
})
export class EmployeeScheduleService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/employeeschedule`;

  /**
   * Create a new employee schedule
   * POST /api/employeeschedule
   */
  createSchedule(schedule: CreateEmployeeSchedule): Observable<EmployeeSchedule> {
    console.log('[EmployeeScheduleService] Creating schedule:', schedule);
    return this.http.post<EmployeeSchedule>(this.baseUrl, schedule);
  }

  /**
   * Get all schedules for a business
   * GET /api/employeeschedule/business/{businessId}
   */
  getSchedulesByBusiness(businessId: string): Observable<EmployeeSchedule[]> {
    const url = `${this.baseUrl}/business/${businessId}`;
    console.log('[EmployeeScheduleService] Getting schedules for business:', businessId);
    return this.http.get<EmployeeSchedule[]>(url);
  }

  /**
   * Get schedule for specific employee in a business
   * GET /api/employeeschedule/employee/{userId}/business/{businessId}
   */
  getScheduleByEmployee(userId: string, businessId: string): Observable<EmployeeSchedule> {
    const url = `${this.baseUrl}/employee/${userId}/business/${businessId}`;
    console.log('[EmployeeScheduleService] Getting schedule for employee:', { userId, businessId });
    return this.http.get<EmployeeSchedule>(url);
  }

  /**
   * Update an existing employee schedule
   * PUT /api/employeeschedule
   */
  updateSchedule(schedule: UpdateEmployeeSchedule): Observable<EmployeeSchedule> {
    console.log('[EmployeeScheduleService] Updating schedule:', schedule);
    return this.http.put<EmployeeSchedule>(this.baseUrl, schedule);
  }

  /**
   * Delete an employee schedule
   * DELETE /api/employeeschedule/{id}
   */
  deleteSchedule(id: string): Observable<void> {
    const url = `${this.baseUrl}/${id}`;
    console.log('[EmployeeScheduleService] Deleting schedule:', id);
    return this.http.delete<void>(url);
  }

  /**
   * Get available time slots for booking
   * GET /api/employeeschedule/available-slots
   */
  getAvailableSlots(
    employeeId: string,
    businessId: string,
    date: string,
    serviceDurationMinutes: number
  ): Observable<AvailableTimeSlots> {
    const url = `${this.baseUrl}/available-slots`;
    const params = new HttpParams()
      .set('employeeId', employeeId)
      .set('businessId', businessId)
      .set('date', date)
      .set('serviceDurationMinutes', serviceDurationMinutes.toString());

    console.log('[EmployeeScheduleService] Getting available slots:', {
      employeeId,
      businessId,
      date,
      serviceDurationMinutes
    });
    return this.http.get<AvailableTimeSlots>(url, { params });
  }
}
