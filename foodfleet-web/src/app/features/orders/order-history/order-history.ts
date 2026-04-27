import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderDto } from '../../../core/models/order.models';
import { RecommendationService, Recommendation } from '../../../core/services/recommendation.service';

@Component({
  selector: 'app-order-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h2>My Orders</h2>
          <p class="subtitle">Your food journey so far</p>
        </div>
        <a routerLink="/restaurants" class="btn-browse">🍴 New Order</a>
      </div>

      <div *ngIf="loading" class="loading-state">
        <div class="icon">⏳</div><p>Loading your orders...</p>
      </div>

      <div *ngIf="error" class="error-state">
        <div class="icon">⚠️</div><p>{{ error }}</p>
        <button (click)="load()">Try again</button>
      </div>

      <div *ngIf="!loading && !error">
        <div *ngIf="orders.length === 0" class="empty-state">
          <div class="icon">🛒</div>
          <p>You haven't placed any orders yet.</p>
          <a routerLink="/restaurants" class="btn-browse">Browse restaurants</a>
        </div>

        <div class="orders-list" *ngIf="orders.length > 0">
          <a *ngFor="let o of orders" [routerLink]="['/orders', o.id]" class="order-card">

            <!-- Restaurant logo -->
            <div class="restaurant-logo">
              <img *ngIf="o.restaurantLogoUrl" [src]="o.restaurantLogoUrl" [alt]="o.restaurantName" />
              <div *ngIf="!o.restaurantLogoUrl" class="logo-placeholder">🍽️</div>
            </div>

            <!-- Main info -->
            <div class="order-main">
              <div class="restaurant-name">{{ o.restaurantName || 'Restaurant' }}</div>
              <div class="order-meta">
                <span class="order-id">#{{ o.id | slice:0:8 | uppercase }}</span>
                <span class="dot">·</span>
                <span class="order-date">{{ o.createdAt | date:'mediumDate' }}</span>
              </div>
              <div class="order-items">
                {{ o.items.slice(0,3).map(i => i.menuItemName).join(', ') }}{{ o.items.length > 3 ? ' +' + (o.items.length - 3) + ' more' : '' }}
              </div>
            </div>

            <!-- Right side -->
            <div class="order-right">
              <div class="order-amount">₹{{ o.totalAmount | number:'1.0-0' }}</div>
              <span class="status-badge" [class]="getStatus(o).toLowerCase()">{{ getStatus(o) }}</span>
              <span class="view-link">View details →</span>
            </div>

          </a>
        </div>

        <!-- ── Recommendations ── -->
        <div class="reco-section" *ngIf="recommendations.length > 0">
          <div class="reco-header">
            <div class="reco-title-wrap">
              <span class="reco-icon">✨</span>
              <div>
                <h3 class="reco-title">Picked for you</h3>
                <p class="reco-sub">Based on your order history</p>
              </div>
            </div>
            <a routerLink="/restaurants" class="reco-see-all">See all →</a>
          </div>

          <div class="reco-grid">
            <a *ngFor="let rec of recommendations"
               [routerLink]="['/restaurants', rec.restaurant.id]"
               class="reco-card">

              <div class="reco-img">
                <img *ngIf="rec.restaurant.logoUrl" [src]="rec.restaurant.logoUrl" [alt]="rec.restaurant.name" />
                <div *ngIf="!rec.restaurant.logoUrl" class="reco-img-placeholder">🍽️</div>
                <span class="reco-open-badge" *ngIf="rec.restaurant.isOpen">● Open</span>
              </div>

              <div class="reco-body">
                <div class="reco-name">{{ rec.restaurant.name }}</div>
                <div class="reco-cuisine">{{ rec.restaurant.cuisineTypes }}</div>

                <div class="reco-meta">
                  <span class="reco-rating">⭐ {{ rec.restaurant.averageRating | number:'1.1-1' }}</span>
                  <span class="reco-dot">·</span>
                  <span class="reco-time">🕐 {{ rec.restaurant.estimatedDeliveryMinutes }} min</span>
                </div>

                <div class="reco-reason">
                  <span class="reason-icon">💡</span>
                  {{ rec.reason }}
                </div>
              </div>
            </a>
          </div>
        </div>

      </div>
    </div>
  `,
  styleUrl: './order-history.scss'
})
export class OrderHistoryComponent implements OnInit {
  orders: OrderDto[] = [];
  recommendations: Recommendation[] = [];
  loading = true;
  error = '';

  private readonly statusMap: Record<number, string> = {
    0: 'Placed', 1: 'Confirmed', 2: 'Preparing', 3: 'Ready', 4: 'PickedUp', 5: 'Delivered', 6: 'Cancelled', 7: 'Rejected'
  };

  constructor(
    private orderSvc: OrderService,
    private auth: AuthService,
    private recoSvc: RecommendationService
  ) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true; this.error = '';
    const userId = this.auth.currentUser()?.id!;
    this.orderSvc.getHistory(userId).subscribe({
      next: o => {
        this.orders = o.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        this.loading = false;
        // Load recommendations after orders are ready
        this.recoSvc.getRecommendations(userId).subscribe({
          next: recs => this.recommendations = recs,
          error: () => {}
        });
      },
      error: () => { this.error = 'Could not load orders. Make sure the server is running.'; this.loading = false; }
    });
  }

  getStatus(order: OrderDto): string {
    const s = order.status as unknown;
    return typeof s === 'number' ? (this.statusMap[s] ?? String(s)) : String(s);
  }
}
