import { Routes } from "@angular/router";
import { AuthGuard } from "./core/guards/auth.guard";

export const routes: Routes = [
  { path: "", redirectTo: "/landing", pathMatch: "full" },
  {
    path: "login",
    loadComponent: () => import('./features/auth/components/login-form/login-form.component').then(m => m.LoginFormComponent)
  },
  {
    path: "register",
    loadComponent: () => import('./features/auth/components/register-form/register-form.component').then(m => m.RegisterFormComponent)
  },
  {
    path: "boards",
    loadComponent: () => import('./shared/components/boards/boards.component').then(m => m.BoardsComponent),
    canActivate: [AuthGuard]
  },
  {
    path: "boards/:id",
    loadComponent: () => import('./features/boards/components').then(m => m.BoardDetailComponent),
    canActivate: [AuthGuard]
  },
  {
    path: "collaboration",
    loadComponent: () => import('./features/collaboration/components').then(m => m.PendingInvitationsComponent),
    canActivate: [AuthGuard]
  },
  {
    path: "landing",
    loadComponent: () => import('./shared/components/landing').then(m => m.LandingComponent)
  },
  { path: "**", redirectTo: "/login" }
];