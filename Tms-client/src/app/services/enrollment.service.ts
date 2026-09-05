import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, timer, throwError } from 'rxjs';
import { mergeMap, retryWhen, scan } from 'rxjs/operators';
import { Enrollment } from '../models/enrollment.model';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class EnrollmentService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/enrollments`;

  getAll(): Observable<Enrollment[]> {
    return this.http.get<Enrollment[]>(this.baseUrl).pipe(
      retryRateLimit()
    );
  }

  approve(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/approve`, {});
  }
}

const retryRateLimit = <T>() => (source: Observable<T>): Observable<T> =>
  source.pipe(
    retryWhen(errors =>
      errors.pipe(
        scan((attempt, error: HttpErrorResponse) => {
          if (error.status !== 429 || attempt >= 2) {
            throw error;
          }
          return attempt + 1;
        }, 0),
        mergeMap(() => timer(10_000))
      )
    )
  );