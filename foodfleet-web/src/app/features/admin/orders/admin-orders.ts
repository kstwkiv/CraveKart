import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderDto } from '../../../core/models/order.models';

@Component({
  selector: 'app-admin-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h2>Order Management</h2>
          <p class="subtitle">View and manage all platform orders</p>
        </div>
        <a routerLink="/admin/dashboard" class="btn-back">← Dashboard</a>
      </div>

      <!-- Filter tabs -->
      <div class="filters">
        <button *ngFor="let s of statuses" class="filter-btn"
          [class.active]="activeStatus === s"
          (click)="setStatus(s)">
          {{ s }}
          <span class="count" *ngIf="s !== 'All'">{{ getCount(s) }}</span>
        </button>
      </div>

      <div class="loading" *ngIf="loading">Loading orders...</div>

      <div class="table-wrap" *ngIf="!loading">
        <table>
          <thead>
            <tr>
              <th>Order ID</th>
              <th>Customer</th>
              <th>Items</th>
              <th>Total</th>
              <th>Status</th>
              <th>Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let o of filtered">
              <td class="order-id">#{{ o.id | slice:0:8 }}...</td>
              <td class="customer">{{ o.customerId | slice:0:8 }}...</td>
              <td>{{ o.items.length }} item(s)</td>
              <td class="amount">₹{{ o.totalAmount }}</td>
              <td><span class="status-badge" [class]="getStatus(o).toLowerCase()">{{ getStatus(o) }}</span></td>
              <td class="date">{{ o.createdAt | date:'mediumDate' }}</td>
              <td class="actions">
                <a [routerLink]="['/orders', o.id]" class="btn-view">View</a>
                <button *ngIf="getStatus(o) === 'Ready' || getStatus(o) === 'PickedUp'"
                  class="btn-deliver" (click)="markDelivered(o)">🏠 Delivered</button>
                <button *ngIf="getStatus(o) === 'Placed' || getStatus(o) === 'Confirmed'"
                  class="btn-cancel" (click)="cancel(o)">Cancel</button>
              </td>
            </tr>
            <tr *ngIf="filtered.length === 0">
              <td colspan="7" class="empty">No orders found</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  `,
  styles: [`
    .page { max-width: 1200px; margin: 0 auto; padding: 2rem 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
    .page-header h2 { font-size: 1.6rem; font-weight: 800; margin: 0; color: var(--text-primary); }
    .subtitle { color: var(--text-muted); font-size: 0.9rem; margin-top: 0.25rem; }
    .btn-back { padding: 0.5rem 1rem; background: var(--surface-alt); border-radius: 8px; text-decoration: none; color: var(--text-primary); font-size: 0.875rem; font-weight: 600; border: 1px solid var(--border); }
    .filters { display: flex; gap: 0.5rem; margin-bottom: 1.5rem; flex-wrap: wrap; }
    .filter-btn { padding: 0.4rem 1rem; border: 1px solid var(--border); border-radius: 20px; background: var(--surface); cursor: pointer; font-size: 0.85rem; font-weight: 500; color: var(--text-secondary); transition: all 0.15s; display: flex; align-items: center; gap: 0.4rem; }
    .filter-btn.active { background: var(--primary); color: white; border-color: var(--primary); }
    .count { background: rgba(255,255,255,0.25); padding: 0.1rem 0.4rem; border-radius: 10px; font-size: 0.72rem; font-weight: 700; }
    .filter-btn:not(.active) .count { background: var(--surface-alt); color: var(--text-muted); }
    .table-wrap { background: var(--surface); border-radius: 12px; box-shadow: var(--shadow); border: 1px solid var(--border); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th { background: var(--surface-alt); padding: 0.75rem 1rem; text-align: left; font-size: 0.78rem; font-weight: 700; text-transform: uppercase; color: var(--text-muted); letter-spacing: 0.05em; }
    td { padding: 0.875rem 1rem; border-top: 1px solid var(--border); font-size: 0.875rem; vertical-align: middle; color: var(--text-primary); }
    td.order-id { font-weight: 700; font-family: monospace; }
    td.customer { color: var(--text-muted); font-size: 0.8rem; }
    td.amount { font-weight: 700; color: var(--accent); }
    td.date { color: var(--text-muted); font-size: 0.8rem; }
    .actions { display: flex; gap: 0.4rem; }
    .btn-view { padding: 0.3rem 0.7rem; background: var(--primary); color: white; border-radius: 6px; font-size: 0.78rem; font-weight: 600; text-decoration: none; }
    .btn-cancel { padding: 0.3rem 0.7rem; background: var(--danger); color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .btn-deliver { padding: 0.3rem 0.7rem; background: var(--accent, #1a9090); color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .empty { text-align: center; color: var(--text-muted); padding: 2rem; }
    .loading { text-align: center; padding: 3rem; color: var(--text-muted); }
  `]
})
export class AdminOrdersComponent implements OnInit {
  orders: OrderDto[] = [];
  filtered: OrderDto[] = [];
  loading = false;
  activeStatus = 'All';
  statuses = ['All', 'Placed', 'Confirmed', 'Preparing', 'Ready', 'PickedUp', 'Delivered', 'Cancelled'];

  private readonly statusMap: Record<number, string> = {
    0: 'Placed', 1: 'Confirmed', 2: 'Preparing', 3: 'Ready', 4: 'PickedUp', 5: 'Delivered', 6: 'Cancelled', 7: 'Rejected'
  };

  constructor(private orderSvc: OrderService, private route: ActivatedRoute) {}

  ngOnInit() {
    // Check if a status filter was passed via query param
    const status = this.route.snapshot.queryParamMap.get('status');
    if (status) this.activeStatus = status;
    this.load();
  }

  load() {
    this.loading = true;
    this.orderSvc.adminGetAll().subscribe({
      next: o => { this.orders = o; this.applyFilter(); this.loading = false; },
      error: () => this.loading = false
    });
  }

  setStatus(status: string) {
    this.activeStatus = status;
    this.applyFilter();
  }

  applyFilter() {
    this.filtered = this.activeStatus === 'All'
      ? this.orders
      : this.orders.filter(o => this.getStatus(o) === this.activeStatus);
  }

  getStatus(order: OrderDto): string {
    const s = order.status as unknown;
    return typeof s === 'number' ? (this.statusMap[s] ?? String(s)) : String(s);
  }

  getCount(status: string): number {
    return this.orders.filter(o => this.getStatus(o) === status).length;
  }

  cancel(o: OrderDto) {
    if (!confirm('Cancel this order?')) return;
    this.orderSvc.cancel(o.id).subscribe(() => {
      o.status = 'Cancelled' as any;
      this.applyFilter();
    });
  }

  markDelivered(o: OrderDto) {
    if (!confirm('Mark this order as Delivered?')) return;
    // 5 = Delivered enum value
    this.orderSvc.updateStatus(o.id, 5).subscribe(() => {
      o.status = 5 as any;
      this.applyFilter();
    });
  }
}
