import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable()
export class ApiInterceptor implements HttpInterceptor {
  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // Only prepend base URL for API calls (not for SignalR or other requests)
    if (req.url.startsWith('/api/')) {
      const apiBaseUrl = environment.apiUrl.replace(/\/api\/?$/, ''); // Get base URL without /api
      const newUrl = `${apiBaseUrl}${req.url}`;
      req = req.clone({ url: newUrl });
    }

    return next.handle(req);
  }
}
