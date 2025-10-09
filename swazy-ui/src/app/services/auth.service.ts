import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, of, throwError } from 'rxjs';
import {
  AuthResponse,
  UserInfo,
  LoginRequest,
  RefreshTokenRequest,
  SetupPasswordRequest
} from '../models/auth.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private platformId = inject(PLATFORM_ID);
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private readonly STORAGE_ACCESS_TOKEN = 'access_token';
  private readonly STORAGE_REFRESH_TOKEN = 'refresh_token';
  private readonly STORAGE_USER = 'current_user';
  private isBrowser: boolean;

  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);

  currentUser$ = this.currentUserSubject.asObservable();

  constructor() {
    this.isBrowser = isPlatformBrowser(this.platformId);

    // Load user from storage after platform check
    if (this.isBrowser) {
      const user = this.getCurrentUserFromStorage();
      if (user) {
        this.currentUserSubject.next(user);
      }
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    const request: LoginRequest = { email, password };

    return this.http.post<AuthResponse>(`${this.API_URL}/login`, request).pipe(
      tap(response => this.setSession(response))
    );
  }

  setupPassword(invitationToken: string, password: string): Observable<AuthResponse> {
    const request: SetupPasswordRequest = { invitationToken, password };

    return this.http.post<AuthResponse>(`${this.API_URL}/setup-password`, request).pipe(
      tap(response => this.setSession(response))
    );
  }

  refreshToken(): Observable<AuthResponse | null> {
    const refreshToken = this.getRefreshToken();

    if (!refreshToken) {
      return of(null);
    }

    const request: RefreshTokenRequest = { refreshToken };

    return this.http.post<AuthResponse>(`${this.API_URL}/refresh`, request).pipe(
      tap(response => this.setSession(response)),
      catchError(() => {
        this.logout();
        return of(null);
      })
    );
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();

    if (refreshToken) {
      this.http.post(`${this.API_URL}/logout`, { refreshToken }).subscribe();
    }

    this.clearSession();
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    if (!token) {
      return false;
    }

    return !this.isTokenExpired(token);
  }

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.STORAGE_ACCESS_TOKEN);
  }

  getRefreshToken(): string | null {
    if (!this.isBrowser) return null;
    return localStorage.getItem(this.STORAGE_REFRESH_TOKEN);
  }

  getCurrentUser(): UserInfo | null {
    return this.currentUserSubject.value;
  }

  private setSession(authResponse: AuthResponse): void {
    if (!this.isBrowser) return;
    localStorage.setItem(this.STORAGE_ACCESS_TOKEN, authResponse.accessToken);
    localStorage.setItem(this.STORAGE_REFRESH_TOKEN, authResponse.refreshToken);

    // Decode token to get business_role claim
    const tokenPayload = this.decodeToken(authResponse.accessToken);
    const businessRole = tokenPayload?.business_role as 'Employee' | 'Manager' | 'Owner' | undefined;

    // Add business role to user info
    const userWithRole = {
      ...authResponse.user,
      businessRole: businessRole
    };

    localStorage.setItem(this.STORAGE_USER, JSON.stringify(userWithRole));
    this.currentUserSubject.next(userWithRole);
  }

  private decodeToken(token: string): any {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch {
      return null;
    }
  }

  private clearSession(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem(this.STORAGE_ACCESS_TOKEN);
    localStorage.removeItem(this.STORAGE_REFRESH_TOKEN);
    localStorage.removeItem(this.STORAGE_USER);
    this.currentUserSubject.next(null);
  }

  private getCurrentUserFromStorage(): UserInfo | null {
    if (!this.isBrowser) return null;
    const userJson = localStorage.getItem(this.STORAGE_USER);
    return userJson ? JSON.parse(userJson) : null;
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000;
      return Date.now() >= exp;
    } catch {
      return true;
    }
  }

  /**
   * Get all users (admin only)
   * Calls GET /api/admin/user/all
   */
  getAllUsers(): Observable<UserInfo[]> {
    const url = `${environment.apiUrl}/admin/user/all`;
    return this.http.get<UserInfo[]>(url);
  }
}
