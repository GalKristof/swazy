import { Routes } from '@angular/router';
import { BarberLandingComponent } from './barber-landing/barber-landing';
import { BusinessManagementComponent } from './business-management/business-management';
import { BookingFlowComponent } from './booking-flow/booking-flow';
import { BookingConfirmation } from './booking-confirmation/booking-confirmation';
import { LoginComponent } from './auth/login/login.component';
import { SetupPasswordComponent } from './auth/setup-password/setup-password.component';
import { authGuard } from './guards/auth.guard';
import { loginGuard } from './guards/login.guard';

export const routes: Routes = [
  {
    path: 'auth/login',
    component: LoginComponent,
    canActivate: [loginGuard]
  },
  {
    path: 'auth/setup-password/:token',
    component: SetupPasswordComponent
  },
  {
    path: 'business-management',
    component: BusinessManagementComponent,
    canActivate: [authGuard]
  },
  {
    path: 'manage',
    redirectTo: 'business-management',
    pathMatch: 'full'
  },
  {
    path: 'booking',
    component: BookingFlowComponent
  },
  {
    path: 'booking-confirmation/:code',
    component: BookingConfirmation
  },
  {
    path: '',
    component: BarberLandingComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
