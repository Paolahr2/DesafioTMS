import { Component, Inject } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { TaskService } from '../../../../core/services/task.service';
import { CollaborationService } from '../../../../core/services/collaboration.service';
import { TaskItem, BoardMember } from '@core/models/entities';
import { CommonModule } from '@angular/common';
import { TaskDueDateDialogComponent } from '../task-due-date-dialog/task-due-date-dialog.component';

@Component({
  selector: 'app-assign-task-dialog',
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
  templateUrl: './assign-task-dialog.component.html',
  styleUrls: ['./assign-task-dialog.component.scss']
})
export class AssignTaskDialogComponent {
  boardMembers: BoardMember[] = [];

  constructor(
    private dialogRef: MatDialogRef<AssignTaskDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { task: TaskItem; boardId: string },
    private taskService: TaskService,
    private collaborationService: CollaborationService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.loadBoardMembers();
  }

  private loadBoardMembers(): void {
    this.collaborationService.getBoardMembers(this.data.boardId).subscribe({
      next: (members) => {
        this.boardMembers = members;
      },
      error: (error) => {
        console.error('Error loading board members:', error);
        this.snackBar.open('Error al cargar los miembros del tablero', 'Cerrar', {
          duration: 3000
        });
      }
    });
  }

  assignToUser(member: BoardMember): void {
    this.taskService.assignTask(this.data.task.id, member.userId, this.data.task.title).subscribe({
      next: (updatedTask) => {
        this.snackBar.open(`Tarea asignada a ${this.getMemberDisplayName(member)}`, 'Cerrar', {
          duration: 3000
        });
        this.dialogRef.close(updatedTask);
      },
      error: (error) => {
        console.error('Error assigning task:', error);
        this.snackBar.open('Error al asignar la tarea', 'Cerrar', {
          duration: 3000
        });
      }
    });
  }

  unassignTask(): void {
    this.taskService.assignTask(this.data.task.id, null, this.data.task.title).subscribe({
      next: (updatedTask) => {
        this.snackBar.open('Asignación removida', 'Cerrar', {
          duration: 3000
        });
        this.dialogRef.close(updatedTask);
      },
      error: (error) => {
        console.error('Error unassigning task:', error);
        this.snackBar.open('Error al remover la asignación', 'Cerrar', {
          duration: 3000
        });
      }
    });
  }

  isCurrentlyAssigned(member: BoardMember): boolean {
    return this.data.task.assignedToId === member.userId;
  }

  getUserInitials(member: BoardMember): string {
    const username = member.fullName || member.userName || 'U';
    return username.charAt(0).toUpperCase();
  }

  updateDueDate() {
    const dialogRef = this.dialog.open(TaskDueDateDialogComponent, {
      width: '400px',
      data: { currentDate: this.data.task.dueDate }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.taskService.updateDueDate(this.data.task.id, result).subscribe({
          next: () => {
            this.data.task.dueDate = result;
            this.snackBar.open('Fecha límite actualizada', 'Cerrar', {
              duration: 3000
            });
          },
          error: (error) => {
            console.error('Error updating due date:', error);
            this.snackBar.open('Error al actualizar la fecha límite', 'Cerrar', {
              duration: 3000
            });
          }
        });
      }
    });
  }

  formatDate(date: string | Date | null): string {
    if (!date) return '';
    return new Date(date).toLocaleDateString('es-ES', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
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

  getPriorityClass(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'low':
        return 'priority-low';
      case 'medium':
        return 'priority-medium';
      case 'high':
        return 'priority-high';
      case 'critical':
        return 'priority-critical';
      default:
        return 'priority-medium';
    }
  }

  getPriorityDisplayName(priority: string): string {
    switch (priority?.toLowerCase()) {
      case 'low':
        return 'Baja';
      case 'medium':
        return 'Media';
      case 'high':
        return 'Alta';
      case 'critical':
        return 'Crítica';
      default:
        return 'Media';
    }
  }



  getMemberDisplayName(member: BoardMember): string {
    const fullName = member.fullName;
    const userName = member.userName;
    
    if (fullName && userName) {
      return `${fullName} - ${userName}`;
    } else if (fullName) {
      return fullName;
    } else if (userName) {
      return userName;
    } else {
      return member.userEmail || 'Usuario desconocido';
    }
  }

  onClose(): void {
    this.dialogRef.close();
  }
}