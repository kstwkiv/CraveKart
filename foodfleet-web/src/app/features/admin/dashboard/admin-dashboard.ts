// 'import' — ES module keyword; pulls named exports from another module into this file's scope
// 'Component' — Angular decorator factory; marks a class as a component and attaches template/style metadata
// 'OnInit'    — Angular lifecycle-hook interface; requires ngOnInit() to be implemented; called after first ngOnChanges
import { Component, OnInit } from '@angular/core';
// 'CommonModule' — provides *ngIf, *ngFor, async pipe, and other structural directives
import { CommonModule } from '@angular/common';
// 'RouterLink' — directive that turns an anchor into a client-side navigation link
import { RouterLink } from '@angular/router';
// 'Router' — Angular service for programmatic navigation between routes
import { Router } from '@angular/router';
// 'OrderService'      — application service that wraps HTTP calls to the Order API
import { OrderService } from '../../../core/services/order.service';
// 'RestaurantService' — application service that wraps HTTP calls to the Restaurant API
import { RestaurantService } from '../../../core/services/restaurant.service';
// 'OrderStats'    — TypeScript interface (pure type contract) for aggregate order statistics
import { OrderStats } from '../../../core/models/order.models';
// 'RestaurantDto' — TypeScript interface (pure type contract) for a restaurant data transfer object
import { RestaurantDto } from '../../../core/models/restaurant.models';
// 'OrderDto'      — TypeScript interface (pure type contract) for an order data transfer object
import { OrderDto } from '../../../core/models/order.models';

/**
 * Admin dashboard component.
 * Displays platform-wide order statistics, a list of pending restaurant
 * approvals, and a feed of recent orders. Provides quick-action buttons
 * for approving or rejecting restaurants inline.
 */
// '@Component' — class decorator; Angular reads this metadata to compile the template and wire up DI
@Component({
  selector: 'app-admin-dashboard',  // CSS selector used in templates/router to render this component
  standalone: true,                 // standalone: true — no NgModule needed; the component manages its own imports
  imports: [CommonModule, RouterLink], // 'imports' — Angular dependencies for this component's template
  template: `
    <div class="page">
      <div class="page-header">
        <h2>Admin Dashboard</h2>
        <p class="subtitle">Manage restaurants, orders and platform activity</p>
      </div>

      <div class="stats-grid" *ngIf="stats">
        <div class="stat-card clickable" (click)="goOrders()">
          <div class="stat-icon"><img src="/icons/orders.png" alt="Total Orders" /></div>
          <div class="stat-value">{{ stats.total }}</div>
          <div class="stat-label">Total Orders</div>
        </div>
        <div class="stat-card green clickable" (click)="goOrders('Delivered')">
          <div class="stat-icon"><img src="/icons/delivered.png" alt="Delivered" /></div>
          <div class="stat-value">{{ stats.delivered }}</div>
          <div class="stat-label">Delivered</div>
        </div>
        <div class="stat-card orange clickable" (click)="goOrders('Preparing')">
          <div class="stat-icon"><img src="/icons/in-progress.png" alt="In Progress" /></div>
          <div class="stat-value">{{ stats.preparing + stats.confirmed }}</div>
          <div class="stat-label">In Progress</div>
        </div>
        <div class="stat-card blue clickable" (click)="goOrders()">
          <div class="stat-icon"><img src="/icons/revenue.png" alt="Revenue" /></div>
          <div class="stat-value">₹{{ stats.totalRevenue | number:'1.0-0' }}</div>
          <div class="stat-label">Revenue</div>
        </div>
      </div>

      <div class="sections">
        <div class="section">
          <div class="section-header">
            <h3>Pending Approvals</h3>
            <a routerLink="/admin/restaurants">View all</a>
          </div>
          <div *ngFor="let r of pendingRestaurants" class="row-item">
            <span class="name">{{ r.name }}</span>
            <span class="addr">{{ r.address }}</span>
            <div class="actions">
              <button class="btn-approve" (click)="approve(r)">Approve</button>
              <button class="btn-reject" (click)="reject(r)">Reject</button>
            </div>
          </div>
          <div *ngIf="pendingRestaurants.length === 0" class="empty">No pending approvals 🎉</div>
        </div>

        <div class="section">
          <div class="section-header">
            <h3>Recent Orders</h3>
            <a routerLink="/admin/orders">View all</a>
          </div>
          <div *ngFor="let o of recentOrders" class="row-item">
            <span class="name">#{{ o.id | slice:0:8 }}</span>
            <span class="status-badge" [class]="o.status.toLowerCase()">{{ o.status }}</span>
            <span class="amount">₹{{ o.totalAmount }}</span>
            <span class="date">{{ o.createdAt | date:'shortDate' }}</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrl: './admin-dashboard.scss'
})
// 'export' — makes this class importable by the Angular router and other modules
// 'class'  — ES6/TypeScript keyword; defines a reusable blueprint with properties and methods
// 'implements' — TypeScript keyword; enforces that the class satisfies the listed interface contracts
// 'OnInit' — Angular lifecycle interface being implemented; requires ngOnInit() method
export class AdminDashboardComponent implements OnInit {
  // '?' — optional property; may be undefined until the HTTP response arrives
  // 'OrderStats' — TypeScript interface used as the type annotation
  stats?: OrderStats;
  // 'RestaurantDto[]' — Array type annotation; an ordered collection of RestaurantDto objects
  pendingRestaurants: RestaurantDto[] = []; // '= []' — initialised to an empty array to avoid null-reference errors in the template
  // 'OrderDto[]' — Array type annotation; an ordered collection of OrderDto objects
  recentOrders: OrderDto[] = [];

  // 'constructor' — called by Angular's DI system when instantiating this class
  // 'private' — access modifier; these injected services are only accessible within this class
  constructor(
    private orderSvc: OrderService,
    private restaurantSvc: RestaurantService,
    private router: Router
  ) {}

  // 'ngOnInit' — Angular lifecycle hook method; called once after the component's inputs are first set
  // Ideal place to trigger initial data-loading HTTP calls
  ngOnInit() {
    // 'subscribe' — RxJS method that activates an Observable and registers a callback for emitted values
    this.orderSvc.getStats().subscribe(s => this.stats = s);
    this.restaurantSvc.adminGetAll('Pending').subscribe(r => this.pendingRestaurants = r.slice(0, 5));
    this.orderSvc.adminGetAll().subscribe(o => this.recentOrders = o.slice(0, 10));
  }

  // 'status?' — optional parameter; TypeScript '?' makes the argument optional (may be undefined)
  goOrders(status?: string) {
    // 'if' — conditional; only adds queryParams when a status filter is provided
    this.router.navigate(['/admin/orders'], status ? { queryParams: { status } } : {});
  }

  approve(r: RestaurantDto) { // 'RestaurantDto' — type annotation; ensures only valid restaurant objects are passed
    // 'subscribe' — activates the Observable returned by approve(); triggers the HTTP PATCH/PUT call
    this.restaurantSvc.approve(r.id).subscribe(() => {
      // Arrow function — removes the approved restaurant from the local array without a full reload
      this.pendingRestaurants = this.pendingRestaurants.filter(x => x.id !== r.id);
    });
  }

  reject(r: RestaurantDto) {
    // 'null' — JS/TS primitive; prompt() returns null when the user cancels the dialog
    // 'if' — guards against sending a rejection with no reason
    // 'return' — exits the function early if no reason was provided
    const reason = prompt('Reason for rejection:');
    if (!reason) return;
    this.restaurantSvc.reject(r.id, reason).subscribe(() => {
      this.pendingRestaurants = this.pendingRestaurants.filter(x => x.id !== r.id);
    });
  }
}
