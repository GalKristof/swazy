import { Routes } from '@angular/router';
import { BarberLandingComponent } from './barber-landing/barber-landing';
import { BusinessManagementComponent } from './business-management/business-management';
import { BookingFlowComponent } from './booking-flow/booking-flow';
import { BookingConfirmation } from './booking-confirmation/booking-confirmation';
import { LoginComponent } from './auth/login/login.component';
import { SetupPasswordComponent } from './auth/setup-password/setup-password.component';
import { AdminManagementComponent } from './admin-management/admin-management.component';
import { BusinessListComponent } from './admin-management/business-list/business-list.component';
import { BusinessDetailsComponent } from './admin-management/business-details/business-details.component';
import { UserListComponent } from './admin-management/user-list/user-list.component';
import { ServiceListComponent } from './admin-management/service-list/service-list.component';
import { authGuard } from './guards/auth.guard';
import { loginGuard } from './guards/login.guard';
import { adminGuard } from './guards/admin.guard';
import { tenantGuard } from './guards/tenant.guard';

export const routes: Routes = [
  // Login route
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [loginGuard]
  },
  // Setup password from invitation
  {
    path: 'setup/:token',
    component: SetupPasswordComponent
  },
  // Admin panel
  {
    path: 'admin',
    component: AdminManagementComponent,
    canActivate: [adminGuard],
    children: [
      {
        path: 'businesses',
        component: BusinessListComponent
      },
      {
        path: 'businesses/:id',
        component: BusinessDetailsComponent
      },
      {
        path: 'users',
        component: UserListComponent
      },
      {
        path: 'services',
        component: ServiceListComponent
      },
      {
        path: '',
        redirectTo: 'businesses',
        pathMatch: 'full'
      }
    ]
  },
  // Business owner management
  {
    path: 'manage',
    component: BusinessManagementComponent,
    canActivate: [tenantGuard, authGuard]
  },
  // Booking flow
  {
    path: 'book',
    component: BookingFlowComponent
  },
  // Booking confirmation
  {
    path: 'confirmation/:code',
    component: BookingConfirmation
  },
  // Landing page
  {
    path: '',
    component: BarberLandingComponent
  },
  // Catch all
  {
    path: '**',
    redirectTo: ''
  }
];
