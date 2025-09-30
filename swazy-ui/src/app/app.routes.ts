import { Routes } from '@angular/router';
import {BarberLandingComponent} from './barber-landing/barber-landing';
import {BusinessManagementComponent} from './business-management/business-management';

export const routes: Routes = [
  {
    path: 'manage',
    component: BusinessManagementComponent
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
