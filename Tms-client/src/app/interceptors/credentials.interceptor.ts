import { HttpInterceptorFn } from '@angular/common/http';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  // Clone the request and set withCredentials to true to send cookies/credentials
  const clonedReq = req.clone({
    withCredentials: true,
  });

  return next(clonedReq);
};
