import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-notifications-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="notifications-container">
      <div *ngFor="let notification of notifications" 
           class="notification-item"
           [ngClass]="{
             'success': notification.type === 'success',
             'error': notification.type === 'error',
             'warning': notification.type === 'warning',
             'info': notification.type === 'info',
             'welcome': notification.type === 'welcome'
           }"
           (click)="onNotificationClick(notification)">
        <div class="notification-header">
          <span class="notification-title">{{ notification.title }}</span>
          <button class="close-btn" (click)="removeNotification(notification.id, $event)">&times;</button>
        </div>
        <div class="notification-message">{{ notification.message }}</div>
      </div>
    </div>
  `,
  styles: [`
    .notifications-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 1000;
      max-width: 400px;
    }

    .notification-item {
      background: white;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      padding: 16px;
      margin-bottom: 10px;
      cursor: pointer;
      transition: transform 0.2s ease-in-out;
      border-left: 4px solid transparent;
    }

    .notification-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 8px;
    }

    .notification-title {
      font-weight: bold;
      color: #333;
    }

    .notification-message {
      color: #666;
      font-size: 14px;
    }

    .close-btn {
      background: none;
      border: none;
      color: #999;
      font-size: 20px;
      cursor: pointer;
      padding: 4px 8px;
      border-radius: 4px;
    }

    .close-btn:hover {
      background: rgba(0, 0, 0, 0.05);
    }

    .notification-item:hover {
      transform: translateY(-2px);
    }

    .notification-item.info {
      border-left-color: #3b82f6;
    }

    .notification-item.success {
      border-left-color: #10b981;
    }

    .notification-item.warning {
      border-left-color: #f59e0b;
    }

    .notification-item.error {
      border-left-color: #ef4444;
    }

    .notification-item.welcome {
      border-left-color: #8b5cf6;
    }
  `]
})
export class NotificationsContainerComponent implements OnInit, OnDestroy {
  notifications: any[] = [];
  private subscription: Subscription | undefined;

  constructor(
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.subscription = this.notificationService.notifications.subscribe(
      notifications => this.notifications = notifications
    );
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  removeNotification(id: string, event: Event) {
    event.stopPropagation();
    this.notificationService.remove(id);
  }

  onNotificationClick(notification: any) {
    if (notification.action?.type === 'navigate') {
      if (notification.action.data?.taskId) {
        // Navegar a la tarea
        this.router.navigate(['/tasks', notification.action.data.taskId]);
      }
      this.notificationService.remove(notification.id);
    }
  }
}