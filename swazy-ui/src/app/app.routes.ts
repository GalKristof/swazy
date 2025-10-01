import { Routes } from '@angular/router';
import {BarberLandingComponent} from './barber-landing/barber-landing';
import {BusinessManagementComponent} from './business-management/business-management';
import {BookingFlowComponent} from './booking-flow/booking-flow';

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
    path: '',
    component: BarberLandingComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
