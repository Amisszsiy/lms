import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Runs after authGuard on the parent route, so the current user is already loaded.
export const adminGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAdmin$()) {
    return true;
  }
  return router.createUrlTree(['/dashboard']);
};
