import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '', // New default route
    loadComponent: () => import('./barbershop-landing/barbershop-landing.component').then(m => m.BarbershopLandingComponent),
    data: { title: 'Welcome to Swazy Barbershop' } // Optional: Page title
  },
  {
    path: 'services',
    loadComponent: () => import('./service-list/service-list.component').then(m => m.ServiceListComponent),
    data: { title: 'Services Management' }
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
    path: 'bookings',
    loadComponent: () => import('./bookings/bookings.component').then(m => m.BookingsComponent),
    data: { title: 'Bookings' }
  },
  {
    path: 'scheduler',
    loadComponent: () => import('./employee-scheduler/employee-scheduler.component').then(m => m.EmployeeSchedulerComponent),
    data: { title: 'Scheduler' }
  },
  {
    path: '**', // Wildcard route for a 404 or redirect
    redirectTo: '' // Redirect wildcard to the new default route (landing page)
    // loadComponent: () => import('./not-found/not-found.component').then(m => m.NotFoundComponent) // Or a dedicated NotFoundComponent
  }
];
