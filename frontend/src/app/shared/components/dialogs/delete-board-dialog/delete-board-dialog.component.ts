import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Board } from '@core/models/entities';

export interface DeleteBoardDialogData {
  board: Board;
}

@Component({
  selector: 'app-delete-board-dialog',
  templateUrl: './delete-board-dialog.component.html',
  styleUrls: ['./delete-board-dialog.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ]
})
export class DeleteBoardDialogComponent {
  isDeleting = false;

  constructor(
    public dialogRef: MatDialogRef<DeleteBoardDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DeleteBoardDialogData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.isDeleting = true;
    // El componente padre manejará la eliminación y cerrará el diálogo
    this.dialogRef.close(true);
  }
}