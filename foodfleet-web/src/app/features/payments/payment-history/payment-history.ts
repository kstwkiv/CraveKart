import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PaymentService, PaymentDto } from '../../../core/services/payment.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-payment-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
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
  styleUrl: './payment-history.scss'
})
export class PaymentHistoryComponent implements OnInit {
  payments: PaymentDto[] = [];
  loading = true;
  error = '';

  constructor(private paymentSvc: PaymentService, private auth: AuthService) {}

  ngOnInit() {
    this.loading = true;
    this.error = '';
    const userId = this.auth.currentUser()?.id;
    if (!userId) { this.loading = false; this.error = 'Not logged in.'; return; }
    this.paymentSvc.getByCustomer(userId).subscribe({
      next: p => { this.payments = p; this.loading = false; },
      error: (err) => {
        this.loading = false;
        this.error = err.error?.message || err.error || `Error ${err.status}: Could not load payments.`;
      }
    });
  }

  get confirmedTotal(): number {
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
    if (!this.payments.length) return 0;
    return this.payments.reduce((s, p) => s + p.amount, 0) / this.payments.length;
  }
}
