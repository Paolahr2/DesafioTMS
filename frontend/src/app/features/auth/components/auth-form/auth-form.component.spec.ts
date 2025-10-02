import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { AuthFormComponent } from './auth-form.component';
import { AuthService } from '../../../../core/services/auth.service';

describe('AuthFormComponent', () => {
  let component: AuthFormComponent;
  let fixture: ComponentFixture<AuthFormComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['login', 'register', 'isAuthenticated']);
    const snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        RouterTestingModule.withRoutes([
          { path: 'boards', component: {} as any }
        ]),
        AuthFormComponent
      ],
      providers: [
        FormBuilder,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AuthFormComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
    snackBar = TestBed.inject(MatSnackBar) as jasmine.SpyObj<MatSnackBar>;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize forms with required fields', () => {
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('emailOrUsername')).toBeDefined();
    expect(component.loginForm.get('password')).toBeDefined();

    expect(component.registerForm).toBeDefined();
    expect(component.registerForm.get('username')).toBeDefined();
    expect(component.registerForm.get('firstName')).toBeDefined();
    expect(component.registerForm.get('lastName')).toBeDefined();
    expect(component.registerForm.get('email')).toBeDefined();
    expect(component.registerForm.get('password')).toBeDefined();
    expect(component.registerForm.get('confirmPassword')).toBeDefined();
  });

  it('should initialize with default values', () => {
    expect(component.loginErrors).toEqual([]);
    expect(component.registerErrors).toEqual([]);
    expect(component.isLoadingLogin).toBe(false);
    expect(component.isLoadingRegister).toBe(false);
    expect(component.submittedLogin).toBe(false);
    expect(component.submittedRegister).toBe(false);
    expect(component.showPassword).toBe(false);
    expect(component.showConfirmPassword).toBe(false);
    expect(component.selectedTabIndex).toBe(0);
  });

  describe('Tab Switching', () => {
    it('should switch to register tab', () => {
      component.switchToRegister();

      expect(component.selectedTabIndex).toBe(1);
    });

    it('should switch to login tab', () => {
      component.selectedTabIndex = 1;

      component.switchToLogin();

      expect(component.selectedTabIndex).toBe(0);
    });
  });

  describe('Login Form Validation', () => {
    it('should be invalid when login form is empty', () => {
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be invalid when emailOrUsername is empty', () => {
      component.loginForm.patchValue({
        emailOrUsername: '',
        password: 'password123'
      });
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be invalid when password is empty', () => {
      component.loginForm.patchValue({
        emailOrUsername: 'test@example.com',
        password: ''
      });
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be valid when login form is filled', () => {
      component.loginForm.patchValue({
        emailOrUsername: 'test@example.com',
        password: 'password123'
      });
      expect(component.loginForm.valid).toBe(true);
    });
  });

  describe('Register Form Validation', () => {
    it('should be invalid when register form is empty', () => {
      expect(component.registerForm.valid).toBe(false);
    });

    it('should be invalid when username is empty', () => {
      component.registerForm.patchValue({
        username: '',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
    });

    it('should be invalid when username is too short', () => {
      component.registerForm.patchValue({
        username: 'ab',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(false);
    });

    it('should be invalid when passwords do not match', () => {
      component.registerForm.patchValue({
        username: 'johndoe',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'differentpassword'
      });
      expect(component.registerForm.valid).toBe(false);
      expect(component.registerForm.get('confirmPassword')?.errors).toEqual(jasmine.objectContaining({ passwordMismatch: true }));
    });

    it('should be valid when register form is filled correctly', () => {
      component.registerForm.patchValue({
        username: 'johndoe',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        password: 'password123',
        confirmPassword: 'password123'
      });
      expect(component.registerForm.valid).toBe(true);
    });
  });

  describe('onLogin', () => {
    it('should not submit when login form is invalid', () => {
      const emailControl = component.loginForm.get('emailOrUsername');

      component.onLogin();

      expect(component.submittedLogin).toBe(true);
      expect(emailControl?.touched).toBe(true);
      expect(authService.login).not.toHaveBeenCalled();
    });

    it('should submit login when form is valid', fakeAsync(() => {
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      const mockUser = {
        id: 'user1',
        username: 'testuser',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: 'User',
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      };
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();
      tick();

      expect(authService.login).toHaveBeenCalledWith(loginData);
      expect(component.isLoadingLogin).toBe(false);
    }));

    it('should show success snackbar and navigate on successful login', fakeAsync(() => {
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      const mockUser = {
        id: 'user1',
        username: 'testuser',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: 'User',
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      };
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();
      tick();

      expect(snackBar.open).toHaveBeenCalledTimes(1);
      expect(router.url).toBe('/boards');
      expect(component.isLoadingLogin).toBe(false);
    }));

    it('should handle login error', fakeAsync(() => {
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(throwError(() => ({ error: { message: 'Invalid credentials' } })));

      component.onLogin();
      tick();

      expect(component.loginErrors).toEqual(['Invalid credentials']);
      expect(component.isLoadingLogin).toBe(false);
    }));

    it('should clear login errors on successful login', fakeAsync(() => {
      component.loginErrors = ['Previous error'];
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      const mockUser = {
        id: 'user1',
        username: 'testuser',
        email: 'test@example.com',
        firstName: 'Test',
        lastName: 'User',
        role: 'User',
        isActive: true,
        createdAt: new Date(),
        updatedAt: new Date()
      };
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();
      tick();

      expect(component.loginErrors).toEqual([]);
    }));
  });

  describe('onRegister', () => {
    it('should not submit when register form is invalid', () => {
      const usernameControl = component.registerForm.get('username');

      component.onRegister();

      expect(component.submittedRegister).toBe(true);
      expect(usernameControl?.touched).toBe(true);
      expect(authService.register).not.toHaveBeenCalled();
    });

    it('should submit register when form is valid', fakeAsync(() => {
      const registerData = {
        username: 'johndoe',
        firstName: 'John',
        lastName: 'Doe',
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
      expect(component.isLoadingRegister).toBe(false);
    }));

    it('should show success snackbar and switch to login tab on successful registration', fakeAsync(() => {
      const registerData = {
        username: 'johndoe',
        firstName: 'John',
        lastName: 'Doe',
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
      expect(component.selectedTabIndex).toBe(0);
      expect(component.registerForm.pristine).toBe(true);
      expect(component.isLoadingRegister).toBe(false);
    }));

    it('should handle register error', fakeAsync(() => {
      const registerData = {
        username: 'johndoe',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
        password: 'password123'
      };
      component.registerForm.patchValue({
        ...registerData,
        confirmPassword: 'password123'
      });
      authService.register.and.returnValue(throwError(() => ({ error: { message: 'Username already exists' } })));

      component.onRegister();
      tick();

      expect(component.registerErrors).toEqual(['Username already exists']);
      expect(component.isLoadingRegister).toBe(false);
    }));

    it('should clear register errors on successful registration', fakeAsync(() => {
      component.registerErrors = ['Previous error'];
      const registerData = {
        username: 'johndoe',
        firstName: 'John',
        lastName: 'Doe',
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
  });
});