import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { TaskItem } from '../models/entities';
import { NotificationService } from './notification.service';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private apiUrl = environment.apiUrl + environment.endpoints.tasks;

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {}

  getBoardTasks(boardId: string): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${this.apiUrl}/board/${boardId}`);
  }

  createTask(task: Partial<TaskItem>) {
    console.log('Sending task data:', task);
    return this.http.post(`${this.apiUrl}`, task);
  }

  updateTask(id: string, task: any) {
    // Acepta any para permitir enviar prioridad como número
    return this.http.put(`${this.apiUrl}/${id}`, task);
  }

  changeTaskStatus(id: string, status: 'Pending' | 'InProgress' | 'Completed') {
    const statusNumber = this.mapStatusToNumber(status);
    return this.http.patch(`${this.apiUrl}/${id}/status`, { Status: statusNumber });
  }

  private mapStatusToNumber(status: string): number {
    switch (status) {
      case 'Pending': return 0;      // Todo = 0
      case 'InProgress': return 1;   // InProgress = 1
      case 'Completed': return 3;    // Done = 3
      default: return 0;
    }
  }

  deleteTask(id: string) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  assignTask(id: string, assignedToId: string | null, taskTitle: string) {
    return this.http.patch(`${this.apiUrl}/${id}/assign`, { AssignedToId: assignedToId })
      .pipe(
        tap(() => {
          if (assignedToId) {
            this.notificationService.info(
              'Tarea Asignada',
              `Se te ha asignado la tarea: ${taskTitle}`,
              undefined,
              { type: 'navigate', data: { taskId: id } }
            );
          }
        })
      );
  }

  updateDueDate(id: string, dueDate: Date | null) {
    return this.http.patch(`${this.apiUrl}/${id}/due-date`, { dueDate });
  }

  getTaskById(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.apiUrl}/${id}`);
  }
}



