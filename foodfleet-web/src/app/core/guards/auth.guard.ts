import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Route guard that allows navigation only when the user is authenticated.
 * Redirects unauthenticated users to `/auth/login`.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn()) return true;
  return router.createUrlTree(['/auth/login']);
};

/**
 * Factory that returns a route guard allowing navigation only when the user
 * is authenticated and has one of the specified roles.
 * Redirects unauthorised users to the root route.
 * @param roles - The list of roles permitted to access the route.
 */
export const roleGuard = (roles: string[]): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isLoggedIn() && roles.includes(auth.role!)) return true;
  return router.createUrlTree(['/']);
};
