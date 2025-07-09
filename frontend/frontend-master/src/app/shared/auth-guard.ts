import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigateByUrl('/login');
    return false;
  }

  const role = authService.getUserRoles();
  const requiredRole = route.data?.['requiredRole'];

  if (requiredRole && role !== requiredRole) {
    router.navigateByUrl('/forbidden');
    return false;
  }

  return true;
};
