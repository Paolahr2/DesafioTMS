import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PLATFORM_ID } from '@angular/core';
import { AuthService } from './auth.service';
import { LoginData, RegisterData, AuthResponse, User } from '@core/models/interfaces/user.interface';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const mockUser: User = {
    id: 'user123',
    username: 'testuser',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    role: 'User',
    isActive: true,
    createdAt: new Date(),
    updatedAt: new Date()
  };

  const mockAuthResponse: AuthResponse = {
    success: true,
    message: 'Login successful',
    token: 'mock-jwt-token',
    user: mockUser
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: PLATFORM_ID, useValue: 'browser' }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    
    // Clear localStorage before each test
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('login', () => {
    it('should login successfully and store token and user', (done) => {
      const loginData: LoginData = {
        emailOrUsername: 'test@example.com',
        password: 'password123'
      };

      service.login(loginData).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(localStorage.getItem('auth_token')).toBe('mock-jwt-token');
        expect(localStorage.getItem('current_user')).toBe(JSON.stringify(mockUser));
        
        service.currentUser$.subscribe(user => {
          expect(user).toEqual(mockUser);
          done();
        });
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.auth}/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(loginData);
      req.flush(mockAuthResponse);
    });

    it('should handle login error', (done) => {
      const loginData: LoginData = {
        emailOrUsername: 'test@example.com',
        password: 'wrong-password'
      };

      service.login(loginData).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          // El catchError del servicio transforma HttpErrorResponse en Error con mensaje específico
          expect(error).toBeInstanceOf(Error);
          expect(error.message).toBe('Credenciales incorrectas. Por favor, verifica tu email/usuario y contraseña.');
          expect(localStorage.getItem('auth_token')).toBeNull();
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.auth}/login`);
      req.flush({ message: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });
    });
  });

  describe('register', () => {
    it('should register successfully', (done) => {
      const registerData: RegisterData = {
        username: 'newuser',
        email: 'newuser@example.com',
        password: 'password123',
        firstName: 'New',
        lastName: 'User'
      };

      service.register(registerData).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        done();
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.auth}/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(registerData);
      req.flush(mockAuthResponse);
    });

    it('should handle registration error', (done) => {
      const registerData: RegisterData = {
        username: 'existinguser',
        email: 'existing@example.com',
        password: 'password123',
        firstName: 'Existing',
        lastName: 'User'
      };

      service.register(registerData).subscribe({
        next: () => fail('should have failed'),
        error: (error) => {
          expect(error.status).toBe(400);
          done();
        }
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.auth}/register`);
      req.flush({ message: 'User already exists' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('logout', () => {
    it('should clear authentication data', () => {
      // Setup: login first
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.setItem('current_user', JSON.stringify(mockUser));

      service.logout();

      expect(localStorage.getItem('auth_token')).toBeNull();
      expect(localStorage.getItem('current_user')).toBeNull();
      
      service.currentUser$.subscribe(user => {
        expect(user).toBeNull();
      });
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token and user exist', () => {
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.setItem('current_user', JSON.stringify(mockUser));
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when token does not exist', () => {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('current_user');
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should return false when only token exists', () => {
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.removeItem('current_user');
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('getToken', () => {
    it('should return token from localStorage', () => {
      localStorage.setItem('auth_token', 'mock-token');
      expect(service.getToken()).toBe('mock-token');
    });

    it('should return null when no token exists', () => {
      localStorage.removeItem('auth_token');
      expect(service.getToken()).toBeNull();
    });
  });

  describe('currentUser$ observable', () => {
    it('should emit current user when logged in', (done) => {
      service.login({
        emailOrUsername: 'test@example.com',
        password: 'password123'
      }).subscribe(() => {
        service.currentUser$.subscribe(user => {
          expect(user).toEqual(mockUser);
          done();
        });
      });

      const req = httpMock.expectOne(`${environment.apiUrl}${environment.endpoints.auth}/login`);
      req.flush(mockAuthResponse);
    });

    it('should emit null after logout', (done) => {
      // Login first
      localStorage.setItem('auth_token', 'mock-token');
      localStorage.setItem('current_user', JSON.stringify(mockUser));

      service.logout();

      service.currentUser$.subscribe(user => {
        expect(user).toBeNull();
        done();
      });
    });
  });
});
