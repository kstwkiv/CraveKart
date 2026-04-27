import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  AuthResponse, LoginRequest, RegisterRequest,
  ForgotPasswordRequest, ResetPasswordRequest, RefreshTokenRequest
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly base = `${environment.apiUrl}/auth`;
  currentUser = signal<AuthResponse | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  }

  register(req: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.base}/register`, req).pipe(
      tap(res => this.persist(res))
    );
  }

  login(req: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.base}/login`, req).pipe(
      tap(res => this.persist(res))
    );
  }

  forgotPassword(req: ForgotPasswordRequest) {
    return this.http.post<{ message: string }>(`${this.base}/forgot-password`, req);
  }

  resetPassword(req: ResetPasswordRequest) {
    return this.http.post<{ message: string }>(`${this.base}/reset-password`, req);
  }

  refreshToken() {
    const user = this.currentUser();
    if (!user) return;
    const req: RefreshTokenRequest = { email: user.email, refreshToken: user.refreshToken };
    return this.http.post<{ accessToken: string; refreshToken: string }>(`${this.base}/refresh-token`, req).pipe(
      tap(res => {
        const updated = { ...user, accessToken: res.accessToken, refreshToken: res.refreshToken };
        this.persist(updated);
      })
    );
  }

  logout() {
    const user = this.currentUser();
    if (user) {
      this.http.post(`${this.base}/logout`, { email: user.email }).subscribe();
    }
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  private persist(user: AuthResponse) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  get token(): string | null {
    return this.currentUser()?.accessToken ?? null;
  }

  get role(): string | null {
    return this.currentUser()?.role ?? null;
  }

  isLoggedIn(): boolean {
    return !!this.currentUser();
  }
}
