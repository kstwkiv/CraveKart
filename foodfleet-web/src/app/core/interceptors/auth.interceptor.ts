import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token;

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && auth.isLoggedIn()) {
        const refresh$ = auth.refreshToken();
        if (refresh$) {
          return refresh$.pipe(
            switchMap(() => {
              const retried = req.clone({ setHeaders: { Authorization: `Bearer ${auth.token}` } });
              return next(retried);
            }),
            catchError(() => {
              auth.logout();
              return throwError(() => err);
            })
          );
        }
        auth.logout();
      }
      return throwError(() => err);
    })
  );
};
