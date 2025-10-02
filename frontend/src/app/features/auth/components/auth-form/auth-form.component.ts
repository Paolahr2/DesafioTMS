import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-auth-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './auth-form.component.html',
  styleUrls: ['./auth-form.component.scss']
})
export class AuthFormComponent implements OnInit {
  loginForm: FormGroup;
  registerForm: FormGroup;
  loginErrors: string[] = [];
  registerErrors: string[] = [];
  isLoadingLogin = false;
  isLoadingRegister = false;
  submittedLogin = false;
  submittedRegister = false;
  showPassword = false;
  showConfirmPassword = false;
  selectedTabIndex = 0;

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

    this.registerForm = this.fb.group({
      username: ['', [Validators.required, Validators.minLength(3)]],
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    // Component initialization
  }

  // Custom validator for password confirmation
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  switchToRegister() {
    this.selectedTabIndex = 1;
  }

  switchToLogin() {
    this.selectedTabIndex = 0;
  }

  onLogin() {
    if (this.loginForm.valid) {
      this.isLoadingLogin = true;
      this.loginErrors = [];
      this.submittedLogin = false;

      const loginData = {
        emailOrUsername: this.loginForm.value.emailOrUsername,
        password: this.loginForm.value.password
      };

      this.authService.login(loginData).subscribe({
        next: (response) => {
          this.isLoadingLogin = false;
          this.snackBar.open('¡Inicio de sesión exitoso! Bienvenido de vuelta.', 'Cerrar', { duration: 3000 });
          // Navegar al dashboard de tableros
          this.router.navigate(['/boards']);
        },
        error: (error) => {
          this.isLoadingLogin = false;
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
      this.submittedLogin = true;
      this.markFormGroupTouched(this.loginForm);
    }
  }

  onRegister() {
    if (this.registerForm.valid) {
      this.isLoadingRegister = true;
      this.registerErrors = [];
      this.submittedRegister = false;

      const registerData = {
        username: this.registerForm.value.username,
        firstName: this.registerForm.value.firstName,
        lastName: this.registerForm.value.lastName,
        email: this.registerForm.value.email,
        password: this.registerForm.value.password
      };

      this.authService.register(registerData).subscribe({
        next: (response) => {
          this.isLoadingRegister = false;
          this.snackBar.open('¡Cuenta creada exitosamente! Ahora puedes iniciar sesión.', 'Cerrar', { duration: 4000 });
          // Cambiar a la pestaña de login
          this.switchToLogin();
          // Limpiar el formulario de registro
          this.registerForm.reset();
        },
        error: (error) => {
          this.isLoadingRegister = false;
          if (error.error?.errors) {
            this.registerErrors = error.error.errors;
          } else if (error.error?.message) {
            this.registerErrors = [error.error.message];
          } else {
            this.registerErrors = ['Error al crear la cuenta. Inténtalo de nuevo.'];
          }
        }
      });
    } else {
      this.submittedRegister = true;
      this.markFormGroupTouched(this.registerForm);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }
}


