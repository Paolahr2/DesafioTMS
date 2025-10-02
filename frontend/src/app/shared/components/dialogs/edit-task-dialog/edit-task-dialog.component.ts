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
import { TaskItem } from '@core/models/entities';

export interface EditTaskData {
  task: TaskItem;
  boardMembers: any[];
}

@Component({
  selector: 'app-edit-task-dialog',
  templateUrl: './edit-task-dialog.component.html',
  styleUrls: ['./edit-task-dialog.component.scss'],
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
  ]
})
export class EditTaskDialogComponent {
  task: TaskItem;
  boardMembers: any[] = [];
  separatorKeysCodes = [13, 188]; // Enter, comma
  addOnBlur = true;

  constructor(
    public dialogRef: MatDialogRef<EditTaskDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EditTaskData
  ) {
    this.task = { ...data.task }; // Crear una copia para no modificar el original
    this.boardMembers = data.boardMembers || [];
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.task.title?.trim()) {
      // Convertir los valores de string a los valores numéricos que espera el backend
      const taskData = {
        ...this.task,
        priority: this.mapPriorityToNumber(this.task.priority),
        dueDate: this.task.dueDate ? new Date(this.task.dueDate).toISOString().split('T')[0] : undefined // Solo fecha, sin hora
      };

      console.log('Datos a enviar al backend:', taskData);
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
      this.task.tags.push(value);
    }
    event.chipInput!.clear();
  }

  removeTag(tag: string): void {
    const index = this.task.tags.indexOf(tag);
    if (index >= 0) {
      this.task.tags.splice(index, 1);
    }
  }
}


