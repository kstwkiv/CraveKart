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
    <div class="page">
      <div class="page-header">
        <div>
          <h2>Payment History</h2>
          <p class="subtitle">All your transactions in one place</p>
        </div>
        <a routerLink="/orders" class="btn-back">← My Orders</a>
      </div>

      <!-- Summary cards -->
      <div class="summary-grid" *ngIf="!loading && !error && payments.length > 0">
        <div class="summary-card">
          <div class="summary-icon">💳</div>
          <div class="summary-body">
            <div class="summary-val">{{ payments.length }}</div>
            <div class="summary-label">Total Transactions</div>
          </div>
        </div>
        <div class="summary-card">
          <div class="summary-icon">✅</div>
          <div class="summary-body">
            <div class="summary-val">₹{{ confirmedTotal | number:'1.0-0' }}</div>
            <div class="summary-label">Total Paid</div>
          </div>
        </div>
        <div class="summary-card">
          <div class="summary-icon">💸</div>
          <div class="summary-body">
            <div class="summary-val">₹{{ refundedTotal | number:'1.0-0' }}</div>
            <div class="summary-label">Total Refunded</div>
          </div>
        </div>
      </div>

      <!-- Loading -->
      <div class="state-box" *ngIf="loading">
        <div class="state-icon">⏳</div>
        <p>Loading payment history...</p>
      </div>

      <!-- Error -->
      <div class="state-box error-state" *ngIf="!loading && error">
        <div class="state-icon">⚠️</div>
        <p>{{ error }}</p>
        <button (click)="ngOnInit()">Try again</button>
      </div>

      <!-- Empty -->
      <div class="state-box" *ngIf="!loading && !error && payments.length === 0">
        <div class="state-icon">💳</div>
        <p>No payments yet.</p>
        <a routerLink="/restaurants" class="btn-browse">Browse restaurants</a>
      </div>

      <!-- Table -->
      <div class="table-wrap" *ngIf="!loading && !error && payments.length > 0">
        <table>
          <thead>
            <tr>
              <th>Order</th>
              <th>Method</th>
              <th>Amount</th>
              <th>Status</th>
              <th>Date</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let p of payments">
              <td class="order-id">#{{ p.orderId | slice:0:8 | uppercase }}</td>
              <td>
                <span class="method-badge">
                  {{ p.paymentMethod === 'CashOnDelivery' ? '💵 Cash' : '💳 Card' }}
                </span>
              </td>
              <td class="amount">₹{{ p.amount | number:'1.2-2' }}</td>
              <td>
                <span class="status-badge" [class]="p.status.toLowerCase()">
                  {{ statusIcon(p.status) }} {{ p.status }}
                </span>
              </td>
              <td class="date">{{ p.createdAt | date:'mediumDate' }}</td>
              <td>
                <a [routerLink]="['/orders', p.orderId]" class="btn-view">View Order →</a>
              </td>
            </tr>
          </tbody>
        </table>
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

  statusIcon(status: string): string {
    const icons: Record<string, string> = {
      Confirmed: '✅', Pending: '⏳', Failed: '❌', Refunded: '💸'
    };
    return icons[status] ?? '💳';
  }
}
