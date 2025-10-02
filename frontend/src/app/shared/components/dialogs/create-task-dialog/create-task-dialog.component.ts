import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TaskItem } from '@core/models/entities'

export interface CreateTaskData {
  boardId: string;
  status: number; // 0=Todo, 1=InProgress, 3=Done
}

@Component({
  selector: 'app-create-task-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatChipsModule,
    MatIconModule,
    FormsModule
  ],
  templateUrl: './create-task-dialog.component.html',
  styleUrls: ['./create-task-dialog.component.scss']
})
export class CreateTaskDialogComponent {
  task: Partial<TaskItem> = {
    title: '',
    description: '',
    priority: 'Medium',
    status: 0, // Default: Todo
    listId: undefined, // Opcional: para checklists
    tags: []
  };

  separatorKeysCodes = [13, 188]; // Enter, comma
  addOnBlur = true;

  constructor(
    public dialogRef: MatDialogRef<CreateTaskDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateTaskData
  ) {
    // Establecer el boardId y status desde los datos
    this.task.boardId = data.boardId;
    this.task.status = data.status;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onCreate(): void {
    if (this.task.title?.trim()) {
      // Convertir al formato que espera el backend (camelCase)
      const taskData = {
        title: this.task.title,
        description: this.task.description || '',
        boardId: this.task.boardId,
        status: this.task.status, // Campo obligatorio: TaskStatus enum
        listId: this.task.listId, // Campo opcional: solo para checklists
        priority: this.mapPriorityToNumber(this.task.priority),
        dueDate: this.task.dueDate ? new Date(this.task.dueDate).toISOString() : null,
        assignedToId: this.task.assignedToId || null,
        tags: this.task.tags || []
      };

      console.log('Datos a enviar al backend:', taskData);
      console.log('Status:', this.task.status);
      this.dialogRef.close(taskData);
    }
  }

  private mapPriorityToNumber(priority: string | undefined): number {
    switch (priority) {
      case 'Low': return 0;
      case 'Medium': return 1;
      case 'High': return 2;
      case 'Critical': return 3;
      default: return 1; // Medium por defecto
    }
  }

  addTag(event: any): void {
    const value = (event.value || '').trim();
    if (value) {
      this.task.tags!.push(value);
    }
    event.chipInput!.clear();
  }

  removeTag(tag: string): void {
    const index = this.task.tags!.indexOf(tag);
    if (index >= 0) {
      this.task.tags!.splice(index, 1);
    }
  }
}
