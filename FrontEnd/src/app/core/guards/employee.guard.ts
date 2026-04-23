import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { jwtDecode } from 'jwt-decode';

export const employeeGuard: CanActivateFn = (route, state) => {

  const auth = inject(AuthService);
  const router = inject(Router);

  const token = auth.getToken();
  if (!token || !auth.isAuthenticated()) {
    router.navigate(['/login']);
    return false;
  }

  const decoded: any = jwtDecode(token);
  const role = decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"];

  if (role === "Employee") {
    return true;
  }

  router.navigate(['/login']);
  return false;
};