import { Component, OnInit } from '@angular/core';
import { CollaborationService } from '../../../../core/services/collaboration.service';
import { BoardInvitation, RespondToInvitationRequest } from '@core/models/entities';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-pending-invitations',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pending-invitations.component.html',
  styleUrls: ['./pending-invitations.component.scss']
})
export class PendingInvitationsComponent implements OnInit {
  invitations: BoardInvitation[] = [];

  constructor(private collaborationService: CollaborationService) {}

  ngOnInit(): void {
    this.loadPendingInvitations();
  }

  private loadPendingInvitations(): void {
    this.collaborationService.getPendingInvitations().subscribe({
      next: (invitations: BoardInvitation[]) => {
        this.invitations = invitations;
      },
      error: (error: Error) => {
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
      error: (error: Error) => {
        console.error('Error responding to invitation:', error);
      }
    });
  }
}