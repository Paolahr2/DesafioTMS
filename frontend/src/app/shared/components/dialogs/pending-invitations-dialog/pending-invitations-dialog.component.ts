import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { CollaborationService } from '../../../../core/services/collaboration.service';
import { BoardInvitation, RespondToInvitationRequest } from '@core/models/entities';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-pending-invitations-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './pending-invitations-dialog.component.html',
  styleUrls: ['./pending-invitations-dialog.component.scss']
})
export class PendingInvitationsDialogComponent implements OnInit {
  invitations: BoardInvitation[] = [];
  processingInvitation: string | null = null;

  constructor(
    private dialogRef: MatDialogRef<PendingInvitationsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) private data: any,
    private collaborationService: CollaborationService,
    private snackBar: MatSnackBar
  ) {}

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
        this.snackBar.open('Error al cargar las invitaciones', 'Cerrar', { duration: 3000 });
      }
    });
  }

  respondToInvitation(invitation: BoardInvitation, accept: boolean): void {
    if (this.processingInvitation) return;

    this.processingInvitation = invitation.id;
    const request: RespondToInvitationRequest = { accept };

    this.collaborationService.respondToInvitation(invitation.id, request).subscribe({
      next: () => {
        // Remove the invitation from the list
        this.invitations = this.invitations.filter(inv => inv.id !== invitation.id);
        this.processingInvitation = null;

        const boardName = invitation.boardTitle || 'el tablero';
        const message = accept
          ? `✅ Has aceptado colaborar en "${boardName}"`
          : `❌ Has rechazado la invitación a "${boardName}"`;

        this.snackBar.open(message, 'Cerrar', { 
          duration: 4000,
          panelClass: accept ? ['success-snackbar'] : ['info-snackbar']
        });

        // Notify the inviter (both accept and reject)
        if (invitation.invitedById) {
          this.notifyInviter(invitation, accept);
        }

        // If no more invitations, close the dialog
        if (this.invitations.length === 0) {
          setTimeout(() => this.close(), 1500);
        }
      },
      error: (error) => {
        console.error('Error responding to invitation:', error);
        this.processingInvitation = null;
        this.snackBar.open('Error al procesar la invitación', 'Cerrar', { duration: 3000 });
      }
    });
  }

  private notifyInviter(invitation: BoardInvitation, accepted: boolean): void {
    // The backend handles sending the notification to the inviter
    // Log for debugging purposes
    const action = accepted ? 'aceptó' : 'rechazó';
    const inviterName = invitation.invitedByName || 'el invitador';
    console.log(`Se notificará a ${inviterName} que ${action} la invitación al tablero "${invitation.boardTitle}"`);
  }

  close(): void {
    this.dialogRef.close();
  }
}