import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { CollaborationService } from '../../../core/services/collaboration.service';
import { InviteUserToBoardRequest } from '@core/models/entities';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-invite-user-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatFormFieldModule,
    MatIconModule,
    MatDividerModule,
    MatDialogModule,
    MatSnackBarModule
  ],
  templateUrl: './invite-user-dialog.component.html',
  styleUrls: ['./invite-user-dialog.component.scss']
})
export class InviteUserDialogComponent {
  inviteForm: FormGroup;
  loading = false;
  sending = false;

  constructor(
    private dialogRef: MatDialogRef<InviteUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { boardId: string },
    private formBuilder: FormBuilder,
    private collaborationService: CollaborationService,
    private snackBar: MatSnackBar
  ) {
    this.inviteForm = this.formBuilder.group({
      inviteMethod: ['username'], // Cambiado a username por defecto
      Email: [''],
      Username: ['', Validators.required], // Username requerido por defecto
      Role: ['Editor', Validators.required],
      Message: [''],
      ExpiresAt: ['']
    });

    // Configurar validadores dinámicos
    this.inviteForm.get('inviteMethod')?.valueChanges.subscribe(method => {
      const emailControl = this.inviteForm.get('Email');
      const usernameControl = this.inviteForm.get('Username');

      if (method === 'email') {
        emailControl?.setValidators([Validators.required, Validators.email]);
        usernameControl?.clearValidators();
      } else {
        usernameControl?.setValidators([Validators.required]);
        emailControl?.clearValidators();
      }

      emailControl?.updateValueAndValidity();
      usernameControl?.updateValueAndValidity();
    });
  }

  showError(controlName: string): boolean {
    const control = this.inviteForm.get(controlName);
    return control ? control.invalid && (control.dirty || control.touched) : false;
  }

  getErrorMessage(controlName: string): string {
    const control = this.inviteForm.get(controlName);
    if (!control) return '';
    
    if (control.hasError('required')) {
      return 'Este campo es requerido';
    }
    if (control.hasError('email')) {
      return 'Por favor ingrese un correo electrónico válido';
    }
    return '';
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onInvite(): void {
    if (this.inviteForm.valid) {
      this.sending = true;
      this.onSubmit();
    }
  }

  onSubmit(): void {
    if (this.inviteForm.valid) {
      this.loading = true;
      const formValue = this.inviteForm.value;

      // Crear el request basado en el método seleccionado
      const request: InviteUserToBoardRequest = {
        Role: formValue.Role,
        Message: formValue.Message,
        ExpiresAt: formValue.ExpiresAt
      };

      if (formValue.inviteMethod === 'email') {
        request.Email = formValue.Email;
      } else {
        request.Username = formValue.Username;
      }

      this.collaborationService.inviteUserToBoard(this.data.boardId, request).subscribe({
        next: (result) => {
          this.snackBar.open('Invitación enviada exitosamente', 'Cerrar', {
            duration: 3000
          });
          this.sending = false;
          this.dialogRef.close(result);
        },
        error: (error) => {
          console.error('Error sending invitation:', error);
          this.sending = false;
          let errorMessage = 'Error al enviar la invitación';

          if (error.error?.message) {
            errorMessage = error.error.message;
          } else if (error.status === 404) {
            errorMessage = 'Usuario no encontrado';
          } else if (error.status === 409) {
            errorMessage = 'El usuario ya es colaborador de este tablero';
          }

          this.snackBar.open(errorMessage, 'Cerrar', {
            duration: 5000
          });
          this.loading = false;
        }
      });
    } else {
      this.inviteForm.markAllAsTouched();
    }
  }

  onClose(): void {
    this.dialogRef.close();
  }
}
