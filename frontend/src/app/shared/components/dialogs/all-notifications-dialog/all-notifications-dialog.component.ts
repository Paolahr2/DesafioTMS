import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MatBadgeModule } from '@angular/material/badge';
import { UserNotificationService } from '@core/services/user-notification.service';
import { CollaborationService } from '@core/services/collaboration.service';
import { Notification, BoardInvitation } from '@core/models/entities';
import { Router } from '@angular/router';

@Component({
  selector: 'app-all-notifications-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatTabsModule, MatBadgeModule],
  template: `
    <div class="all-notifications-dialog">
      <div class="dialog-header">
        <h2 mat-dialog-title>
          <i class="fas fa-bell mr-2"></i>
          Notificaciones
        </h2>
        <button class="close-btn" (click)="closeDialog()">
          <i class="fas fa-times"></i>
        </button>
      </div>

      <mat-dialog-content>
        <mat-tab-group>
          <!-- Tab de Notificaciones Generales -->
          <mat-tab>
            <ng-template mat-tab-label>
              <span class="tab-label">
                General
                <span *ngIf="unreadGeneralCount > 0" class="badge">{{unreadGeneralCount}}</span>
              </span>
            </ng-template>
            
            <div class="notifications-list">
              <div *ngIf="notifications.length === 0" class="no-notifications">
                <i class="fas fa-inbox text-gray-400 text-4xl mb-3"></i>
                <p class="text-gray-500">No tienes notificaciones</p>
              </div>

              <div *ngFor="let notification of notifications" 
                   class="notification-item"
                   [class.unread]="!notification.isRead"
                   (click)="handleNotificationClick(notification)">
                <div class="notification-icon" [ngClass]="getNotificationIconClass(notification.type)">
                  <i [class]="getNotificationIcon(notification.type)"></i>
                </div>
                <div class="notification-content">
                  <h4>{{ notification.title }}</h4>
                  <p>{{ notification.message }}</p>
                  <span class="notification-time">{{ getTimeAgo(notification.createdAt) }}</span>
                </div>
                <button *ngIf="!notification.isRead" 
                        class="mark-read-btn"
                        (click)="markAsRead(notification.id, $event)">
                  <i class="fas fa-check"></i>
                </button>
              </div>
            </div>
          </mat-tab>

          <!-- Tab de Invitaciones -->
          <mat-tab>
            <ng-template mat-tab-label>
              <span class="tab-label">
                Invitaciones
                <span *ngIf="invitations.length > 0" class="badge">{{invitations.length}}</span>
              </span>
            </ng-template>
            
            <div class="invitations-list">
              <div *ngIf="invitations.length === 0" class="no-invitations">
                <i class="fas fa-envelope-open text-gray-400 text-4xl mb-3"></i>
                <p class="text-gray-500">No tienes invitaciones pendientes</p>
              </div>

              <div *ngFor="let invitation of invitations" class="invitation-item">
                <div class="invitation-header">
                  <div class="invitation-icon">
                    <i class="fas fa-users"></i>
                  </div>
                  <div class="invitation-info">
                    <h4>{{ invitation.boardTitle }}</h4>
                    <p>Invitado por <strong>{{ invitation.invitedByName }}</strong></p>
                    <span class="invitation-time">{{ getTimeAgo(invitation.createdAt) }}</span>
                  </div>
                </div>
                <div class="invitation-actions">
                  <button class="accept-btn" (click)="acceptInvitation(invitation)">
                    <i class="fas fa-check mr-1"></i> Aceptar
                  </button>
                  <button class="reject-btn" (click)="rejectInvitation(invitation)">
                    <i class="fas fa-times mr-1"></i> Rechazar
                  </button>
                </div>
              </div>
            </div>
          </mat-tab>
        </mat-tab-group>
      </mat-dialog-content>
    </div>
  `,
  styles: [`
    .all-notifications-dialog {
      width: 600px;
      max-height: 80vh;
    }

    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      border-bottom: 1px solid #e5e7eb;
      background: linear-gradient(135deg, #4a5568 0%, #2563eb 100%);
      color: white;
    }

    .dialog-header h2 {
      margin: 0;
      font-size: 1.5rem;
      font-weight: 600;
    }

    .close-btn {
      background: rgba(255, 255, 255, 0.2);
      border: none;
      color: white;
      width: 32px;
      height: 32px;
      border-radius: 50%;
      cursor: pointer;
      transition: all 0.2s;
    }

    .close-btn:hover {
      background: rgba(255, 255, 255, 0.3);
      transform: scale(1.1);
    }

    mat-dialog-content {
      padding: 0;
      overflow: visible;
    }

    .tab-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-weight: 500;
    }

    .badge {
      background: #ef4444;
      color: white;
      border-radius: 12px;
      padding: 2px 8px;
      font-size: 0.75rem;
      font-weight: 600;
    }

    .notifications-list,
    .invitations-list {
      max-height: 400px;
      overflow-y: auto;
      padding: 16px;
    }

    .no-notifications,
    .no-invitations {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 60px 20px;
    }

    .notification-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 16px;
      border-radius: 12px;
      margin-bottom: 8px;
      cursor: pointer;
      transition: all 0.2s;
      background: white;
      border: 1px solid #e5e7eb;
    }

    .notification-item:hover {
      background: #f9fafb;
      transform: translateX(4px);
    }

    .notification-item.unread {
      background: #eff6ff;
      border-color: #3b82f6;
    }

    .notification-icon {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .notification-icon.success {
      background: #d1fae5;
      color: #059669;
    }

    .notification-icon.error {
      background: #fee2e2;
      color: #dc2626;
    }

    .notification-icon.info {
      background: #dbeafe;
      color: #2563eb;
    }

    .notification-content {
      flex: 1;
    }

    .notification-content h4 {
      margin: 0 0 4px 0;
      font-size: 0.95rem;
      font-weight: 600;
      color: #1f2937;
    }

    .notification-content p {
      margin: 0 0 8px 0;
      font-size: 0.875rem;
      color: #6b7280;
      line-height: 1.4;
    }

    .notification-time {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .mark-read-btn {
      background: #3b82f6;
      color: white;
      border: none;
      width: 28px;
      height: 28px;
      border-radius: 50%;
      cursor: pointer;
      transition: all 0.2s;
      flex-shrink: 0;
    }

    .mark-read-btn:hover {
      background: #2563eb;
      transform: scale(1.1);
    }

    .invitation-item {
      background: white;
      border: 1px solid #e5e7eb;
      border-radius: 12px;
      padding: 16px;
      margin-bottom: 12px;
    }

    .invitation-header {
      display: flex;
      gap: 12px;
      margin-bottom: 12px;
    }

    .invitation-icon {
      width: 48px;
      height: 48px;
      border-radius: 12px;
      background: linear-gradient(135deg, #4a5568 0%, #2563eb 100%);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.25rem;
      flex-shrink: 0;
    }

    .invitation-info {
      flex: 1;
    }

    .invitation-info h4 {
      margin: 0 0 4px 0;
      font-size: 1rem;
      font-weight: 600;
      color: #1f2937;
    }

    .invitation-info p {
      margin: 0 0 4px 0;
      font-size: 0.875rem;
      color: #6b7280;
    }

    .invitation-time {
      font-size: 0.75rem;
      color: #9ca3af;
    }

    .invitation-actions {
      display: flex;
      gap: 8px;
    }

    .accept-btn,
    .reject-btn {
      flex: 1;
      padding: 10px 16px;
      border: none;
      border-radius: 8px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
      font-size: 0.875rem;
    }

    .accept-btn {
      background: #10b981;
      color: white;
    }

    .accept-btn:hover {
      background: #059669;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
    }

    .reject-btn {
      background: #ef4444;
      color: white;
    }

    .reject-btn:hover {
      background: #dc2626;
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
    }

    ::ng-deep .mat-mdc-tab-body-content {
      overflow: visible !important;
    }

    ::ng-deep .mat-mdc-tab-header {
      background: #f9fafb;
      border-bottom: 1px solid #e5e7eb;
    }
  `]
})
export class AllNotificationsDialogComponent implements OnInit {
  notifications: Notification[] = [];
  invitations: BoardInvitation[] = [];
  unreadGeneralCount = 0;

  constructor(
    private dialogRef: MatDialogRef<AllNotificationsDialogComponent>,
    private userNotificationService: UserNotificationService,
    private collaborationService: CollaborationService,
    private router: Router,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
    this.loadInvitations();
  }

  loadNotifications(): void {
    this.userNotificationService.loadNotifications().subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.unreadGeneralCount = notifications.filter(n => !n.isRead).length;
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
      }
    });
  }

  loadInvitations(): void {
    this.collaborationService.getPendingInvitations().subscribe({
      next: (invitations) => {
        this.invitations = invitations;
      },
      error: (error) => {
        console.error('Error loading invitations:', error);
      }
    });
  }

  getNotificationIcon(type: string): string {
    const icons: { [key: string]: string } = {
      'TaskAssigned': 'fas fa-tasks',
      'TaskCompleted': 'fas fa-check-circle',
      'TaskDueDate': 'fas fa-clock',
      'BoardInvitation': 'fas fa-envelope',
      'InvitationAccepted': 'fas fa-user-check',
      'InvitationRejected': 'fas fa-user-times',
      'SystemMessage': 'fas fa-info-circle'
    };
    return icons[type] || 'fas fa-bell';
  }

  getNotificationIconClass(type: string): string {
    if (type === 'InvitationAccepted' || type === 'TaskCompleted') return 'success';
    if (type === 'InvitationRejected') return 'error';
    return 'info';
  }

  getTimeAgo(date: Date): string {
    const now = new Date();
    const notificationDate = new Date(date);
    const diffMs = now.getTime() - notificationDate.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Ahora';
    if (diffMins < 60) return `Hace ${diffMins} min`;
    if (diffHours < 24) return `Hace ${diffHours}h`;
    if (diffDays < 7) return `Hace ${diffDays}d`;
    return notificationDate.toLocaleDateString('es-ES');
  }

  handleNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.markAsRead(notification.id);
    }

    // Navegar según el tipo de notificación
    if (notification.data && notification.data['BoardId']) {
      this.closeDialog();
      this.router.navigate(['/boards', notification.data['BoardId']]);
    }
  }

  markAsRead(notificationId: string, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    
    this.userNotificationService.markAsRead(notificationId).subscribe({
      next: () => {
        const notification = this.notifications.find(n => n.id === notificationId);
        if (notification) {
          notification.isRead = true;
          this.unreadGeneralCount = this.notifications.filter(n => !n.isRead).length;
        }
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  acceptInvitation(invitation: BoardInvitation): void {
    this.collaborationService.respondToInvitation(invitation.id, { accept: true }).subscribe({
      next: () => {
        this.invitations = this.invitations.filter(i => i.id !== invitation.id);
        // Recargar notificaciones para ver si hay nuevas
        this.loadNotifications();
      },
      error: (error) => {
        console.error('Error accepting invitation:', error);
      }
    });
  }

  rejectInvitation(invitation: BoardInvitation): void {
    this.collaborationService.respondToInvitation(invitation.id, { accept: false }).subscribe({
      next: () => {
        this.invitations = this.invitations.filter(i => i.id !== invitation.id);
      },
      error: (error) => {
        console.error('Error rejecting invitation:', error);
      }
    });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }
}
