// import: ES module keyword — pulls named exports from external packages/files into this file's scope
import { Component, OnInit } from '@angular/core';
// CommonModule: Angular module that provides structural directives (*ngIf, *ngFor) and built-in pipes (| date, | number, | async)
import { CommonModule } from '@angular/common';
// RouterLink: Angular directive that enables declarative client-side navigation without a full page reload
import { RouterLink } from '@angular/router';
// PaymentService: injectable service class for payment-related HTTP calls; PaymentDto: interface describing a payment record's shape
import { PaymentService, PaymentDto } from '../../../core/services/payment.service';
// AuthService: injectable service that exposes the currently authenticated user and login state
import { AuthService } from '../../../core/services/auth.service';

/**
 * Payment history component.
 * Displays all payment transactions for the authenticated customer with
 * aggregate stats (total paid, refunded, average order value) and a
 * detailed transaction list linking to the associated order.
 */
// @Component: Angular decorator — attaches component metadata (selector, template, styles, imports) to the class below
@Component({
  selector: 'app-payment-history', // selector: the custom HTML element name used to embed this component in a parent template
  standalone: true,                 // standalone: true — this component is self-contained and does not require an NgModule declaration
  imports: [CommonModule, RouterLink], // imports: declares the Angular directives and pipes this standalone component needs
  template: `
    <div class="ph-page">

      <!-- ── Hero header ── -->
      <div class="ph-hero">
        <div class="ph-hero-inner">
          <div class="ph-hero-left">
            <div class="ph-hero-icon">💳</div>
            <div>
              <h1 class="ph-hero-title">Payment History</h1>
              <p class="ph-hero-sub">Track every rupee you've spent on CraveKart</p>
            </div>
          </div>
          <a routerLink="/orders" class="ph-back-btn">
            <span>←</span> My Orders
          </a>
        </div>
      </div>

      <div class="ph-content">

        <!-- ── Loading ── -->
        <div class="ph-state" *ngIf="loading">
          <div class="ph-spinner"></div>
          <p>Fetching your transactions...</p>
        </div>

        <!-- ── Error ── -->
        <div class="ph-state ph-error" *ngIf="!loading && error">
          <div class="ph-state-icon">⚠️</div>
          <p>{{ error }}</p>
          <button class="ph-retry-btn" (click)="ngOnInit()">Try again</button>
        </div>

        <!-- ── Empty ── -->
        <div class="ph-state" *ngIf="!loading && !error && payments.length === 0">
          <div class="ph-state-icon">🧾</div>
          <p class="ph-empty-title">No transactions yet</p>
          <p class="ph-empty-sub">Place your first order to see payments here</p>
          <a routerLink="/restaurants" class="ph-cta-btn">🍴 Browse Restaurants</a>
        </div>

        <ng-container *ngIf="!loading && !error && payments.length > 0">

          <!-- ── Stats strip ── -->
          <div class="ph-stats">
            <div class="ph-stat ph-stat-total">
              <div class="ph-stat-icon-wrap">
                <span class="ph-stat-icon">🧾</span>
              </div>
              <div class="ph-stat-body">
                <div class="ph-stat-val">{{ payments.length }}</div>
                <div class="ph-stat-label">Transactions</div>
              </div>
            </div>

            <div class="ph-stat ph-stat-paid">
              <div class="ph-stat-icon-wrap">
                <span class="ph-stat-icon">✅</span>
              </div>
              <div class="ph-stat-body">
                <div class="ph-stat-val">₹{{ confirmedTotal | number:'1.0-0' }}</div>
                <div class="ph-stat-label">Total Paid</div>
              </div>
            </div>

            <div class="ph-stat ph-stat-refunded">
              <div class="ph-stat-icon-wrap">
                <span class="ph-stat-icon">💸</span>
              </div>
              <div class="ph-stat-body">
                <div class="ph-stat-val">₹{{ refundedTotal | number:'1.0-0' }}</div>
                <div class="ph-stat-label">Refunded</div>
              </div>
            </div>

            <div class="ph-stat ph-stat-avg">
              <div class="ph-stat-icon-wrap">
                <span class="ph-stat-icon">📊</span>
              </div>
              <div class="ph-stat-body">
                <div class="ph-stat-val">₹{{ avgOrder | number:'1.0-0' }}</div>
                <div class="ph-stat-label">Avg. Order</div>
              </div>
            </div>
          </div>

          <!-- ── Transaction list ── -->
          <div class="ph-list-header">
            <h2 class="ph-list-title">Recent Transactions</h2>
            <span class="ph-list-count">{{ payments.length }} total</span>
          </div>

          <div class="ph-list">
            <div class="ph-item" *ngFor="let p of payments; let i = index"
                 [style.animation-delay]="(i * 0.05) + 's'">

              <!-- Left: status indicator + icon -->
              <div class="ph-item-left">
                <div class="ph-item-dot" [class]="p.status.toLowerCase()"></div>
                <div class="ph-item-method-icon">
                  {{ p.paymentMethod === 'CashOnDelivery' ? '💵' : '💳' }}
                </div>
              </div>

              <!-- Center: order info -->
              <div class="ph-item-center">
                <div class="ph-item-order">
                  Order <span class="ph-item-id">#{{ p.orderId | slice:0:8 | uppercase }}</span>
                </div>
                <div class="ph-item-meta">
                  <span class="ph-item-method">
                    {{ p.paymentMethod === 'CashOnDelivery' ? 'Cash on Delivery' : 'Card Payment' }}
                  </span>
                  <span class="ph-item-sep">·</span>
                  <span class="ph-item-date">{{ p.createdAt | date:'d MMM yyyy, h:mm a' }}</span>
                </div>
              </div>

              <!-- Right: amount + status + action -->
              <div class="ph-item-right">
                <div class="ph-item-amount" [class.refunded]="p.status === 'Refunded'">
                  {{ p.status === 'Refunded' ? '-' : '' }}₹{{ p.amount | number:'1.2-2' }}
                </div>
                <span class="ph-status-pill" [class]="p.status.toLowerCase()">
                  <span class="ph-status-dot"></span>
                  {{ p.status }}
                </span>
                <a [routerLink]="['/orders', p.orderId]" class="ph-view-btn">
                  View →
                </a>
              </div>

            </div>
          </div>

        </ng-container>
      </div>
    </div>
  `,
  styleUrl: './payment-history.scss' // styleUrl: path to the component-scoped SCSS file; styles are encapsulated and do not leak globally
})
// export: makes this class importable by other modules (e.g., the router's loadComponent)
// class: TypeScript/ES6 keyword — defines a named reference type with state (fields) and behaviour (methods)
// implements: TypeScript keyword — declares that this class fulfils the OnInit interface contract
// OnInit: Angular lifecycle interface — mandates the ngOnInit() method, called once after the first change-detection cycle
export class PaymentHistoryComponent implements OnInit {
  // PaymentDto[]: typed array — an ordered list where every element must conform to the PaymentDto interface
  payments: PaymentDto[] = [];
  // boolean: primitive type — represents a true/false flag; here it tracks whether the HTTP request is in flight
  loading = true;
  // string: primitive type — holds the error message to display if the HTTP request fails
  error = '';

  // constructor: called once by Angular's dependency-injection system when the component is instantiated
  // private: access modifier — the injected services are only accessible within this class
  constructor(private paymentSvc: PaymentService, private auth: AuthService) {}

  // ngOnInit: Angular lifecycle hook — the recommended place to trigger initial data fetching
  ngOnInit() {
    this.loading = true;
    this.error = '';
    // ?.id: optional chaining — safely accesses .id only if currentUser() returns a non-null value
    const userId = this.auth.currentUser()?.id;
    // if: conditional control-flow — guards against calling the API when no user is logged in
    if (!userId) { this.loading = false; this.error = 'Not logged in.'; return; }
    // .subscribe(): Observable consumer — registers next/error callbacks to handle the async HTTP response
    this.paymentSvc.getByCustomer(userId).subscribe({
      // next: callback invoked when the Observable emits a successful value
      next: p => { this.payments = p; this.loading = false; },
      // error: callback invoked when the Observable emits an error (e.g., HTTP 4xx/5xx)
      error: (err) => {
        this.loading = false;
        // ||: logical OR — falls back to the next expression if the previous is falsy (null/undefined/'')
        this.error = err.error?.message || err.error || `Error ${err.status}: Could not load payments.`;
      }
    });
  }

  // get: accessor keyword — defines a computed read-only property; recalculated on every access
  // number: return type annotation — this getter always produces a numeric value
  get confirmedTotal(): number {
    // .filter(): Array method — returns a new array containing only elements that pass the predicate
    // .reduce(): Array method — accumulates all elements into a single value (here, a running sum)
    return this.payments
      .filter(p => p.status === 'Confirmed')
      .reduce((s, p) => s + p.amount, 0);
  }

  get refundedTotal(): number {
    return this.payments
      .filter(p => p.status === 'Refunded')
      .reduce((s, p) => s + p.amount, 0);
  }

  get avgOrder(): number {
    // if: guard — avoids a division-by-zero error when the payments array is empty
    if (!this.payments.length) return 0;
    // return: exits the getter and sends the computed average back to the caller
    return this.payments.reduce((s, p) => s + p.amount, 0) / this.payments.length;
  }
}
