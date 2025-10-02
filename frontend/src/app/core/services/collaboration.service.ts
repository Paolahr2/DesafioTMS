import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  BoardInvitation,
  BoardMember,
  InviteUserToBoardRequest,
  RespondToInvitationRequest
} from '../models/entities';

@Injectable({
  providedIn: 'root'
})
export class CollaborationService {
  private apiUrl = 'http://localhost:8080/api';

  constructor(private http: HttpClient) {}

  // Invitations
  inviteUserToBoard(boardId: string, request: InviteUserToBoardRequest): Observable<BoardInvitation> {
    return this.http.post<BoardInvitation>(`${this.apiUrl}/boards/${boardId}/invite`, request);
  }

  respondToInvitation(invitationId: string, request: RespondToInvitationRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/boards/invitations/${invitationId}/respond`, request);
  }

  getPendingInvitations(): Observable<BoardInvitation[]> {
    return this.http.get<BoardInvitation[]>(`${this.apiUrl}/boards/invitations/pending`);
  }

  // Board Members
  getBoardMembers(boardId: string): Observable<BoardMember[]> {
    return this.http.get<BoardMember[]>(`${this.apiUrl}/boards/${boardId}/members`);
  }

  removeBoardMember(boardId: string, memberId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/boards/${boardId}/members/${memberId}`);
  }
}



