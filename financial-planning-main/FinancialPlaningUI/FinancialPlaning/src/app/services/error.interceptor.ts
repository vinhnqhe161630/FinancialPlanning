import { HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error) => {
      console.log(error.status);

      if ([404].includes(error.status)) {
        window.location.href = '/404';
      }

      if ([403].includes(error.status)) {
        console.log('Unauthrized request ');
        window.location.href = '/login';
      }

      if ([401].includes(error.status)) {
        localStorage.removeItem('token');
        window.location.href = '/login';
      }
      return throwError(() => error);
    })
  );
};
