import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-task-due-date-dialog',
  standalone: true,
  imports: [
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="p-6">
      <h2 class="text-xl font-bold mb-4">Establecer fecha límite</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <mat-form-field appearance="fill" class="w-full">
          <mat-label>Fecha límite</mat-label>
          <input matInput [matDatepicker]="picker" formControlName="dueDate">
          <mat-hint>DD/MM/YYYY</mat-hint>
          <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
          <mat-datepicker #picker></mat-datepicker>
        </mat-form-field>
        <div class="flex justify-end gap-3 mt-4">
          <button mat-button type="button" (click)="dialogRef.close()">Cancelar</button>
          <button mat-raised-button color="primary" type="submit" [disabled]="!form.valid">Guardar</button>
        </div>
      </form>
    </div>
  `
})
export class TaskDueDateDialogComponent {
  form: FormGroup;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { currentDate: Date | null },
    public dialogRef: MatDialogRef<TaskDueDateDialogComponent>,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      dueDate: [data.currentDate, Validators.required]
    });
  }

  submit() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value.dueDate);
    }
  }
}