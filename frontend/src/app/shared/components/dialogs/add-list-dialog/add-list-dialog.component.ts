import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';

export interface AddListData {
  boardId: string;
}

export interface ListItem {
  id: string;
  text: string;
  completed: boolean;
  notes?: string;
  showNotes?: boolean;
}

export interface BoardList {
  id: string;
  title: string;
  items: ListItem[];
  notes?: string;
}

@Component({
  selector: 'app-add-list-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    FormsModule,
    MatChipsModule
  ],
  templateUrl: './add-list-dialog.component.html',
  styleUrls: ['./add-list-dialog.component.scss']
})
export class AddListDialogComponent {
  list: Partial<BoardList> = {
    title: '',
    items: [],
    notes: ''
  };

  constructor(
    public dialogRef: MatDialogRef<AddListDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AddListData
  ) {}

  onCancel(): void {
    this.dialogRef.close();
  }

  onCreate(): void {
    if (this.list.title?.trim()) {
      const listData = {
        title: this.list.title,
        items: this.list.items?.map(item => ({
          text: item.text,
          completed: item.completed,
          notes: item.notes || ''
        })) || [],
        notes: this.list.notes || ''
      };
      this.dialogRef.close(listData);
    }
  }

  addChecklistItem(): void {
    if (!this.list.items) {
      this.list.items = [];
    }
    this.list.items.push({
      id: this.generateId(),
      text: '',
      completed: false
    });
  }

  removeChecklistItem(index: number): void {
    if (this.list.items) {
      this.list.items.splice(index, 1);
    }
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }
}


