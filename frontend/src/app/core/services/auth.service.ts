import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';

import { LoginData, RegisterData, AuthResponse, User } from '@core/models/interfaces/user.interface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    // Cargar usuario desde localStorage al inicializar (solo en el browser)
    if (isPlatformBrowser(this.platformId)) {
      const storedUser = localStorage.getItem('current_user');
      if (storedUser) {
        try {
          this.currentUserSubject.next(JSON.parse(storedUser));
        } catch (error) {
          console.error('Error parsing stored user:', error);
          localStorage.removeItem('current_user');
        }
      }
    }
  }

  login(loginData: LoginData): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}${environment.endpoints.auth}/login`, loginData)
      .pipe(
        tap((response: AuthResponse) => {
          console.log('Respuesta del servidor:', response);
          if (response.token && response.user && isPlatformBrowser(this.platformId)) {
            try {
              console.log('AuthService - Guardando token:', response.token);
              console.log('AuthService - Guardando usuario:', response.user);
              localStorage.setItem('auth_token', response.token);
              localStorage.setItem('current_user', JSON.stringify(response.user));
              this.currentUserSubject.next(response.user);
              console.log('AuthService - Datos guardados correctamente');
              console.log('AuthService - Token guardado:', localStorage.getItem('auth_token'));
              console.log('AuthService - Usuario guardado:', localStorage.getItem('current_user'));
              console.log('Usuario autenticado:', this.getCurrentUser());
            } catch (error) {
              console.error('Error al guardar datos de autenticación:', error);
              throw new Error('Error al guardar los datos de autenticación en el navegador. Por favor, inténtalo de nuevo.');
            }
          } else {
            console.error('Respuesta incompleta del servidor:', response);
            throw new Error('Respuesta de autenticación inválida');
          }
        }),
        catchError((error: any) => {
          console.error('Error durante el inicio de sesión:', error);
          if (error.status === 401) {
            return throwError(() => new Error('Credenciales incorrectas. Por favor, verifica tu email/usuario y contraseña.'));
          } else if (error.status === 403) {
            return throwError(() => new Error('Tu cuenta está bloqueada. Por favor, espera unos minutos antes de intentar de nuevo.'));
          } else if (error.status === 0) {
            return throwError(() => new Error('No se puede conectar con el servidor. Por favor, verifica tu conexión a internet.'));
          } else if (error.error?.message) {
            return throwError(() => new Error(error.error.message));
          }
          return throwError(() => new Error('Error al iniciar sesión. Por favor, inténtalo de nuevo más tarde.'));
        })
      );
  }

  register(registerData: any): Observable<AuthResponse> {
    // Los datos ya vienen separados como firstName y lastName
    const backendData = {
      firstName: registerData.firstName,
      lastName: registerData.lastName,
      username: registerData.username,
      email: registerData.email,
      password: registerData.password
    };

    return this.http.post<AuthResponse>(`${this.apiUrl}${environment.endpoints.auth}/register`, backendData)
      .pipe(
        tap((response: AuthResponse) => {
          if (response.token && response.user && isPlatformBrowser(this.platformId)) {
            localStorage.setItem('auth_token', response.token);
            localStorage.setItem('current_user', JSON.stringify(response.user));
            this.currentUserSubject.next(response.user);
          }
        }),
        catchError((error: any) => {
          console.error('Register error:', error);
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('auth_token');
      localStorage.removeItem('current_user');
    }
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  public isAuthenticated(): boolean {
    if (!isPlatformBrowser(this.platformId)) {
      return false;
    }
    const token = localStorage.getItem('auth_token');
    const user = localStorage.getItem('current_user');
    return !!token && !!user;
  }

  getToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }
    return localStorage.getItem('auth_token');
  }

  // Método para refrescar el token (opcional, para implementar más adelante)
  refreshToken(): Observable<AuthResponse> {
    const token = this.getToken();
    return this.http.post<AuthResponse>(`${this.apiUrl}/auth/refresh`, { token })
      .pipe(
        tap(response => {
          if (response.token && isPlatformBrowser(this.platformId)) {
            localStorage.setItem('auth_token', response.token);
            if (response.user) {
              localStorage.setItem('current_user', JSON.stringify(response.user));
              this.currentUserSubject.next(response.user);
            }
          }
        }),
        catchError(error => {
          console.error('Token refresh error:', error);
          this.logout(); // Si no se puede refrescar, cerrar sesión
          return throwError(() => error);
        })
      );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}${environment.endpoints.auth}/forgot-password`, { email })
      .pipe(
        catchError((error: any) => {
          console.error('Forgot password error:', error);
          return throwError(() => error);
        })
      );
  }

  resetPassword(token: string, email: string, newPassword: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}${environment.endpoints.auth}/reset-password`, { token, email, newPassword })
      .pipe(
        catchError((error: any) => {
          console.error('Reset password error:', error);
          return throwError(() => error);
        })
      );
  }
}



