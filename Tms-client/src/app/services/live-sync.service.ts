import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
} from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface EnrollmentStatusEvent {
  id: string;
  status: 'Pending' | 'Approved' | 'Rejected';
}

@Injectable({
  providedIn: 'root'
})
export class LiveSyncService {
  private platformId = inject(PLATFORM_ID);
  private connection: HubConnection | null = null;
  private startPromise: Promise<void> | null = null;
  private retryTimer: ReturnType<typeof setTimeout> | null = null;
  private eventsSubject = new Subject<EnrollmentStatusEvent>();

  events$ = this.eventsSubject.asObservable();
  connectionState = signal<'connected' | 'reconnecting' | 'disconnected'>('disconnected');

  connect(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    if (this.startPromise || this.retryTimer) return;
    if (this.connection) return;

    this.connection = new HubConnectionBuilder()
      .withUrl(environment.signalRUrl, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    this.connection.on(
      'ReceiveEnrollmentStatusUpdated',
      (enrollmentId: string, status: 'Pending' | 'Approved' | 'Rejected') => {
        this.eventsSubject.next({ id: enrollmentId, status });
      }
    );

    this.connection.onreconnecting(() => this.connectionState.set('reconnecting'));
    this.connection.onreconnected(() => this.connectionState.set('connected'));
    this.connection.onclose(() => this.connectionState.set('disconnected'));

    const connection = this.connection;
    this.startPromise = connection
      .start()
      .then(() => this.connectionState.set('connected'))
      .catch(err => {
        if (!this.isRateLimitError(err)) {
          console.error('SignalR connection error:', err);
        }
        this.connectionState.set('disconnected');
        this.connection = null;
        this.retryTimer = setTimeout(() => {
          this.retryTimer = null;
          this.connect();
        }, 10_000);
      })
      .finally(() => {
        this.startPromise = null;
      });
  }

  private isRateLimitError(error: unknown): boolean {
    return error instanceof Error && /status code ['"]?429\b/i.test(error.message);
  }
}