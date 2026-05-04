// 'import' — ES module keyword; pulls named exports from another module into this file's scope
// 'Component' — Angular decorator factory; marks a class as a component and attaches template/style metadata
import { Component } from '@angular/core';
// 'CommonModule' — provides *ngIf, *ngFor, async pipe, and other structural directives
import { CommonModule } from '@angular/common';
// 'FormBuilder' — Angular service that creates FormGroup/FormControl instances with less boilerplate
// 'FormGroup'   — represents a group of form controls; tracks the combined validity and value
// 'ReactiveFormsModule' — Angular module that enables reactive (model-driven) forms in templates
// 'Validators'  — collection of built-in validator functions (required, email, minLength, etc.)
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// 'Router'     — Angular service for programmatic navigation between routes
// 'RouterLink' — directive that turns an anchor into a client-side navigation link
import { Router, RouterLink } from '@angular/router';
// 'AuthService' — application service that wraps HTTP calls to the Identity API
import { AuthService } from '../../../core/services/auth.service';

/**
 * Login page component.
 * Presents a two-panel layout with a branded visual on the left and a
 * reactive login form on the right. On success, redirects the user to
 * the role-appropriate dashboard.
 */
// '@Component' — class decorator; Angular reads this metadata to compile the template and wire up DI
@Component({
  selector: 'app-login',   // CSS selector used in templates/router to render this component: <app-login>
  standalone: true,        // standalone: true — no NgModule needed; the component manages its own imports
  imports: [CommonModule, ReactiveFormsModule, RouterLink], // 'imports' — Angular dependencies for this component's template
  template: `
    <div class="auth-page">
      <div class="auth-visual">
        <div class="visual-logo">🍽️</div>
        <h2>Good to see you again!</h2>
        <p>Your favourite restaurants are waiting. Let's get you fed.</p>
        <div class="features">
          <div class="feature"><span class="feat-icon">🚀</span> Fast delivery, right to your door</div>
          <div class="feature"><span class="feat-icon">🥗</span> Hundreds of restaurants to explore</div>
          <div class="feature"><span class="feat-icon">📍</span> Track your order in real-time</div>
        </div>
      </div>
      <div class="auth-form-side">
        <div class="auth-card">
          <div class="auth-header">
            <div class="brand-mark">🌿 CraveKart</div>
            <h2>Sign In</h2>
            <p class="subtitle">Welcome back — let's find your next meal</p>
          </div>

          <form [formGroup]="form" (ngSubmit)="submit()"> 
            <div class="field">
              <label>Email address</label>
              <input type="email" formControlName="email" placeholder="aaaa@example.com" />
            </div>
            <div class="field">
              <label>Password</label>
              <input type="password" formControlName="password" placeholder="••••••••" />
            </div>

            <div class="error" *ngIf="error">{{ error }}</div>

            <button type="submit" [disabled]="loading || form.invalid" class="btn-primary">
              <span *ngIf="!loading">Let's go →</span>
              <span *ngIf="loading">Signing you in...</span>
            </button>
          </form>

          <div class="links">
            <a routerLink="/auth/forgot-password">Forgot password?</a>
            <span>·</span>
            <a routerLink="/auth/register">New here? Join us</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-page {
      min-height: 100vh;
      display: grid;
      grid-template-columns: 1fr 1fr;
    }

    /* ── Left visual panel ── */
    .auth-visual {
      background: linear-gradient(145deg, #1b4332 0%, #2d6a4f 45%, #40916c 75%, #52b788 100%);
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem;
      color: white;
      position: relative;
      overflow: hidden;
    }

    .auth-visual::before {
      content: '';
      position: absolute;
      width: 500px; height: 500px;
      background: radial-gradient(circle, rgba(82,183,136,0.2) 0%, transparent 70%);
      border-radius: 50%;
      top: -150px; right: -150px;
      animation: pulse 6s ease-in-out infinite;
    }

    .auth-visual::after {
      content: '';
      position: absolute;
      width: 400px; height: 400px;
      background: radial-gradient(circle, rgba(233,196,106,0.15) 0%, transparent 70%);
      border-radius: 50%;
      bottom: -100px; left: -100px;
      animation: pulse 8s ease-in-out infinite reverse;
    }

    .visual-logo {
      font-size: 5rem;
      margin-bottom: 1.5rem;
      position: relative;
      z-index: 1;
      filter: drop-shadow(0 0 24px rgba(233,196,106,0.5));
    }

    .auth-visual h2 {
      font-size: 2rem;
      font-weight: 800;
      margin-bottom: 0.75rem;
      position: relative;
      z-index: 1;
      text-align: center;
      font-family: 'Poppins', sans-serif;
    }

    .auth-visual p {
      font-size: 1rem;
      opacity: 0.85;
      text-align: center;
      max-width: 280px;
      position: relative;
      z-index: 1;
      line-height: 1.6;
    }

    .features {
      margin-top: 2.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.875rem;
      position: relative;
      z-index: 1;
    }

    .feature {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      font-size: 0.9rem;
      opacity: 0.92;
      background: rgba(255,255,255,0.1);
      padding: 0.6rem 1rem;
      border-radius: 10px;
      border: 1px solid rgba(255,255,255,0.15);
    }

    .feat-icon { font-size: 1.1rem; }

    @keyframes pulse {
      0%, 100% { transform: scale(1); opacity: 0.8; }
      50% { transform: scale(1.1); opacity: 1; }
    }

    /* ── Right form panel ── */
    .auth-form-side {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 3rem 2rem;
      background: #f8faf8;
    }

    .auth-card {
      width: 100%;
      max-width: 420px;
    }

    .brand-mark {
      font-size: 1rem;
      font-weight: 700;
      color: #2d6a4f;
      margin-bottom: 1.25rem;
      letter-spacing: 0.02em;
    }

    .auth-header {
      margin-bottom: 2rem;
    }

    .auth-header h2 {
      font-size: 1.85rem;
      font-weight: 800;
      color: #0a1f14;
      margin-bottom: 0.35rem;
      font-family: 'Poppins', sans-serif;
    }

    .subtitle {
      color: #5a8a6a;
      font-size: 0.95rem;
    }

    .field {
      margin-bottom: 1.25rem;
    }

    .field label {
      display: block;
      font-weight: 500;
      font-size: 0.875rem;
      margin-bottom: 0.4rem;
      color: #1e4030;
    }

    .field input, .field select {
      width: 100%;
      padding: 0.75rem 1rem;
      border: 1.5px solid #c8e6d0;
      border-radius: 8px;
      font-size: 0.95rem;
      background: white;
      color: #0a1f14;
      transition: border-color 0.2s, box-shadow 0.2s;
      font-family: inherit;
    }

    .field input:focus, .field select:focus {
      outline: none;
      border-color: #2d6a4f;
      box-shadow: 0 0 0 3px rgba(45,106,79,0.12);
    }

    .field input::placeholder {
      color: #5a8a6a;
    }

    .btn-primary {
      width: 100%;
      padding: 0.875rem;
      background: linear-gradient(135deg, #2d6a4f, #52b788);
      color: white;
      border: none;
      border-radius: 10px;
      font-size: 0.95rem;
      font-weight: 700;
      cursor: pointer;
      margin-top: 0.5rem;
      transition: all 0.2s;
      box-shadow: 0 4px 20px rgba(45,106,79,0.25);
      font-family: inherit;
    }

    .btn-primary:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 28px rgba(45,106,79,0.35);
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .error {
      background: rgba(220,38,38,0.08);
      border: 1px solid rgba(220,38,38,0.25);
      color: #b91c1c;
      padding: 0.65rem 0.9rem;
      border-radius: 8px;
      font-size: 0.875rem;
      margin-bottom: 1rem;
    }

    .links {
      margin-top: 1.5rem;
      text-align: center;
      font-size: 0.875rem;
      color: #5a8a6a;
      display: flex;
      justify-content: center;
      gap: 0.5rem;
      flex-wrap: wrap;
    }

    .links a {
      color: #2d6a4f;
      font-weight: 600;
      text-decoration: none;
    }

    .links a:hover {
      text-decoration: underline;
    }

    @media (max-width: 768px) {
      .auth-page { grid-template-columns: 1fr; }
      .auth-visual { display: none; }
      .auth-form-side { padding: 2rem 1.25rem; }
    }
  `]
})
// 'export' — makes this class importable by the Angular router and other modules
// 'class'  — ES6/TypeScript keyword; defines a reusable blueprint with properties and methods
export class LoginComponent {
  // 'FormGroup' — typed reference to the reactive form group; tracks validity and values of all child controls
  form: FormGroup;
  // 'boolean' (inferred) — primitive type; true/false flag to disable the submit button while the HTTP call is in flight
  loading = false;
  // 'string' (inferred) — holds the error message to display; empty string means no error
  error = '';

  // 'constructor' — special method called when Angular's DI system instantiates this class
  // 'private' — access modifier; these injected services are only accessible within this class
  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    // 'this' — refers to the current class instance
    // 'fb.group()' — FormBuilder method that creates a FormGroup from a config object
    this.form = this.fb.group({
      // 'Validators.required' — built-in validator; marks the control invalid if the value is empty
      // 'Validators.email'    — built-in validator; marks the control invalid if the value is not a valid email
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  submit() {
    // 'if' — conditional control-flow; short-circuits the function when the form is invalid
    // 'return' — exits the function immediately; no HTTP call is made for an invalid form
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';
    // '.login()' — returns an Observable; calling .subscribe() starts the HTTP request
    // 'subscribe' — RxJS method that activates an Observable and registers callbacks for next/error/complete
    this.auth.login(this.form.value).subscribe({
      // 'next' — callback invoked when the Observable emits a value (successful HTTP response)
      next: (res) => this.redirectByRole(res.role),
      // 'error' — callback invoked when the Observable errors (HTTP error, network failure, etc.)
      error: (err) => {
        this.error = this.parseError(err);
        this.loading = false;
      }
    });
  }

  // 'private' — this helper is an implementation detail; not part of the public API of this class
  private parseError(err: any): string { // 'any' — opt-out of type-checking; accepts any shape of error object
    // 'if' — conditional checks for specific error shapes to produce user-friendly messages
    if (err.status === 0) return 'Cannot reach the server. Please check your connection or try again later.';
    // 'typeof' — TypeScript/JS operator that returns the runtime type of a value as a string
    if (typeof err.error === 'string' && err.error.trim()) return err.error;
    if (err.error?.message) return err.error.message; // '?.' — optional chaining; avoids TypeError if err.error is null/undefined
    if (err.message) return err.message;
    return 'Invalid credentials. Please try again.';
  }

  // 'private' — encapsulates role-based routing logic; not exposed to the template
  private redirectByRole(role: string) { // 'string' — parameter type annotation
    // 'const' — block-scoped variable declaration; the binding cannot be reassigned (though the object is still mutable)
    // 'Record<string, string>' — TypeScript utility type; an object whose keys and values are both strings
    const routes: Record<string, string> = {
      Admin: '/admin/dashboard',
      Customer: '/restaurants',
      RestaurantOwner: '/owner/dashboard',
      DeliveryAgent: '/agent/dashboard'
    };
    // '??' — nullish coalescing operator; falls back to '/' if routes[role] is null or undefined
    this.router.navigate([routes[role] ?? '/']);
  }
}
