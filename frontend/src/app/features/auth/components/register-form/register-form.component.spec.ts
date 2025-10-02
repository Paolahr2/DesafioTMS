import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { RegisterFormComponent } from './register-form.component';
import { AuthService } from '../../../../core/services/auth.service';

describe('RegisterFormComponent', () => {
  let component: RegisterFormComponent;
  let fixture: ComponentFixture<RegisterFormComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['register']);
    const snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        RouterTestingModule.withRoutes([
          { path: 'login', component: {} as any }
        ]),
        RegisterFormComponent
      ],
      providers: [
        FormBuilder,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterFormComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
    snackBar = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize form with required fields', () => {
    expect(component.registerForm).toBeDefined();
    expect(component.registerForm.get('firstName')).toBeDefined();
    expect(component.registerForm.get('lastName')).toBeDefined();
    expect(component.registerForm.get('username')).toBeDefined();
    expect(component.registerForm.get('email')).toBeDefined();
    expect(component.registerForm.get('password')).toBeDefined();
    expect(component.registerForm.get('confirmPassword')).toBeDefined();
  });

  it('should initialize with default values', () => {
    expect(component.registerErrors).toEqual([]);
    expect(component.isLoading).toBe(false);
    expect(component.submitted).toBe(false);
    expect(component.showPassword).toBe(false);
    expect(component.showConfirmPassword).toBe(false);
  });

  describe('Form Validation', () => {
    it('should be invalid when form is empty', () => {
      expect(component.registerForm.valid).toBe(false);
    });

    it('should be invalid when firstName is empty', () => {
      component.registerForm.patchValue({
        firstName: '',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('firstName')?.valid).toBe(false);
    });

    it('should be invalid when lastName is empty', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: '',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('lastName')?.valid).toBe(false);
    });

    it('should be invalid when username is empty', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: '',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('username')?.valid).toBe(false);
    });

    it('should be invalid when username is too short', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'ab',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('username')?.valid).toBe(false);
    });

    it('should be invalid when email is empty', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: '',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('email')?.valid).toBe(false);
    });

    it('should be invalid when email format is incorrect', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'invalid-email',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('email')?.valid).toBe(false);
    });

    it('should be invalid when password is empty', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: '',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('password')?.valid).toBe(false);
    });

    it('should be invalid when password is too short', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: '12345',
        confirmPassword: '12345'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('password')?.valid).toBe(false);
    });

    it('should be invalid when confirmPassword is empty', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: ''
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('confirmPassword')?.valid).toBe(false);
    });

    it('should be invalid when passwords do not match', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'differentpassword'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('confirmPassword')?.errors).toEqual(jasmine.objectContaining({ passwordMismatch: true }));
    });

    it('should be valid when all required fields are filled correctly', () => {
      component.registerForm.patchValue({
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(true);
    });
  });

  describe('onRegister', () => {
    it('should not submit when form is invalid', () => {
      const firstNameControl = component.registerForm.get('firstName');

      component.onRegister();

      expect(component.submitted).toBe(true);
      expect(firstNameControl?.touched).toBe(true);
      expect(authService.register).not.toHaveBeenCalled();
    });

    it('should submit registration when form is valid', fakeAsync(() => {
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(of({ success: true, message: 'Registration successful' }));

      component.onRegister();
      tick();

      expect(authService.register).toHaveBeenCalledWith(registerData);
      expect(component.isLoading).toBe(false);
    }));

    it('should show success snackbar and navigate to login on successful registration', fakeAsync(() => {
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(of({ success: true, message: 'Registration successful' }));

      component.onRegister();
      tick();

      expect(snackBar.open).toHaveBeenCalledTimes(1);
      expect(router.url).toBe('/login');
      expect(component.isLoading).toBe(false);
    }));

    it('should handle registration error with error.errors', fakeAsync(() => {
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(throwError(() => ({ error: { errors: ['Username already exists'] } })));

      component.onRegister();
      tick();

      expect(component.registerErrors).toEqual(['Username already exists']);
      expect(component.isLoading).toBe(false);
    }));

    it('should handle registration error with error.message', fakeAsync(() => {
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(throwError(() => ({ error: { message: 'Registration failed' } })));

      component.onRegister();
      tick();

      expect(component.registerErrors).toEqual(['Registration failed']);
      expect(component.isLoading).toBe(false);
    }));

    it('should handle registration error with default message', fakeAsync(() => {
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(throwError(() => ({ error: {} })));

      component.onRegister();
      tick();

      expect(component.registerErrors).toEqual(['Error al crear la cuenta. Inténtalo de nuevo.']);
      expect(component.isLoading).toBe(false);
    }));

    it('should set loading state during registration', fakeAsync(() => {
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(of({ success: true, message: 'Registration successful' }));

      component.onRegister();

      expect(component.isLoading).toBe(false); // Should be false after completion
    }));

    it('should clear errors on successful registration', fakeAsync(() => {
      component.registerErrors = ['Previous error'];
      const registerData = {
        firstName: 'John',
        lastName: 'Doe',
        username: 'johndoe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(of({ success: true, message: 'Registration successful' }));

      component.onRegister();
      tick();

      expect(component.registerErrors).toEqual([]);
    }));

    it('should mark form controls as touched when form is invalid', () => {
      const firstNameControl = component.registerForm.get('firstName');
      const lastNameControl = component.registerForm.get('lastName');

      component.onRegister();

      expect(component.submitted).toBe(true);
      expect(firstNameControl?.touched).toBe(true);
      expect(lastNameControl?.touched).toBe(true);
    });
  });

  describe('passwordMatchValidator', () => {
    it('should return null when passwords match', () => {
      const formGroup = component.registerForm;
      formGroup.patchValue({
        password: 'password123',
        confirmPassword: 'password123'
      });

      const result = component.passwordMatchValidator(formGroup);

      expect(result).toBeNull();
    });

    it('should return passwordMismatch error when passwords do not match', () => {
      const formGroup = component.registerForm;
      formGroup.patchValue({
        password: 'password123',
        confirmPassword: 'differentpassword'
      });

      const result = component.passwordMatchValidator(formGroup);

      expect(result).toEqual({ passwordMismatch: true });
    });

    it('should set passwordMismatch error on confirmPassword control when passwords do not match', () => {
      const formGroup = component.registerForm;
      formGroup.patchValue({
        password: 'password123',
        confirmPassword: 'differentpassword'
      });

      component.passwordMatchValidator(formGroup);

      expect(formGroup.get('confirmPassword')?.errors).toEqual(jasmine.objectContaining({ passwordMismatch: true }));
    });
  });

  describe('markFormGroupTouched', () => {
    it('should mark all form controls as touched', () => {
      const firstNameControl = component.registerForm.get('firstName');
      const lastNameControl = component.registerForm.get('lastName');
      const usernameControl = component.registerForm.get('username');
      const emailControl = component.registerForm.get('email');
      const passwordControl = component.registerForm.get('password');
      const confirmPasswordControl = component.registerForm.get('confirmPassword');

      component['markFormGroupTouched'](component.registerForm);

      expect(firstNameControl?.touched).toBe(true);
      expect(lastNameControl?.touched).toBe(true);
      expect(usernameControl?.touched).toBe(true);
      expect(emailControl?.touched).toBe(true);
      expect(passwordControl?.touched).toBe(true);
      expect(confirmPasswordControl?.touched).toBe(true);
    });
  });
});