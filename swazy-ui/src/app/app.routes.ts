import { Routes } from '@angular/router';
import {BarberLandingComponent} from './barber-landing/barber-landing';
import {BusinessManagementComponent} from './business-management/business-management';
import {BookingFlowComponent} from './booking-flow/booking-flow';
import {BookingConfirmation} from './booking-confirmation/booking-confirmation';

export const routes: Routes = [
  {
    path: 'manage',
    component: BusinessManagementComponent
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
