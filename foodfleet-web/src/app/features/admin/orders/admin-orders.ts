// import: ES module keyword — pulls named exports from external packages/files into this file's scope
// Component: Angular decorator factory — attaches component metadata to the class
// OnInit: Angular lifecycle interface — requires ngOnInit() to be implemented; called after the first change-detection cycle
import { Component, OnInit } from '@angular/core';
// CommonModule: provides *ngIf, *ngFor, and built-in pipes (| date, | slice, | number) for use in templates
import { CommonModule } from '@angular/common';
// FormsModule: enables template-driven forms and the [(ngModel)] two-way data-binding directive
import { FormsModule } from '@angular/forms';
// RouterLink: directive that turns an anchor/element into a client-side navigation link
// ActivatedRoute: service that exposes the current route's URL params, query params, and data
import { RouterLink, ActivatedRoute } from '@angular/router';
// OrderService: injectable service encapsulating all order-related HTTP calls
import { OrderService } from '../../../core/services/order.service';
// PaymentService: injectable service encapsulating all payment-related HTTP calls (e.g., refunds)
import { PaymentService } from '../../../core/services/payment.service';
// OrderDto: TypeScript interface — a structural type contract describing the shape of an order object from the API
import { OrderDto } from '../../../core/models/order.models';

/**
 * Admin order management component.
 * Lists all platform orders with status filter tabs, supports cancellation,
 * manual delivery marking, and payment refund initiation.
 */
// @Component: Angular decorator — registers this class as a component and provides its configuration metadata
@Component({
  selector: 'app-admin-orders', // selector: the custom HTML tag name used to embed this component in a parent template
  standalone: true,              // standalone: true — self-contained component; no NgModule declaration needed
  imports: [CommonModule, FormsModule, RouterLink], // imports: Angular modules/directives this standalone component depends on
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
                <button *ngIf="getStatus(o) === 'Cancelled' || getStatus(o) === 'Rejected'"
                  class="btn-refund" (click)="refund(o)" [disabled]="refundingId === o.id">
                  {{ refundingId === o.id ? '...' : '💸 Refund' }}
                </button>
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
    .btn-refund { padding: 0.3rem 0.7rem; background: #1a237e; color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .btn-refund:disabled { opacity: 0.5; cursor: not-allowed; }
    .btn-deliver { padding: 0.3rem 0.7rem; background: var(--accent, #1a9090); color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .empty { text-align: center; color: var(--text-muted); padding: 2rem; }
    .loading { text-align: center; padding: 3rem; color: var(--text-muted); }
  `]
})
// export: makes this class importable by the router's lazy loadComponent mechanism
// class: TypeScript/ES6 keyword — defines a named reference type with fields (state) and methods (behaviour)
// implements OnInit: TypeScript keyword — enforces the lifecycle interface contract; the compiler errors if ngOnInit is missing
export class AdminOrdersComponent implements OnInit {
  // OrderDto[]: typed array — every element must conform to the OrderDto interface shape
  orders: OrderDto[] = [];
  filtered: OrderDto[] = [];
  // boolean: primitive type — true/false flag tracking whether the HTTP request is in flight
  loading = false;
  // string: primitive type — holds the currently selected status filter tab label
  activeStatus = 'All';
  // string | null: union type — either a string (the order ID being refunded) or null (no refund in progress)
  refundingId: string | null = null;
  // Array of string literals — the set of valid status filter labels shown as tabs
  statuses = ['All', 'Placed', 'Confirmed', 'Preparing', 'Ready', 'PickedUp', 'Delivered', 'Cancelled'];

  // private: access modifier — this field is an internal implementation detail not accessible from outside the class
  // readonly: modifier — the statusMap object reference cannot be reassigned after construction
  // Record<number, string>: TypeScript utility type — an object whose keys are numbers and values are strings (a lookup table)
  private readonly statusMap: Record<number, string> = {
    0: 'Placed', 1: 'Confirmed', 2: 'Preparing', 3: 'Ready', 4: 'PickedUp', 5: 'Delivered', 6: 'Cancelled', 7: 'Rejected'
  };

  // constructor: called once by Angular's DI system; parameters are automatically injected by type
  constructor(private orderSvc: OrderService, private paymentSvc: PaymentService, private route: ActivatedRoute) {}

  // ngOnInit: Angular lifecycle hook — runs after the component's inputs are initialised; ideal for initial data loading
  ngOnInit() {
    // Check if a status filter was passed via query param
    // .get('status'): reads a query parameter from the current URL snapshot (returns string | null)
    const status = this.route.snapshot.queryParamMap.get('status');
    // if: conditional — applies the query-param filter only when one is present
    if (status) this.activeStatus = status;
    this.load();
  }

  load() {
    this.loading = true;
    // .subscribe(): Observable consumer — registers callbacks to handle the async HTTP response stream
    this.orderSvc.adminGetAll().subscribe({
      // next: callback invoked when the Observable emits a successful value (the orders array)
      next: o => { this.orders = o; this.applyFilter(); this.loading = false; },
      error: () => this.loading = false
    });
  }

  setStatus(status: string) {
    this.activeStatus = status;
    this.applyFilter();
  }

  applyFilter() {
    // Ternary operator: concise if/else — returns all orders when 'All' is selected, otherwise filters by status
    this.filtered = this.activeStatus === 'All'
      ? this.orders
      : this.orders.filter(o => this.getStatus(o) === this.activeStatus);
  }

  // getStatus: converts the numeric status enum value to a human-readable string using the statusMap lookup table
  getStatus(order: OrderDto): string {
    // as unknown: type assertion — widens the type to bypass strict checks before narrowing with typeof
    const s = order.status as unknown;
    // typeof: runtime operator — returns a string describing the primitive type of the operand
    return typeof s === 'number' ? (this.statusMap[s] ?? String(s)) : String(s);
  }

  // number return type: counts how many orders match the given status string
  getCount(status: string): number {
    return this.orders.filter(o => this.getStatus(o) === status).length;
  }

  cancel(o: OrderDto) {
    // confirm(): browser built-in — shows a modal dialog and returns true if the user clicks OK
    if (!confirm('Cancel this order?')) return;
    this.orderSvc.cancel(o.id).subscribe(() => {
      // as any: type assertion — bypasses TypeScript's type checker; used here to assign a string to a numeric enum field
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

  refund(o: OrderDto) {
    if (!confirm(`Issue a refund of ₹${o.totalAmount} for order #${o.id.slice(0, 8).toUpperCase()}?`)) return;
    this.refundingId = o.id;
    this.paymentSvc.refund(o.id).subscribe({
      next: () => {
        // null: the absence of an object value — resets the refundingId to indicate no refund is in progress
        this.refundingId = null;
        alert('Refund processed successfully.');
      },
      error: (err) => {
        this.refundingId = null;
        const msg = err.error?.message || err.error || 'Refund failed.';
        alert(`${msg}`);
      }
    });
  }
}
