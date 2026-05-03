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

  /**
   * Signal holding the currently authenticated user, or `null` when logged out.
   * Persisted to and restored from `localStorage` on page reload.
   */
  currentUser = signal<AuthResponse | null>(this.loadUser());

  constructor(private http: HttpClient, private router: Router) {}

  /** Restores the authenticated user from localStorage on service initialisation. */
  private loadUser(): AuthResponse | null {
    const raw = localStorage.getItem('user');
    return raw ? JSON.parse(raw) : null;
  }

  /**
   * Registers a new user account and persists the returned auth response.
   * @param req - The registration payload.
   * @returns An observable that emits the {@link AuthResponse} on success.
   */
  register(req: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.base}/register`, req).pipe(
      tap(res => this.persist(res))
    );
  }

  /**
   * Authenticates an existing user and persists the returned auth response.
   * @param req - The login credentials.
   * @returns An observable that emits the {@link AuthResponse} on success.
   */
  login(req: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.base}/login`, req).pipe(
      tap(res => this.persist(res))
    );
  }

  /**
   * Sends a password reset OTP to the user's email address.
   * @param req - The forgot-password request containing the email.
   * @returns An observable that emits a confirmation message.
   */
  forgotPassword(req: ForgotPasswordRequest) {
    return this.http.post<{ message: string }>(`${this.base}/forgot-password`, req);
  }

  /**
   * Resets the user's password using a valid OTP.
   * @param req - The reset-password request containing email, OTP, and new password.
   * @returns An observable that emits a confirmation message.
   */
  resetPassword(req: ResetPasswordRequest) {
    return this.http.post<{ message: string }>(`${this.base}/reset-password`, req);
  }

  /**
   * Exchanges the current refresh token for a new access/refresh token pair.
   * Updates the persisted user with the new tokens.
   * @returns An observable that emits the new tokens, or `undefined` if not logged in.
   */
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

  /**
   * Logs out the current user by invalidating the refresh token server-side,
   * clearing localStorage, and navigating to the login page.
   */
  logout() {
    const user = this.currentUser();
    if (user) {
      this.http.post(`${this.base}/logout`, { email: user.email }).subscribe();
    }
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  /** Persists the auth response to localStorage and updates the `currentUser` signal. */
  private persist(user: AuthResponse) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  /** The current JWT access token, or `null` if not authenticated. */
  get token(): string | null {
    return this.currentUser()?.accessToken ?? null;
  }

  /** The current user's role (e.g., "Customer", "Admin"), or `null` if not authenticated. */
  get role(): string | null {
    return this.currentUser()?.role ?? null;
  }

  /**
   * Returns `true` if a user is currently authenticated.
   */
  isLoggedIn(): boolean {
    return !!this.currentUser();
  }
}
