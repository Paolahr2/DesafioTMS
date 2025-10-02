import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { CollaborationService } from '../../../core/services/collaboration.service';
import { TaskService } from '../../../core/services/task.service';
import { BoardMember, TaskItem } from '@core/models/entities';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage-members-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  template: `
    <div class="create-task-modal">
      <div class="modal-content">
        <div class="modal-header">
          <h2 class="modal-title">Gestionar Miembros</h2>
        </div>

        <div class="task-form">
          <!-- Estadísticas del tablero -->
          <div class="stats-section">
            <div class="stats-grid">
              <div class="stat-card">
                <div class="stat-icon">
                  <svg class="w-6 h-6 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
                  </svg>
                </div>
                <div class="stat-info">
                  <div class="stat-number">{{ members.length }}</div>
                  <div class="stat-label">Miembros</div>
                </div>
              </div>
              <div class="stat-card">
                <div class="stat-icon">
                  <svg class="w-6 h-6 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                  </svg>
                </div>
                <div class="stat-info">
                  <div class="stat-number">{{ getOwnerCount() }}</div>
                  <div class="stat-label">Propietarios</div>
                </div>
              </div>
              <div class="stat-card">
                <div class="stat-icon">
                  <svg class="w-6 h-6 text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                  </svg>
                </div>
                <div class="stat-info">
                  <div class="stat-number">{{ getViewerCount() }}</div>
                  <div class="stat-label">Observadores</div>
                </div>
              </div>
            </div>
          </div>

          <!-- Lista de miembros -->
          <div class="members-section">
            <h4 class="section-title">Miembros del tablero</h4>

            <div class="space-y-3" *ngIf="members.length > 0; else noMembers">
              <div *ngFor="let member of members" class="member-card">
                <div class="member-info">
                  <div class="member-avatar">
                    <span class="avatar-initials">{{ getUserInitials(member) }}</span>
                  </div>
                  <div class="member-details">
                    <div class="member-name">{{ getMemberDisplayName(member) }}</div>
                    <div class="member-role-badge" [ngClass]="getRoleClass(member.role)">
                      {{ getRoleDisplayName(member.role) }}
                    </div>
                    <div class="member-tasks-count">
                      <span class="tasks-icon">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                        </svg>
                      </span>
                      {{ getAssignedTasksCount(member) }} tareas asignadas
                    </div>
                  </div>
                </div>
                <div class="member-actions">
                  <button
                    type="button"
                    class="remove-member-btn"
                    (click)="removeMember(member)"
                    *ngIf="canRemoveMember(member)"
                    title="Eliminar miembro">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path>
                    </svg>
                  </button>
                </div>
              </div>
            </div>

            <ng-template #noMembers>
              <div class="no-members-state">
                <div class="no-members-icon">
                  <svg class="w-16 h-16 text-slate-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path>
                  </svg>
                </div>
                <h3 class="no-members-title">No hay miembros</h3>
                <p class="no-members-text">Invita a colaboradores para trabajar juntos en este tablero.</p>
              </div>
            </ng-template>
          </div>
        </div>

        <div class="modal-footer">
          <button type="button" class="task-cancel-btn" (click)="onClose()">
            Cerrar
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .create-task-modal {
      background: white;
      border-radius: 16px;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
      max-width: 600px;
      width: 100%;
      max-height: 90vh;
      overflow-y: auto;
      animation: modalFadeIn 0.4s ease-out;
    }

    @keyframes modalFadeIn {
      0% {
        opacity: 0;
        transform: scale(0.95) translateY(-10px);
      }
      100% {
        opacity: 1;
        transform: scale(1) translateY(0);
      }
    }

    .modal-content {
      padding: 0;
    }

    .modal-header {
      padding: 24px 24px 0 24px;
      border-bottom: 1px solid #e2e8f0;
      margin-bottom: 24px;
      background: linear-gradient(135deg, rgba(34, 197, 94, 0.05) 0%, rgba(59, 130, 246, 0.05) 100%);
      border-radius: 16px 16px 0 0;
    }

    .modal-title {
      font-size: 24px;
      font-weight: 700;
      background: linear-gradient(135deg, #22c55e 0%, #3b82f6 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      margin: 0;
    }

    .task-form {
      padding: 0 24px;
    }

    .modal-footer {
      padding: 24px;
      border-top: 1px solid #e2e8f0;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      background: #f8fafc;
    }

    .task-cancel-btn {
      background: #f1f5f9;
      color: #64748b;
      border-radius: 8px;
      padding: 10px 20px;
      font-weight: 500;
      transition: all 0.2s ease;
    }

    .task-cancel-btn:hover {
      background: #e2e8f0;
      color: #475569;
    }

    /* Estilos específicos para miembros */
    .stats-section {
      margin-bottom: 24px;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
      gap: 16px;
    }

    .stat-card {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
      border: 1px solid #e2e8f0;
      border-radius: 12px;
    }

    .stat-icon {
      flex-shrink: 0;
    }

    .stat-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .stat-number {
      font-size: 24px;
      font-weight: 700;
      color: #1e293b;
      line-height: 1;
    }

    .stat-label {
      font-size: 12px;
      color: #64748b;
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .members-section {
      margin-bottom: 24px;
    }

    .section-title {
      font-size: 16px;
      font-weight: 600;
      color: #374151;
      margin-bottom: 16px;
    }

    .member-card {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
      background: #fafbfc;
      transition: all 0.2s ease;
    }

    .member-card:hover {
      border-color: #64748b;
      background: #f8fafc;
    }

    .member-info {
      display: flex;
      align-items: center;
      gap: 12px;
      flex: 1;
    }

    .member-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: linear-gradient(135deg, #e2e8f0 0%, #cbd5e1 100%);
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .member-details {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .member-name {
      font-weight: 600;
      font-size: 16px;
      color: #1e293b;
    }

    .member-role-badge {
      display: inline-flex;
      align-items: center;
      padding: 4px 8px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 500;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .member-role-badge.owner {
      background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
      color: #92400e;
    }

    .member-role-badge.member {
      background: linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%);
      color: #1e40af;
    }

    .member-role-badge.viewer {
      background: linear-gradient(135deg, #f3e8ff 0%, #e9d5ff 100%);
      color: #6b21a8;
    }

    .member-tasks-count {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 14px;
      color: #64748b;
      margin-top: 4px;
    }

    .tasks-icon {
      color: #10b981;
    }

    .member-actions {
      display: flex;
      gap: 8px;
    }

    .remove-member-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: 6px;
      background: #fee2e2;
      color: #dc2626;
      border: none;
      cursor: pointer;
      transition: all 0.2s ease;
    }

    .remove-member-btn:hover {
      background: #fecaca;
      color: #b91c1c;
      transform: scale(1.05);
    }

    .no-members-state {
      text-align: center;
      padding: 48px 24px;
    }

    .no-members-icon {
      margin-bottom: 16px;
    }

    .no-members-title {
      font-size: 18px;
      font-weight: 600;
      color: #64748b;
      margin-bottom: 8px;
    }

    .no-members-text {
      font-size: 14px;
      color: #94a3b8;
      line-height: 1.5;
    }
  `]
})
export class ManageMembersDialogComponent implements OnInit {
  members: BoardMember[] = [];
  tasks: TaskItem[] = [];
  loading = false;

  constructor(
    private dialogRef: MatDialogRef<ManageMembersDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { boardId: string },
    private collaborationService: CollaborationService,
    private taskService: TaskService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadMembers();
    this.loadTasks();
  }

  private loadMembers(): void {
    this.loading = true;
    this.collaborationService.getBoardMembers(this.data.boardId).subscribe({
      next: (members) => {
        this.members = members;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading board members:', error);
        this.snackBar.open('Error al cargar los miembros del tablero', 'Cerrar', {
          duration: 3000
        });
        this.loading = false;
      }
    });
  }

  private loadTasks(): void {
    this.taskService.getBoardTasks(this.data.boardId).subscribe({
      next: (tasks) => {
        this.tasks = tasks;
      },
      error: (error) => {
        console.error('Error loading board tasks:', error);
        this.snackBar.open('Error al cargar las tareas del tablero', 'Cerrar', {
          duration: 3000
        });
      }
    });
  }

  canRemoveMember(member: BoardMember): boolean {
    // Only owners can remove members
    return member.role === 'Owner';
  }

  getRoleClass(role: string): string {
    switch (role?.toLowerCase()) {
      case 'owner':
        return 'owner';
      case 'member':
        return 'member';
      case 'viewer':
        return 'viewer';
      default:
        return 'member';
    }
  }

  getRoleDisplayName(role: string): string {
    switch (role?.toLowerCase()) {
      case 'owner':
        return 'Propietario';
      case 'member':
        return 'Miembro';
      case 'viewer':
        return 'Observador';
      default:
        return role || 'Miembro';
    }
  }

  getOwnerCount(): number {
    return this.members.filter(member => member.role === 'Owner').length;
  }

  getViewerCount(): number {
    return this.members.filter(member => member.role === 'Viewer').length;
  }

  getUserInitials(member: BoardMember): string {
    const name = member.fullName || member.userName || member.userEmail || 'U';
    return name.charAt(0).toUpperCase();
  }

  removeMember(member: BoardMember): void {
    const memberName = this.getMemberDisplayName(member);
    if (confirm(`¿Estás seguro de que quieres eliminar a ${memberName} del tablero?`)) {
      this.collaborationService.removeBoardMember(this.data.boardId, member.userId).subscribe({
        next: () => {
          this.snackBar.open('Miembro eliminado exitosamente', 'Cerrar', {
            duration: 3000
          });
          this.loadMembers(); // Reload the list
        },
        error: (error) => {
          console.error('Error removing member:', error);
          this.snackBar.open('Error al eliminar el miembro', 'Cerrar', {
            duration: 3000
          });
        }
      });
    }
  }

  getMemberDisplayName(member: BoardMember): string {
    const fullName = member.fullName;
    const userName = member.userName;
    const userEmail = member.userEmail;

    if (fullName && userName) {
      return `${fullName} - ${userName}`;
    } else if (fullName) {
      return fullName;
    } else if (userName) {
      return userName;
    } else if (userEmail) {
      return userEmail;
    } else {
      return `Usuario ${member.userId}`;
    }
  }

  getAssignedTasksCount(member: BoardMember): number {
    return this.tasks.filter(task => task.assignedToId === member.userId).length;
  }

  onClose(): void {
    this.dialogRef.close();
  }
}


