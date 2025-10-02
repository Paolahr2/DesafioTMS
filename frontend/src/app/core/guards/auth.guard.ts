import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    const isAuth = this.authService.isAuthenticated();
    console.log('AuthGuard - isAuthenticated:', isAuth);
    console.log('AuthGuard - token:', localStorage.getItem('auth_token'));
    console.log('AuthGuard - user:', localStorage.getItem('current_user'));
    if (!isAuth) {
      console.log('AuthGuard - Redirigiendo a login por falta de autenticación');
      this.router.navigate(['/login']);
    }
    return isAuth;
  }
}


