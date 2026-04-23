import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const loginGuard: CanActivateFn = (route, state) => {

  const auth = inject(AuthService);
  const router = inject(Router);

  // If user is already logged in, redirect to admin or employee based on their role
  if (auth.isAuthenticated()) {
    try {
      const role = auth.getUserRole();
      if (role === 'Admin') {
        router.navigate(['/admin']);
      } else if (role === 'Employee') {
        router.navigate(['/employee']);
      }
      return false;
    } catch {
      // Token is invalid, allow access to login
      return true;
    }
  }

  // No token, allow access to login
  return true;
};
