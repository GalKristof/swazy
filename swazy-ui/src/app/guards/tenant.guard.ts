import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const tenantGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  const user = authService.getCurrentUser();
  // Allow access if user has a tenant role (not SuperAdmin)
  if (user && user.systemRole !== 'SuperAdmin') {
    return true;
  }

  router.navigate(['/']);
  return false;
};
