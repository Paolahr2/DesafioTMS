import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { BoardService } from '../../../core/services/board.service';
import { AuthService } from '../../../core/services';
import { CreateBoardDialogComponent, CreateBoardData } from '../dialogs/create-board-dialog/create-board-dialog.component';
import { DeleteBoardDialogComponent, DeleteBoardDialogData } from '../dialogs/delete-board-dialog/delete-board-dialog.component';
import { AllNotificationsDialogComponent } from '../dialogs/all-notifications-dialog/all-notifications-dialog.component';
import { Board } from '@core/models/entities';
import { TaskService } from '../../../core/services/task.service';
import { UserNotificationService } from '../../../core/services/user-notification.service';
import { CollaborationService } from '../../../core/services/collaboration.service';

@Component({
  selector: 'app-boards',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    CreateBoardDialogComponent,
    DeleteBoardDialogComponent,
    AllNotificationsDialogComponent
  ],
  templateUrl: './boards.component.html',
  styleUrls: ['./boards.component.scss']
})
export class BoardsComponent implements OnInit {
  boards: Board[] = [];
  isLoading = false;
  currentUser: any = null;

  // Mapa para guardar el número de tareas por tablero
  boardTaskCounts: { [boardId: string]: number } = {};

  // Contador total de notificaciones (invitaciones + notificaciones generales no leídas)
  totalNotificationsCount = 0;

  constructor(
    private boardService: BoardService,
    private authService: AuthService,
    private router: Router,
    private dialog: MatDialog,
    private taskService: TaskService,
    private userNotificationService: UserNotificationService,
    private collaborationService: CollaborationService
  ) {}

  ngOnInit() {
    this.loadBoards();
    this.loadNotificationsCounts();
  }

  async loadBoards() {
    this.isLoading = true;
    try {
      // Solo cargar currentUser si no está ya cargado
      if (!this.currentUser) {
        this.currentUser = await this.authService.getCurrentUser();
      }

      if (this.currentUser) {
        this.boardService.getUserBoards().subscribe({
          next: (boards: Board[]) => {
            this.boards = boards;
            this.isLoading = false;
            console.log('Boards loaded:', boards);
            console.log('Current user:', this.currentUser);

            // Por cada tablero, obtener el número de tareas
            boards.forEach(board => {
              this.taskService.getBoardTasks(board.id).subscribe({
                next: (tasks) => {
                  this.boardTaskCounts[board.id] = tasks.length;
                  console.log(`Board ${board.id} has ${tasks.length} tasks`);
                },
                error: () => {
                  this.boardTaskCounts[board.id] = 0;
                }
              });
            });
          },
          error: (error: any) => {
            console.error('Error al cargar tableros:', error);
            this.isLoading = false;
          }
        });
      } else {
        this.router.navigate(['/login']);
      }
    } catch (error) {
      console.error('Error al cargar tableros:', error);
      this.isLoading = false;
    }
  }

  loadNotificationsCounts() {
    // Suscribirse al conteo de notificaciones no leídas
    this.userNotificationService.unreadCount$.subscribe(unreadCount => {
      // Obtener también las invitaciones pendientes
      this.collaborationService.getPendingInvitations().subscribe({
        next: (invitations) => {
          this.totalNotificationsCount = unreadCount + invitations.length;
        },
        error: () => {
          this.totalNotificationsCount = unreadCount;
        }
      });
    });

    // Forzar carga inicial
    this.userNotificationService.refreshNotifications();
  }

  openNotifications() {
    const dialogRef = this.dialog.open(AllNotificationsDialogComponent, {
      width: '600px',
      maxWidth: '95vw',
      disableClose: false,
      data: {}
    });

    dialogRef.afterClosed().subscribe(() => {
      // Recargar conteos después de cerrar el modal
      this.loadNotificationsCounts();
    });
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }

  async createBoard(): Promise<void> {
    const dialogRef = this.dialog.open(CreateBoardDialogComponent, {
      width: '980px',
      maxWidth: '95vw',
      disableClose: true,
      data: {}
    });

    dialogRef.afterClosed().subscribe(async (result: CreateBoardData | undefined) => {
      if (!result) return;

      try {
        // Usar currentUser ya cargado o cargar si es necesario
        if (!this.currentUser) {
          this.currentUser = await this.authService.getCurrentUser();
        }

        if (!this.currentUser) {
          await this.router.navigate(['/login']);
          return;
        }

        console.log('Creating board with user:', this.currentUser);

        // Preparar payload solo con los campos que espera el backend
        const payload = {
          title: result.title,
          description: result.description,
          color: result.color,
          isPublic: result.isPublic
        };

        console.log('Board payload:', payload);

        // El servicio en core/services/board.service.ts devuelve un Observable
        this.boardService.createBoard(payload).subscribe({
          next: (createdBoard: Board) => {
            console.log('Board created successfully:', createdBoard);
            this.loadBoards();
          },
          error: (error: any) => {
            console.error('Error creando tablero:', error);
            alert(error?.error?.message || error?.message || 'Error creando tablero');
          }
        });
      } catch (err: any) {
        console.error('Error preparando creación de tablero:', err);
        alert(err?.message || 'Error creando tablero');
      }
    });
  }

  openBoard(board: Board) {
    this.router.navigate(['/boards', board.id]);
  }

  trackByBoardId(index: number, board: Board): string {
    return board.id;
  }

  editBoard(board: Board) {
    // Aquí abriríamos un diálogo para editar el tablero
    console.log('Editar tablero:', board.id);
  }

  deleteBoard(board: Board) {
    const dialogRef = this.dialog.open(DeleteBoardDialogComponent, {
      width: '450px',
      maxWidth: '95vw',
      disableClose: true,
      data: { board } as DeleteBoardDialogData
    });

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      if (!confirmed) return;

      this.boardService.deleteBoard(board.id).subscribe({
        next: () => {
          // Recargar la lista de tableros después de eliminar
          this.loadBoards();
        },
        error: (error: any) => {
          console.error('Error eliminando tablero:', error);
          alert(error?.error?.message || error?.message || 'Error eliminando tablero');
        }
      });
    });
  }

  getGradientStyle(color: string): string {
    // Usar color por defecto si no hay color definido
    if (!color) {
      color = '#3498db';
    }

    // Validar que el color sea un string hexadecimal válido
    if (!color || typeof color !== 'string' || !/^#[0-9A-F]{6}$/i.test(color)) {
      color = '#3498db'; // Color por defecto si no es válido
    }

    // Crear un gradiente dinámico más opaco basado en el color del tablero
    const lighterColor = this.lightenColor(color, 0.15); // Reducido de 0.3 a 0.15
    const darkerColor = this.darkenColor(color, 0.15);   // Reducido de 0.2 a 0.15
    return `linear-gradient(135deg, ${lighterColor} 0%, ${color} 50%, ${darkerColor} 100%)`;
  }

  private lightenColor(color: string, percent: number): string {
    // Convertir hex a RGB y aclarar
    const num = parseInt(color.replace("#", ""), 16);
    const amt = Math.round(2.55 * percent * 100);
    const R = (num >> 16) + amt;
    const G = (num >> 8 & 0x00FF) + amt;
    const B = (num & 0x0000FF) + amt;
    return "#" + (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000 +
      (G < 255 ? G < 1 ? 0 : G : 255) * 0x100 +
      (B < 255 ? B < 1 ? 0 : B : 255)).toString(16).slice(1);
  }

  private darkenColor(color: string, percent: number): string {
    // Convertir hex a RGB y oscurecer
    const num = parseInt(color.replace("#", ""), 16);
    const amt = Math.round(2.55 * percent * 100);
    const R = (num >> 16) - amt;
    const G = (num >> 8 & 0x00FF) - amt;
    const B = (num & 0x0000FF) - amt;
    return "#" + (0x1000000 + (R > 255 ? 255 : R < 0 ? 0 : R) * 0x10000 +
      (G > 255 ? 255 : G < 0 ? 0 : G) * 0x100 +
      (B > 255 ? 255 : B < 0 ? 0 : B)).toString(16).slice(1);
  }
}


