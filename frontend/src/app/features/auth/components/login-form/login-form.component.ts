import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    RouterModule
  ],
  templateUrl: './login-form.component.html',
  styleUrls: ['./login-form.component.scss']
})
export class LoginFormComponent implements OnInit {
  loginForm: FormGroup;
  loginErrors: string[] = [];
  isLoading = false;
  submitted = false;
  showPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      emailOrUsername: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit() {
    // Component initialization
  }

  onLogin() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.loginErrors = [];
      this.submitted = false;

      const loginData = {
        emailOrUsername: this.loginForm.value.emailOrUsername,
        password: this.loginForm.value.password
      };

      this.authService.login(loginData).subscribe({
        next: (response) => {
          console.log('Login exitoso:', response);
          this.isLoading = false;
          this.snackBar.open('¡Inicio de sesión exitoso! Bienvenido de vuelta.', 'Cerrar', { duration: 3000 });
          
          // Asegurarse de que el usuario está autenticado antes de navegar
          if (this.authService.isAuthenticated()) {
            console.log('Usuario autenticado, navegando a /boards');
            this.router.navigate(['/boards']).then(
              success => console.log('Navegación exitosa:', success),
              error => console.error('Error en navegación:', error)
            );
          } else {
            console.error('Error: Usuario no autenticado después del login');
            this.loginErrors = ['Error de autenticación. Por favor, intenta nuevamente.'];
          }
        },
        error: (error) => {
          this.isLoading = false;
          if (error.error?.errors) {
            this.loginErrors = error.error.errors;
          } else if (error.error?.message) {
            this.loginErrors = [error.error.message];
          } else {
            this.loginErrors = ['Error al iniciar sesión. Verifica tus credenciales.'];
          }
        }
      });
    } else {
      this.submitted = true;
      this.markFormGroupTouched(this.loginForm);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }
}