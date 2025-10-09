import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const loginGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    const user = authService.getCurrentUser();
    if (user?.systemRole === 'SuperAdmin') {
      router.navigate(['/admin']);
    } else {
      router.navigate(['/business-management']);
    }
    return false;
  }

  return true;
};
