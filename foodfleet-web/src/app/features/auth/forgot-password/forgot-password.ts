import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="centered-page">
      <div class="auth-card" style="width:100%;max-width:440px">
        <div class="auth-header">
          <h2>Reset password 🔑</h2>
        </div>

        <ng-container *ngIf="!otpSent; else resetForm">
          <p class="subtitle">Enter your email to receive an OTP</p>
          <form [formGroup]="emailForm" (ngSubmit)="sendOtp()">
            <div class="field">
              <label>Email</label>
              <input type="email" formControlName="email" placeholder="aaaa@example.com" />
            </div>
            <div class="error" *ngIf="error">{{ error }}</div>
            <button type="submit" [disabled]="loading || emailForm.invalid" class="btn-primary">
              {{ loading ? 'Sending...' : 'Send OTP' }}
            </button>
          </form>
        </ng-container>

        <ng-template #resetForm>
          <p class="subtitle">Enter the OTP sent to your email</p>
          <form [formGroup]="resetFormGroup" (ngSubmit)="resetPassword()">
            <div class="field">
              <label>OTP</label>
              <input formControlName="otp" placeholder="123456" />
            </div>
            <div class="field">
              <label>New Password</label>
              <input type="password" formControlName="newPassword" placeholder="••••••••" />
            </div>
            <div class="error" *ngIf="error">{{ error }}</div>
            <div class="success" *ngIf="success">{{ success }}</div>
            <button type="submit" [disabled]="loading || resetFormGroup.invalid" class="btn-primary">
              {{ loading ? 'Resetting...' : 'Reset Password' }}
            </button>
          </form>
        </ng-template>

        <div class="links"><a routerLink="/auth/login">Back to login</a></div>
      </div>
    </div>
  `,
  styleUrl: '../login/login.scss',
  styles: [`
    .success { color: var(--accent); font-size: 0.875rem; margin-bottom: 0.75rem; }
    .centered-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 2rem 1rem; }
    .auth-card { background: rgba(255,255,255,0.06); backdrop-filter: blur(20px); border: 1px solid rgba(123,63,181,0.3); border-radius: 16px; padding: 2.5rem; box-shadow: 0 8px 40px rgba(0,0,0,0.4); }
    h2 { font-size: 1.6rem; font-weight: 800; color: var(--text-primary); margin-bottom: 0.5rem; }
    .subtitle { color: var(--text-muted); font-size: 0.9rem; margin-bottom: 1.5rem; }
  `]
})
export class ForgotPasswordComponent {
  emailForm: FormGroup;
  resetFormGroup: FormGroup;
  otpSent = false;
  loading = false;
  error = '';
  success = '';

  constructor(private fb: FormBuilder, private auth: AuthService) {
    this.emailForm = this.fb.group({ email: ['', [Validators.required, Validators.email]] });
    this.resetFormGroup = this.fb.group({
      otp: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  sendOtp() {
    if (this.emailForm.invalid) return;
    this.loading = true;
    this.auth.forgotPassword(this.emailForm.value).subscribe({
      next: () => { this.otpSent = true; this.loading = false; },
      error: () => { this.error = 'Something went wrong'; this.loading = false; }
    });
  }

  resetPassword() {
    if (this.resetFormGroup.invalid) return;
    this.loading = true;
    this.auth.resetPassword({
      email: this.emailForm.value.email,
      ...this.resetFormGroup.value
    }).subscribe({
      next: () => { this.success = 'Password reset! You can now log in.'; this.loading = false; },
      error: (err) => { this.error = err.error || 'Invalid OTP'; this.loading = false; }
    });
  }
}
