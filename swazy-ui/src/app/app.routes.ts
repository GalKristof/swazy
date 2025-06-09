import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'services',
    loadComponent: () => import('./service-list/service-list.component').then(m => m.ServiceListComponent),
    data: { title: 'Services Management' } // Optional: for setting page title or breadcrumbs
  },
  {
    path: 'businesses',
    loadComponent: () => import('./businesses/businesses.component').then(m => m.BusinessesComponent),
    data: { title: 'Business Management' }
  },
  {
    path: 'business-services',
    loadComponent: () => import('./business-services/business-services.component').then(m => m.BusinessServicesComponent),
    data: { title: 'Business Services Management' }
  },
  {
    path: 'booking',
    loadComponent: () => import('./booking/booking.component').then(m => m.BookingComponent),
    data: { title: 'Booking' }
  },
  {
    path: 'scheduler',
    loadComponent: () => import('./bookings/bookings.component').then(m => m.BookingsComponent),
    data: { title: 'Scheduler' }
  },
  {
    path: '',
    redirectTo: '/services',
    pathMatch: 'full'
  },
  {
    path: '**', // Wildcard route for a 404 or redirect
    redirectTo: '/services' // Or a dedicated NotFoundComponent if available
    // loadComponent: () => import('./not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];
