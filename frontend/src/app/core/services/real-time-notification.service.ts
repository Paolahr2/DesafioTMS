import { Injectable } from '@angular/core';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { NotificationService } from './notification.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class RealTimeNotificationService {
  private hubConnection!: HubConnection;
  private connectionEstablished = new BehaviorSubject<boolean>(false);

  constructor(
    private notificationService: NotificationService,
    private authService: AuthService
  ) {
    this.initializeConnection();
  }

  private initializeConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:8080/hubs/notifications')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('Conexión establecida con el hub de notificaciones');
        this.connectionEstablished.next(true);
        this.registerOnServerEvents();
      })
      .catch(err => console.error('Error al conectar con el hub de notificaciones:', err));
  }

  private registerOnServerEvents() {
    // Escuchar notificaciones de tareas asignadas
    this.hubConnection.on('ReceiveTaskAssignment', (notification: { taskId: string, taskTitle: string }) => {
      this.notificationService.info(
        'Nueva tarea asignada',
        `Se te ha asignado la tarea: ${notification.taskTitle}`,
        0 // duración indefinida para notificaciones de tareas
      );
    });

    // Escuchar notificaciones de fechas límite próximas
    this.hubConnection.on('ReceiveDueDateReminder', (notification: { taskId: string, taskTitle: string, dueDate: string }) => {
      this.notificationService.warning(
        'Fecha límite próxima',
        `La tarea "${notification.taskTitle}" vence el ${notification.dueDate}`,
        0 // duración indefinida para notificaciones de fecha límite
      );
    });
  }

  public isConnected(): Observable<boolean> {
    return this.connectionEstablished.asObservable();
  }

  public disconnect() {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => {
          console.log('Desconectado del hub de notificaciones');
          this.connectionEstablished.next(false);
        })
        .catch(err => console.error('Error al desconectar del hub de notificaciones:', err));
    }
  }
}