import { Service } from '@angular/core';

@Service()
export class SignalrService {}
import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection!: signalR.HubConnection;

  // Signals for real-time updates
  public enrollmentStatusUpdated = signal<{ enrollmentId: string; status: string } | null>(null);

  public startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('/api/hub/tms', {
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connection started successfully'))
      .catch(err => console.error('Error while starting SignalR connection: ', err));

    // Listen to the backend hub event
    this.hubConnection.on('ReceiveEnrollmentStatusUpdated', (enrollmentId: string, status: string) => {
      this.enrollmentStatusUpdated.set({ enrollmentId, status });
    });
  }
}