import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info' | 'welcome';
  title: string;
  message: string;
  duration?: number;
  timestamp: Date;
  position?: 'top-right' | 'center';
  action?: {
    type: 'navigate';
    data: any;
  };
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications$ = new BehaviorSubject<Notification[]>([]);

  constructor() { }

  get notifications(): Observable<Notification[]> {
    return this.notifications$.asObservable();
  }

  show(notification: Omit<Notification, 'id' | 'timestamp'>): void {
    const newNotification: Notification = {
      ...notification,
      id: this.generateId(),
      timestamp: new Date(),
      duration: notification.duration ?? (notification.type === 'welcome' ? 5000 : 5000) // 5 seconds for welcome notifications
    };

    const currentNotifications = this.notifications$.value;
    this.notifications$.next([...currentNotifications, newNotification]);

    // Auto-remove notification after duration
    if (newNotification.duration && newNotification.duration > 0) {
      console.log('Setting timeout for welcome notification:', newNotification.duration, 'ms');
      setTimeout(() => {
        console.log('Auto-removing welcome notification:', newNotification.id);
        this.remove(newNotification.id);
      }, newNotification.duration);
    }
  }

  success(title: string, message: string, duration?: number): void {
    this.show({ type: 'success', title, message, duration });
  }

  error(title: string, message: string, duration?: number): void {
    this.show({ type: 'error', title, message, duration });
  }

  warning(title: string, message: string, duration?: number): void {
    this.show({ type: 'warning', title, message, duration });
  }

  info(title: string, message: string, duration?: number, action?: { type: 'navigate'; data: any }): void {
    this.show({ type: 'info', title, message, duration, action });
  }

  welcome(title: string, message: string, duration?: number): void {
    this.show({ type: 'welcome', title, message, duration, position: 'center' });
  }

  remove(id: string): void {
    const currentNotifications = this.notifications$.value;
    this.notifications$.next(currentNotifications.filter(n => n.id !== id));
  }

  clear(): void {
    this.notifications$.next([]);
  }

  private generateId(): string {
    return Math.random().toString(36).substring(2) + Date.now().toString(36);
  }
}



