import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Router } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderStats } from '../../../core/models/order.models';
import { RestaurantDto } from '../../../core/models/restaurant.models';
import { OrderDto } from '../../../core/models/order.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
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
export class AdminDashboardComponent implements OnInit {
  stats?: OrderStats;
  pendingRestaurants: RestaurantDto[] = [];
  recentOrders: OrderDto[] = [];

  constructor(
    private orderSvc: OrderService,
    private restaurantSvc: RestaurantService,
    private router: Router
  ) {}

  ngOnInit() {
    this.orderSvc.getStats().subscribe(s => this.stats = s);
    this.restaurantSvc.adminGetAll('Pending').subscribe(r => this.pendingRestaurants = r.slice(0, 5));
    this.orderSvc.adminGetAll().subscribe(o => this.recentOrders = o.slice(0, 10));
  }

  goOrders(status?: string) {
    this.router.navigate(['/admin/orders'], status ? { queryParams: { status } } : {});
  }

  approve(r: RestaurantDto) {
    this.restaurantSvc.approve(r.id).subscribe(() => {
      this.pendingRestaurants = this.pendingRestaurants.filter(x => x.id !== r.id);
    });
  }

  reject(r: RestaurantDto) {
    const reason = prompt('Reason for rejection:');
    if (!reason) return;
    this.restaurantSvc.reject(r.id, reason).subscribe(() => {
      this.pendingRestaurants = this.pendingRestaurants.filter(x => x.id !== r.id);
    });
  }
}
