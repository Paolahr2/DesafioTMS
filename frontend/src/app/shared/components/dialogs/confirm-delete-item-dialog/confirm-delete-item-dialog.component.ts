import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

export interface ConfirmDeleteItemData {
  itemText: string;
}

@Component({
  selector: 'app-confirm-delete-item-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './confirm-delete-item-dialog.component.html',
  styleUrls: ['./confirm-delete-item-dialog.component.scss']
})
export class ConfirmDeleteItemDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDeleteItemDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDeleteItemData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}
