import { Component, OnInit } from '@angular/core';
import { CollaborationService } from '../../../core/services/collaboration.service';
import { BoardInvitation, RespondToInvitationRequest } from '@core/models/entities';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pending-invitations',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="pending-invitations">
      <h3>Invitaciones Pendientes</h3>
      <div *ngIf="invitations.length === 0" class="no-invitations">
        No tienes invitaciones pendientes
      </div>
      <div *ngFor="let invitation of invitations" class="invitation-card">
        <div class="invitation-info">
          <h4>{{ invitation.boardTitle || 'Tablero' }}</h4>
          <p>Invitado por: {{ invitation.invitedByName || 'Usuario' }}</p>
          <p class="invitation-date">Enviado: {{ invitation.createdAt | date:'short' }}</p>
        </div>
        <div class="invitation-actions">
          <button (click)="respondToInvitation(invitation.id, true)" class="accept-btn">
            Aceptar
          </button>
          <button (click)="respondToInvitation(invitation.id, false)" class="reject-btn">
            Rechazar
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .pending-invitations {
      padding: 20px;
    }
    .no-invitations {
      text-align: center;
      color: #666;
      font-style: italic;
    }
    .invitation-card {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 16px;
      margin: 8px 0;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .invitation-info h4 {
      margin: 0 0 8px 0;
    }
    .invitation-info p {
      margin: 4px 0;
      color: #666;
    }
    .invitation-actions {
      display: flex;
      gap: 8px;
    }
    .accept-btn {
      background: #4caf50;
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 4px;
      cursor: pointer;
    }
    .reject-btn {
      background: #f44336;
      color: white;
      border: none;
      padding: 8px 16px;
      border-radius: 4px;
      cursor: pointer;
    }
  `]
})
export class PendingInvitationsComponent implements OnInit {
  invitations: BoardInvitation[] = [];

  constructor(private collaborationService: CollaborationService) {}

  ngOnInit(): void {
    this.loadPendingInvitations();
  }

  private loadPendingInvitations(): void {
    this.collaborationService.getPendingInvitations().subscribe({
      next: (invitations) => {
        this.invitations = invitations;
      },
      error: (error) => {
        console.error('Error loading pending invitations:', error);
      }
    });
  }

  respondToInvitation(invitationId: string, accept: boolean): void {
    const request: RespondToInvitationRequest = { accept };
    this.collaborationService.respondToInvitation(invitationId, request).subscribe({
      next: () => {
        // Remove the invitation from the list
        this.invitations = this.invitations.filter(inv => inv.id !== invitationId);
      },
      error: (error) => {
        console.error('Error responding to invitation:', error);
      }
    });
  }
}


