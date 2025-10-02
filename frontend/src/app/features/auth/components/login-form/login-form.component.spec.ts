import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';

import { LoginFormComponent } from './login-form.component';
import { AuthService } from '../../../../core/services/auth.service';

describe('LoginFormComponent', () => {
  let component: LoginFormComponent;
  let fixture: ComponentFixture<LoginFormComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;
  let snackBar: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['login', 'isAuthenticated']);
    const routerSpy = jasmine.createSpyObj('Router', ['navigate']);
    const snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      imports: [
        ReactiveFormsModule,
        RouterTestingModule.withRoutes([
          { path: 'boards', component: {} as any }
        ]),
        LoginFormComponent
      ],
      providers: [
        FormBuilder,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: MatSnackBar, useValue: snackBarSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginFormComponent);
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
    expect(component.loginForm).toBeDefined();
    expect(component.loginForm.get('emailOrUsername')).toBeDefined();
    expect(component.loginForm.get('password')).toBeDefined();
  });

  it('should initialize with default values', () => {
    expect(component.isLoading).toBe(false);
    expect(component.submitted).toBe(false);
    expect(component.showPassword).toBe(false);
    expect(component.loginErrors).toEqual([]);
  });

  describe('Form Validation', () => {
    it('should be invalid when form is empty', () => {
      expect(component.loginForm.valid).toBe(false);
    });

    it('should be invalid when emailOrUsername is empty', () => {
      component.loginForm.patchValue({ emailOrUsername: '', password: 'password123' });
      expect(component.loginForm.valid).toBe(false);
      expect(component.loginForm.get('emailOrUsername')?.valid).toBe(false);
    });

    it('should be invalid when password is empty', () => {
      component.loginForm.patchValue({ emailOrUsername: 'test@example.com', password: '' });
      expect(component.loginForm.valid).toBe(false);
      expect(component.loginForm.get('password')?.valid).toBe(false);
    });

    it('should be valid when all required fields are filled', () => {
      component.loginForm.patchValue({
        emailOrUsername: 'test@example.com',
        password: 'password123'
      });
      expect(component.loginForm.valid).toBe(true);
    });
  });

  describe('onLogin', () => {
    it('should not submit when form is invalid', () => {
      component.onLogin();
      expect(component.submitted).toBe(true);
      expect(authService.login).not.toHaveBeenCalled();
    });

    it('should mark form controls as touched when form is invalid', () => {
      const emailControl = component.loginForm.get('emailOrUsername');
      const passwordControl = component.loginForm.get('password');

      component.onLogin();

      expect(emailControl?.touched).toBe(true);
      expect(passwordControl?.touched).toBe(true);
    });

    it('should submit login when form is valid', () => {
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
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();

      expect(authService.login).toHaveBeenCalledWith(loginData);
      expect(component.isLoading).toBe(false);
    });

    it('should show success snackbar and navigate on successful login', fakeAsync(() => {
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
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();
      tick();

      expect(snackBar.open).toHaveBeenCalledTimes(1);
      expect(router.url).toBe('/boards');
      expect(component.isLoading).toBe(false);
    }));

    it('should handle login error with error.errors', () => {
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      const errorResponse = { error: { errors: ['Invalid credentials', 'Account locked'] } };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(throwError(() => errorResponse));

      component.onLogin();

      expect(component.loginErrors).toEqual(['Invalid credentials', 'Account locked']);
      expect(component.isLoading).toBe(false);
    });

    it('should handle login error with error.message', () => {
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      const errorResponse = { error: { message: 'Invalid credentials' } };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(throwError(() => errorResponse));

      component.onLogin();

      expect(component.loginErrors).toEqual(['Invalid credentials']);
      expect(component.isLoading).toBe(false);
    });

    it('should handle login error with default message', () => {
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      const errorResponse = { error: {} };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(throwError(() => errorResponse));

      component.onLogin();

      expect(component.loginErrors).toEqual(['Error al iniciar sesión. Verifica tus credenciales.']);
      expect(component.isLoading).toBe(false);
    });

    it('should set loading state during login', () => {
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
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();

      expect(component.isLoading).toBe(false); // Should be false after completion
    });

    it('should clear errors on successful login', () => {
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
      component.loginErrors = ['Previous error'];
      const loginData = { emailOrUsername: 'test@example.com', password: 'password123' };
      component.loginForm.patchValue(loginData);
      authService.login.and.returnValue(of({ success: true, message: 'Login successful', token: 'token', user: mockUser }));
      authService.isAuthenticated.and.returnValue(true);

      component.onLogin();

      expect(component.loginErrors).toEqual([]);
    });
  });

  describe('markFormGroupTouched', () => {
    it('should mark all form controls as touched', () => {
      const emailControl = component.loginForm.get('emailOrUsername');
      const passwordControl = component.loginForm.get('password');

      component['markFormGroupTouched'](component.loginForm);

      expect(emailControl?.touched).toBe(true);
      expect(passwordControl?.touched).toBe(true);
    });
  });
});
