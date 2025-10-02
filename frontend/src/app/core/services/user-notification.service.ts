import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, interval } from 'rxjs';
import { tap, switchMap, startWith, catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Notification } from '@core/models/entities';

@Injectable({
  providedIn: 'root'
})
export class UserNotificationService {
  private apiUrl = `${environment.apiUrl}/api/notifications`;
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private unreadCountSubject = new BehaviorSubject<number>(0);

  public notifications$ = this.notificationsSubject.asObservable();
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {
    // Polling cada 30 segundos para actualizar notificaciones
    interval(30000)
      .pipe(
        startWith(0),
        switchMap(() => this.loadNotifications())
      )
      .subscribe({
        next: (notifications) => console.log('Notificaciones cargadas automáticamente:', notifications),
        error: (error) => console.error('Error cargando notificaciones automáticamente:', error)
      });
  }

  /**
   * Carga las notificaciones del usuario
   */
  loadNotifications(unreadOnly?: boolean, limit?: number): Observable<Notification[]> {
    let url = this.apiUrl;
    const params: string[] = [];
    
    if (unreadOnly !== undefined) {
      params.push(`unreadOnly=${unreadOnly}`);
    }
    if (limit !== undefined) {
      params.push(`limit=${limit}`);
    }
    
    if (params.length > 0) {
      url += '?' + params.join('&');
    }

    return this.http.get<Notification[]>(url).pipe(
      tap(notifications => {
        console.log('Notificaciones obtenidas de API:', notifications);
        this.notificationsSubject.next(notifications);
        this.updateUnreadCount(notifications);
      }),
      catchError((error: any) => {
        console.error('Error obteniendo notificaciones:', error);
        throw error;
      })
    );
  }

  /**
   * Marca una notificación como leída
   */
  markAsRead(notificationId: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/${notificationId}/mark-as-read`, {}).pipe(
      tap(() => {
        // Actualizar el estado local
        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = currentNotifications.map(n =>
          n.id === notificationId ? { ...n, isRead: true } : n
        );
        this.notificationsSubject.next(updatedNotifications);
        this.updateUnreadCount(updatedNotifications);
      })
    );
  }

  /**
   * Obtiene el conteo de notificaciones no leídas
   */
  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`).pipe(
      tap(response => this.unreadCountSubject.next(response.count))
    );
  }

  /**
   * Actualiza el conteo de notificaciones no leídas
   */
  private updateUnreadCount(notifications: Notification[]): void {
    const unreadCount = notifications.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(unreadCount);
  }

  /**
   * Fuerza una recarga inmediata de las notificaciones
   */
  refreshNotifications(): void {
    this.loadNotifications().subscribe();
  }
}
