import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BoardService } from '../../../core/services/board.service';
import { ListService, ListDto, CreateListDto } from '../../../core/services';
import { TaskService } from '../../../core/services/task.service';
import { CollaborationService } from '../../../core/services/collaboration.service';
import { AuthService } from '../../../core/services/auth.service';
import { Board, TaskItem, BoardPermissions } from '@core/models/entities';
import { CommonModule } from '@angular/common';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { EditTaskDialogComponent } from '../../../shared/components/dialogs/edit-task-dialog/edit-task-dialog.component';
import { CreateTaskDialogComponent } from '../../../shared/components/dialogs/create-task-dialog/create-task-dialog.component';
import { DeleteTaskDialogComponent } from '../../../shared/components/dialogs/delete-task-dialog/delete-task-dialog.component';
import { AddListDialogComponent } from '../../../shared/components/dialogs/add-list-dialog/add-list-dialog.component';
import { InviteUserDialogComponent } from '../../../shared/components/dialogs/invite-user-dialog.component';
import { ManageMembersDialogComponent } from '../../../shared/components/dialogs/manage-members-dialog.component';
import { AssignTaskDialogComponent } from '../../../shared/components/dialogs/assign-task-dialog/assign-task-dialog.component';
import { DeleteListDialogComponent } from '../../../shared/components/dialogs/delete-list-dialog/delete-list-dialog.component';
import { ConfirmDeleteItemDialogComponent } from '../../../shared/components/dialogs/confirm-delete-item-dialog/confirm-delete-item-dialog.component';

import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-board-detail',
  standalone: true,
  imports: [
    CommonModule,
    DragDropModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatCardModule,
    MatChipsModule,
    MatDialogModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    MatDividerModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './board-detail.component.html',
  styleUrls: ['./board-detail.component.scss']
})
export class BoardDetailComponent implements OnInit {
  board: Board | null = null;
  loading = true;
  tasks: TaskItem[] = [];
  boardMembers: any[] = [];
  currentUser: any = null;
  boardPermissions: BoardPermissions = BoardPermissions.viewer(); // Default to viewer permissions
  openPriorityMenuId: string | null = null;
  today = new Date();

  // Listas del tablero (equivalente a columnas en Trello)
  lists: ListDto[] = [];

  // Propiedad para el gradiente del fondo del tablero
  boardBackgroundGradient: string = 'linear-gradient(135deg, #5DADE2 0%, #3498db 50%, #2874A6 100%)';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private boardService: BoardService,
    private taskService: TaskService,
    private collaborationService: CollaborationService,
    private listService: ListService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    // Cargar usuario actual
    this.currentUser = this.authService.getCurrentUser();
    
    const boardId = this.route.snapshot.paramMap.get('id');
    if (boardId) {
      this.loadBoard(boardId);
      this.loadLists(boardId);
      this.loadTasks(boardId);
      this.loadBoardMembers(boardId);
    }
  }

  isTaskOverdue(task: TaskItem): boolean {
    if (!task.dueDate) return false;
    const dueDate = new Date(task.dueDate);
    dueDate.setHours(0, 0, 0, 0);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return dueDate < today;
  }

  isTaskDueToday(task: TaskItem): boolean {
    if (!task.dueDate) return false;
    const dueDate = new Date(task.dueDate);
    dueDate.setHours(0, 0, 0, 0);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return dueDate.getTime() === today.getTime();
  }

  updateTaskPriority(task: TaskItem): void {
    if (!task.id) return;
    this.taskService.updateTask(task.id, { priority: task.priority }).subscribe({
      next: () => {
        // La tarea se actualizó correctamente
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error al actualizar la prioridad:', error);
        // Revertir el cambio en caso de error
        task.priority = task.priority === 'High' ? 'Low' : 
                       task.priority === 'Medium' ? 'High' : 'Medium';
      }
    });
  }

  openCreateTaskDialog(status?: string): void {
    // Pasar el status directamente (0=Todo, 1=InProgress, 3=Done)
    let taskStatus = 0; // Default: Todo
    if (status === 'InProgress') {
      taskStatus = 1;
    } else if (status === 'Done') {
      taskStatus = 3;
    }

    const dialogRef = this.dialog.open(CreateTaskDialogComponent, {
      width: '500px',
      data: { 
        boardId: this.board?.id,
        status: taskStatus
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Crear la tarea en el backend
        console.log('Creando tarea con datos:', result);
        this.taskService.createTask(result).subscribe({
          next: (createdTask: any) => {
            console.log('Tarea creada exitosamente:', createdTask);
            // Recargar todas las tareas del tablero
            this.loadTasks(this.board?.id || '');
          },
          error: (error) => {
            console.error('Error al crear la tarea:', error);
            this.snackBar.open('Error al crear la tarea. Por favor, inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        });
      }
    });
  }

  private loadBoard(boardId: string): void {
    this.boardService.getBoardById(boardId).subscribe({
      next: (board: Board) => {
        this.board = board;
        this.loading = false;
        this.updateBoardBackgroundGradient();
        this.applyBoardColor();
        this.cdr.detectChanges(); // Forzar detección de cambios para actualizar el gradiente
      },
      error: (error: any) => {
        console.error('Error loading board:', error);
        this.loading = false;
      }
    });
  }

  private updateBoardBackgroundGradient(): void {
    // Usar color por defecto si no hay color definido o es inválido
    let boardColor = this.board?.color || '#3498db';
    if (!boardColor || typeof boardColor !== 'string' || !/^#[0-9A-F]{6}$/i.test(boardColor)) {
      boardColor = '#3498db'; // Color por defecto si no es válido
    }

    // Hacer el color mucho más opaco mezclándolo con blanco (70% más opaco)
    const opaqueColor = this.mixWithWhite(boardColor, 0.7);
    
    // Crear un gradiente dinámico más suave
    const lighterColor = this.lightenColor(opaqueColor, 0.15);
    const darkerColor = this.darkenColor(opaqueColor, 0.15);
    this.boardBackgroundGradient = `linear-gradient(135deg, ${lighterColor} 0%, ${opaqueColor} 50%, ${darkerColor} 100%)`;
    console.log('Gradiente actualizado:', this.boardBackgroundGradient, 'Color del tablero:', boardColor, 'Color opaco:', opaqueColor);
  }

  private mixWithWhite(color: string, opacity: number): string {
    // Mezclar el color con blanco para hacerlo más opaco
    const hex = color.replace('#', '');
    const r = parseInt(hex.substr(0, 2), 16);
    const g = parseInt(hex.substr(2, 2), 16);
    const b = parseInt(hex.substr(4, 2), 16);
    
    // Mezclar con blanco (255, 255, 255)
    const newR = Math.floor(r + (255 - r) * opacity);
    const newG = Math.floor(g + (255 - g) * opacity);
    const newB = Math.floor(b + (255 - b) * opacity);
    
    return `#${newR.toString(16).padStart(2, '0')}${newG.toString(16).padStart(2, '0')}${newB.toString(16).padStart(2, '0')}`;
  }

  private loadLists(boardId: string): void {
    // Cargar solo listas de checklist creadas por el usuario
    // Las columnas Kanban (Por Hacer, En Progreso, Hecho) están hardcoded en el HTML
    this.listService.getListsByBoardId(boardId).subscribe({
      next: (lists: ListDto[]) => {
        this.lists = lists.sort((a, b) => a.order - b.order);
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading lists:', error);
      }
    });
  }

  private loadTasks(boardId: string): void {
    this.taskService.getBoardTasks(boardId).subscribe({
      next: (tasks: TaskItem[]) => {
        // Mapear las tareas del backend al formato del TaskItem
        this.tasks = tasks.map(task => this.mapTaskFromBackend(task));
        this.organizeTasksByList();
      },
      error: (error: any) => {
        console.error('Error loading tasks:', error);
        this.tasks = [];
        this.organizeTasksByList();
      }
    });
  }

  private organizeTasksByList(): void {
    console.log('Organizando tareas por estado. Total tareas:', this.tasks.length);
    // Las tareas se organizan por status (0=Todo, 1=InProgress, 3=Done)
    // Las columnas Kanban están hardcoded en el HTML y filtran por status
    this.cdr.detectChanges();
  }

  private applyBoardColor(): void {
    // Usar color por defecto si no hay color definido o es inválido
    let boardColor = this.board?.color || '#3498db';
    if (!boardColor || typeof boardColor !== 'string' || !/^#[0-9A-F]{6}$/i.test(boardColor)) {
      boardColor = '#3498db'; // Color por defecto si no es válido
    }

    // Aplicar el color del tablero como variable CSS global
    document.documentElement.style.setProperty('--board-primary-color', boardColor);
    
    // Calcular colores derivados del color principal
    const primaryColor = boardColor;
    const lighterColor = this.lightenColor(primaryColor, 0.3);
    const darkerColor = this.darkenColor(primaryColor, 0.2);
    
    document.documentElement.style.setProperty('--board-primary-lighter', lighterColor);
    document.documentElement.style.setProperty('--board-primary-darker', darkerColor);
  }

  private lightenColor(color: string, percent: number): string {
    // Convertir hex a RGB
    const hex = color.replace('#', '');
    const r = parseInt(hex.substr(0, 2), 16);
    const g = parseInt(hex.substr(2, 2), 16);
    const b = parseInt(hex.substr(4, 2), 16);
    
    // Lighten
    const newR = Math.min(255, Math.floor(r + (255 - r) * percent));
    const newG = Math.min(255, Math.floor(g + (255 - g) * percent));
    const newB = Math.min(255, Math.floor(b + (255 - b) * percent));
    
    // Convertir de vuelta a hex
    return `#${newR.toString(16).padStart(2, '0')}${newG.toString(16).padStart(2, '0')}${newB.toString(16).padStart(2, '0')}`;
  }

  private darkenColor(color: string, percent: number): string {
    // Convertir hex a RGB
    const hex = color.replace('#', '');
    const r = parseInt(hex.substr(0, 2), 16);
    const g = parseInt(hex.substr(2, 2), 16);
    const b = parseInt(hex.substr(4, 2), 16);
    
    // Darken
    const newR = Math.max(0, Math.floor(r * (1 - percent)));
    const newG = Math.max(0, Math.floor(g * (1 - percent)));
    const newB = Math.max(0, Math.floor(b * (1 - percent)));
    
    // Convertir de vuelta a hex
    return `#${newR.toString(16).padStart(2, '0')}${newG.toString(16).padStart(2, '0')}${newB.toString(16).padStart(2, '0')}`;
  }

  onDrop(event: CdkDragDrop<TaskItem[]>): void {
    if (event.previousContainer === event.container) {
      // Mover dentro de la misma columna
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // Mover entre columnas diferentes
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );

      // Actualizar el estado de la tarea en el backend
      const task = event.container.data[event.currentIndex];
      
      // Mapear el ID del contenedor al status numérico
      const statusMap: { [key: string]: number } = {
        'Todo': 0,
        'InProgress': 1,
        'Done': 3
      };
      
      const newStatus = statusMap[event.container.id];
      
      if (task && newStatus !== undefined && task.status !== newStatus) {
        this.taskService.updateTask(task.id, { status: newStatus }).subscribe({
          next: () => {
            task.status = newStatus;
          },
          error: (error) => {
            console.error('Error updating task status:', error);
            // Revertir el cambio en caso de error
            this.loadTasks(this.board!.id);
          }
        });
      }
    }
  }

  createTask(status: number): void {
    if (!this.board) {
      console.error('Error: board no disponible', { board: this.board });
      return;
    }

    console.log('Abriendo diálogo para crear tarea con status:', status);

    const dialogRef = this.dialog.open(CreateTaskDialogComponent, {
      width: '800px',
      maxWidth: '95vw',
      disableClose: true,
      data: {
        boardId: this.board.id,
        status: status
      }
    });

    dialogRef.afterClosed().subscribe((result: any | undefined) => {
      if (result) {
        console.log('Datos de la nueva tarea del diálogo:', result);

        // Crear el objeto correcto para enviar al backend
        const taskToCreate = {
          title: result.title,
          description: result.description || '',
          boardId: result.boardId,
          status: result.status, // Usar status en lugar de listId
          listId: result.listId, // Opcional: para asociar con checklist
          priority: result.priority,
          dueDate: result.dueDate,
          assignedToId: result.assignedToId,
          tags: result.tags || []
        };

        console.log('Enviando al backend:', taskToCreate);

        this.taskService.createTask(taskToCreate).subscribe({
          next: (createdTask: any) => {
            console.log('Tarea creada exitosamente:', createdTask);

            // Mapear la respuesta del backend al formato del TaskItem
            const mappedTask = this.mapTaskFromBackend(createdTask);

            this.tasks.push(mappedTask);
            this.organizeTasksByList();
            console.log('Tarea agregada a la lista. Total tareas:', this.tasks.length);
            console.log('Tareas por lista:', this.lists.map(list => `${list.title}: ${(list as any).tasks?.length || 0}`));
            this.cdr.detectChanges();
          },
          error: (error) => {
            console.error('Error creating task:', error);
            this.snackBar.open('Error al crear la tarea: ' + (error.error?.message || error.message || 'Error desconocido'), 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        });
      }
    });
  }

  deleteTask(task: TaskItem): void {
    // Regla 1: Las tareas completadas NO se pueden eliminar
    if (task.isCompleted || task.completedAt) {
      this.snackBar.open('❌ No se puede eliminar una tarea completada. Las tareas finalizadas son permanentes por razones de auditoría.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      return;
    }

    // Regla 2: Solo el creador de la tarea puede eliminarla
    if (this.currentUser && task.createdById !== this.currentUser.id) {
      this.snackBar.open('❌ No tienes permiso para eliminar esta tarea. Solo el creador puede eliminarla.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      return;
    }

    // Si pasa las validaciones, abrir diálogo de confirmación
    const dialogRef = this.dialog.open(DeleteTaskDialogComponent, {
      width: '500px',
      disableClose: true,
      data: { task, allowDelete: true }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.taskService.deleteTask(task.id).subscribe({
          next: () => {
            this.tasks = this.tasks.filter(t => t.id !== task.id);
            this.organizeTasksByList();
          },
          error: (error) => {
            console.error('Error deleting task:', error);
            this.snackBar.open('Error al eliminar la tarea. Inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        });
      }
    });
  }

  // Método para mover tareas entre estados (Por Hacer -> En Progreso -> Hecho)
  moveTaskToNextStatus(task: TaskItem): void {
    // Mapeo de estados: 0=Todo, 1=InProgress, 3=Done
    let nextStatus: number;
    
    if (task.status === 0) { // Todo -> InProgress
      nextStatus = 1;
    } else if (task.status === 1) { // InProgress -> Done
      nextStatus = 3;
    } else {
      return; // Ya está en Done
    }

    // Actualizar la tarea con el nuevo status
    this.taskService.updateTask(task.id, { status: nextStatus }).subscribe({
      next: (updatedTask: any) => {
        task.status = nextStatus;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error moving task:', error);
        this.snackBar.open('Error al mover la tarea. Inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  // Método para mover tareas al estado anterior
  moveTaskToPreviousStatus(task: TaskItem): void {
    // Mapeo de estados: 0=Todo, 1=InProgress, 3=Done
    let previousStatus: number;

    if (task.status === 3) { // Done -> InProgress
      previousStatus = 1;
    } else if (task.status === 1) { // InProgress -> Todo
      previousStatus = 0;
    } else {
      return; // Ya está en Todo
    }

    // Actualizar la tarea con el nuevo status
    this.taskService.updateTask(task.id, { status: previousStatus }).subscribe({
      next: (updatedTask: any) => {
        task.status = previousStatus;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error moving task to previous status:', error);
        this.snackBar.open('Error al mover la tarea. Inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  // Verificar si se puede mover a siguiente estado
  canMoveToNext(task: TaskItem): boolean {
    return task.status !== 3; // No se puede mover si ya está en Done (3)
  }

  // Verificar si se puede mover a estado anterior
  canMoveToPrevious(task: TaskItem): boolean {
    return task.status !== 0; // No se puede mover si ya está en Todo (0)
  }

  // Verificar si el usuario puede eliminar la tarea
  canDeleteTask(task: TaskItem): boolean {
    // No se pueden eliminar tareas completadas
    if (task.isCompleted || task.completedAt) {
      return false;
    }
    // Solo el creador puede eliminar
    return this.currentUser && task.createdById === this.currentUser.id;
  }



  goBack(): void {
    this.router.navigate(['/boards']);
  }

  // Helper para convertir prioridad string a número (como espera el backend)
  private priorityToNumber(priority: 'Low' | 'Medium' | 'High' | 'Critical'): number {
    const priorityMap = {
      'Low': 0,
      'Medium': 1,
      'High': 2,
      'Critical': 3
    };
    return priorityMap[priority];
  }

  changeTaskPriority(task: TaskItem, newPriority: 'Low' | 'Medium' | 'High' | 'Critical'): void {
    if (task.priority === newPriority) {
      return; // No hacer nada si la prioridad es la misma
    }

    // Convertir la prioridad a número para el backend
    const priorityValue = this.priorityToNumber(newPriority);

    this.taskService.updateTask(task.id, { priority: priorityValue }).subscribe({
      next: (updatedTask) => {
        task.priority = newPriority;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error updating task priority:', error);
        this.snackBar.open('Error al cambiar la prioridad de la tarea. Inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  togglePriorityMenu(task: TaskItem | null): void {
    this.openPriorityMenuId = task ? (this.openPriorityMenuId === task.id ? null : task.id) : null;
    this.cdr.detectChanges();
  }

  addList(): void {
    const dialogRef = this.dialog.open(AddListDialogComponent, {
      width: '600px',
      disableClose: true,
      data: { boardId: this.board?.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.lists.push(result);
        this.cdr.detectChanges();
      }
    });
  }

  editTask(task: TaskItem): void {
    const dialogRef = this.dialog.open(EditTaskDialogComponent, {
      width: '600px',
      disableClose: true,
      data: { task, boardMembers: this.boardMembers }
    });

    dialogRef.afterClosed().subscribe((result: TaskItem | undefined) => {
      if (result) {
        this.taskService.updateTask(task.id, result).subscribe({
          next: (updatedTask: any) => {
            const mappedTask = this.mapTaskFromBackend(updatedTask);
            const index = this.tasks.findIndex(t => t.id === task.id);
            if (index !== -1) {
              this.tasks[index] = mappedTask;
              this.organizeTasksByList();
            }
          },
          error: (error) => {
            console.error('Error updating task:', error);
          }
        });
      }
    });
  }



  private mapTaskFromBackend(taskDto: any): TaskItem {
    return {
      id: taskDto.id,
      title: taskDto.title,
      description: taskDto.description,
      status: taskDto.status !== undefined ? taskDto.status : 0, // TaskStatus enum del backend
      listId: taskDto.listId, // Opcional: para checklists
      priority: this.mapPriorityFromNumber(taskDto.priority),
      boardId: taskDto.boardId,
      assignedToId: taskDto.assignedToId,
      createdById: taskDto.createdById,
      createdByName: taskDto.createdByName || 'Usuario desconocido',
      dueDate: taskDto.dueDate ? new Date(taskDto.dueDate) : undefined,
      completedAt: taskDto.completedAt ? new Date(taskDto.completedAt) : undefined,
      tags: taskDto.tags || [],
      createdAt: new Date(taskDto.createdAt),
      updatedAt: new Date(taskDto.updatedAt),
      position: taskDto.position || 0,
      isCompleted: taskDto.isCompleted || false,
      completedBy: taskDto.completedBy,
      progressPercentage: taskDto.progressPercentage || 0,
      attachments: taskDto.attachments || []
    };
  }



  private mapPriorityFromNumber(priority: number): 'Low' | 'Medium' | 'High' | 'Critical' {
    switch (priority) {
      case 0: return 'Low';
      case 1: return 'Medium';
      case 2: return 'High';
      case 3: return 'Critical';
      default: return 'Medium';
    }
  }

  private loadBoardMembers(boardId: string): void {
    this.collaborationService.getBoardMembers(boardId).subscribe({
      next: (members: any[]) => {
        this.boardMembers = members;
        this.cdr.detectChanges();
      },
      error: (error: any) => {
        console.error('Error loading board members:', error);
        this.boardMembers = [];
      }
    });
  }

  // Collaboration methods
  openInviteDialog(): void {
    if (this.board) {
      const dialogRef = this.dialog.open(InviteUserDialogComponent, {
        width: '500px',
        data: { boardId: this.board.id }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          console.log('User invited successfully');
          // TODO: Refresh board members
        }
      });
    }
  }

  openManageMembersDialog(): void {
    if (this.board) {
      const dialogRef = this.dialog.open(ManageMembersDialogComponent, {
        width: '600px',
        data: { boardId: this.board.id }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          console.log('Members updated successfully');
          // TODO: Refresh board members
        }
      });
    }
  }

  openAddListDialog(): void {
    if (this.board) {
      console.log('Opening add list dialog for board:', this.board.id);
      const dialogRef = this.dialog.open(AddListDialogComponent, {
        width: '500px',
        data: { boardId: this.board.id }
      });

      dialogRef.afterClosed().subscribe(result => {
        console.log('Dialog closed with result:', result);
        if (result) {
          const createListDto: CreateListDto = {
            title: result.title,
            boardId: this.board!.id,
            order: this.lists.length,
            items: result.items || [],
            notes: result.notes || ''
          };

          console.log('Creating list with DTO:', createListDto);
          this.listService.createList(createListDto).subscribe({
            next: (response) => {
              console.log('List created successfully:', response);
              this.loadLists(this.board!.id);
            },
            error: (error) => {
              console.error('Error creating list:', error);
            }
          });
        } else {
          console.log('Dialog was cancelled');
        }
      });
    }
  }

  assignTask(task: TaskItem): void {
    if (this.board) {
      const dialogRef = this.dialog.open(AssignTaskDialogComponent, {
        width: '500px',
        data: { task, boardId: this.board.id }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          // Update the task in the local data
          const mappedTask = this.mapTaskFromBackend(result);
          const index = this.tasks.findIndex(t => t.id === mappedTask.id);
          if (index !== -1) {
            this.tasks[index] = mappedTask;
            this.organizeTasksByList();
          }
        }
      });
    }
  }



  getAssignedUser(task: TaskItem): any {
    if (!task.assignedToId || !this.boardMembers.length) {
      return null;
    }
    const member = this.boardMembers.find(m => m.userId === task.assignedToId);
    return member?.user || null;
  }

  getAssignedUserInitials(task: TaskItem): string {
    const user = this.getAssignedUser(task);
    if (user && user.username) {
      return user.username.charAt(0).toUpperCase();
    }
    return 'U';
  }

  // List management methods
  deleteList(list: ListDto): void {
    const dialogRef = this.dialog.open(DeleteListDialogComponent, {
      width: '500px',
      disableClose: true,
      data: { listTitle: list.title }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Deleting list:', list.id);
        this.listService.deleteList(list.id).subscribe({
          next: () => {
            console.log('List deleted successfully from backend');
            this.lists = this.lists.filter(l => l.id !== list.id);
            this.cdr.detectChanges();
          },
          error: (error) => {
            console.error('Error deleting list:', error);
            this.snackBar.open('Error al eliminar la lista del servidor. Por favor, inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        });
      }
    });
  }

  editList(list: ListDto): void {
    const dialogRef = this.dialog.open(AddListDialogComponent, {
      width: '500px',
      data: { 
        boardId: this.board?.id,
        isEdit: true,
        list: { ...list } // Pasar copia de la lista para editar
      }
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result) {
        // Actualizar la lista en el backend
        this.listService.updateList(list.id, result).subscribe({
          next: (updatedList: ListDto) => {
            // Actualizar la lista localmente
            const index = this.lists.findIndex(l => l.id === list.id);
            if (index !== -1) {
              this.lists[index] = updatedList;
              this.cdr.detectChanges();
            }
          },
          error: (error) => {
            console.error('Error updating list:', error);
            this.snackBar.open('Error al actualizar la lista. Inténtalo de nuevo.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        });
      }
    });
  }

  openAssignDialog(task: TaskItem): void {
    this.assignTask(task);
  }

  openEditTaskDialog(task: TaskItem): void {
    // Validación: No permitir editar tareas completadas
    if (task.isCompleted || task.completedAt) {
      this.snackBar.open('❌ No se puede editar una tarea completada. Las tareas finalizadas son permanentes por razones de auditoría.', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      return;
    }

    if (this.board) {
      const dialogRef = this.dialog.open(EditTaskDialogComponent, {
        width: '600px',
        data: { task, boardId: this.board.id }
      });

      dialogRef.afterClosed().subscribe(result => {
        if (result) {
          // Update the task in the local data
          const mappedTask = this.mapTaskFromBackend(result);
          const index = this.tasks.findIndex(t => t.id === mappedTask.id);
          if (index !== -1) {
            this.tasks[index] = mappedTask;
            this.organizeTasksByList();
          }
        }
      });
    }
  }

  getTasksForList(listId: string): TaskItem[] {
    return this.tasks.filter(task => task.listId === listId);
  }

  getConnectedLists(): string[] {
    return this.lists.map(list => list.id);
  }

  trackByTaskId(index: number, task: TaskItem): string {
    return task.id;
  }

  // Método para obtener tareas por estado (Trello-style)
  getTasksByStatus(status: string): TaskItem[] {
    // Mapear el estado de string al número correspondiente (TaskStatus enum)
    const statusMap: { [key: string]: number } = {
      'Todo': 0,        // TaskStatus.Todo
      'InProgress': 1,  // TaskStatus.InProgress
      'Done': 3         // TaskStatus.Done
    };
    
    const statusNumber = statusMap[status];
    
    if (statusNumber !== undefined) {
      return this.tasks.filter(task => task.status === statusNumber);
    }
    
    return [];
  }

  // Método para arreglar tareas huérfanas (sin status definido)
  fixOrphanTasks(): void {
    // Ahora las tareas usan el campo status (0=Todo, 1=InProgress, 3=Done)
    // Buscar tareas sin status definido y asignarlas a Todo
    const orphanTasks = this.tasks.filter(task => task.status === undefined || task.status === null);
    console.log('Orphan tasks found:', orphanTasks);

    if (orphanTasks.length === 0) {
      this.snackBar.open('No hay tareas sin estado asignado', 'Cerrar', { duration: 3000 });
      return;
    }

    let updatedCount = 0;
    orphanTasks.forEach(task => {
      this.taskService.updateTask(task.id, { status: 0 }).subscribe({
        next: (updatedTask: any) => {
          console.log('Task updated:', updatedTask);
          task.status = 0; // Todo
          updatedCount++;
          
          if (updatedCount === orphanTasks.length) {
            this.snackBar.open(`✅ ${updatedCount} tareas asignadas a "Por Hacer"`, 'Cerrar', { duration: 3000 });
            this.cdr.detectChanges();
          }
        },
        error: (error) => {
          console.error('Error updating task:', error);
          this.snackBar.open('Error al actualizar tarea: ' + task.title, 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
        }
      });
    });
  }

  toggleChecklistItem(list: ListDto, item: any): void {
    if (!list.items || !item) return;

    // Find the index of the item
    const itemIndex = list.items.indexOf(item);
    if (itemIndex === -1) return;

    // Toggle the completed state
    const currentItem = list.items[itemIndex];
    if (currentItem) {
      currentItem.completed = !currentItem.completed;
    }

    // Update the list in the backend
    const updateDto = {
      title: list.title,
      order: list.order,
      items: list.items
    };

    this.listService.updateList(list.id, updateDto).subscribe({
      next: (updatedList) => {
        console.log('Checklist item updated successfully');
        // Update the local list with the response
        const listIndex = this.lists.findIndex(l => l.id === list.id);
        if (listIndex !== -1) {
          this.lists[listIndex] = updatedList;
        }
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error updating checklist item:', error);
        // Revert the change on error
        if (currentItem) {
          currentItem.completed = !currentItem.completed;
        }
        this.snackBar.open('Error al actualizar el item del checklist', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  getCompletedItemsCount(list: ListDto): number {
    return list.items?.filter(item => item.completed).length || 0;
  }

  editChecklistItem(list: ListDto, item: any, index: number): void {
    // Prompt para editar el texto del item
    const newText = prompt('Editar item:', item.text);
    if (newText === null || newText.trim() === '') return;

    // Actualizar el texto del item
    item.text = newText.trim();

    // Actualizar en el backend
    const updateDto = {
      title: list.title,
      order: list.order,
      items: list.items
    };

    this.listService.updateList(list.id, updateDto).subscribe({
      next: (updatedList) => {
        console.log('Item de checklist editado exitosamente');
        const listIndex = this.lists.findIndex(l => l.id === list.id);
        if (listIndex !== -1) {
          this.lists[listIndex] = updatedList;
        }
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error al editar item del checklist:', error);
        this.snackBar.open('Error al editar el item del checklist', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
      }
    });
  }

  deleteChecklistItem(list: ListDto, item: any, index: number): void {
    // Abrir diálogo de confirmación
    const dialogRef = this.dialog.open(ConfirmDeleteItemDialogComponent, {
      width: '400px',
      data: { itemText: item.text }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result === true) {
        // Eliminar el item del array
        list.items?.splice(index, 1);

        // Actualizar en el backend
        const updateDto = {
          title: list.title,
          order: list.order,
          items: list.items
        };

        this.listService.updateList(list.id, updateDto).subscribe({
          next: (updatedList) => {
            console.log('Item de checklist eliminado exitosamente');
            const listIndex = this.lists.findIndex(l => l.id === list.id);
            if (listIndex !== -1) {
              this.lists[listIndex] = updatedList;
            }
            this.cdr.detectChanges();
          },
          error: (error) => {
            console.error('Error al eliminar item del checklist:', error);
            // Revertir el cambio en caso de error
            list.items?.splice(index, 0, item);
            this.snackBar.open('Error al eliminar el item del checklist', 'Cerrar', { duration: 5000, panelClass: ['error-snackbar'] });
          }
        });
      }
    });
  }
}
