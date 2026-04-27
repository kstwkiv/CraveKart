import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService } from '../../../core/services/order.service';
import { DeliveryService } from '../../../core/services/delivery.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderDto } from '../../../core/models/order.models';
import { DeliveryDto } from '../../../core/models/delivery.models';
import { RestaurantDto } from '../../../core/models/restaurant.models';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  styleUrl: './order-detail.scss',
  template: `
    <div *ngIf="loading" class="loading-screen">Loading order...</div>

    <div *ngIf="!order && !loading" class="not-found">
      <p>Order not found or could not be loaded.</p>
      <a routerLink="/orders">← Back to orders</a>
    </div>

    <div *ngIf="order" class="page">
      <div class="page-header">
        <div>
          <button class="back-link" (click)="goBack()">← Back</button>
          <h2>Order #{{ order.id | slice:0:8 | uppercase }}</h2>
          <p class="order-date">Placed on {{ order.createdAt | date:'medium' }}</p>
        </div>
        <div class="header-right">
          <span class="status-badge" [class]="statusStr.toLowerCase()">{{ statusStr }}</span>
          <button class="btn-cancel"
            *ngIf="statusStr === 'Placed' || statusStr === 'Confirmed'"
            (click)="cancelOrder()" [disabled]="cancelling">
            {{ cancelling ? 'Cancelling...' : 'Cancel Order' }}
          </button>
        </div>
      </div>

      <!-- Restaurant banner -->
      <div class="restaurant-banner" *ngIf="order.restaurantName">
        <div class="restaurant-logo-wrap">
          <img *ngIf="order.restaurantLogoUrl" [src]="order.restaurantLogoUrl" [alt]="order.restaurantName" />
          <div *ngIf="!order.restaurantLogoUrl" class="logo-placeholder">🍽️</div>
        </div>
        <div class="restaurant-info">
          <div class="restaurant-label">Ordered from</div>
          <div class="restaurant-name">{{ order.restaurantName }}</div>
        </div>
        <a [routerLink]="['/restaurants', order.restaurantId]" class="btn-reorder">Order again →</a>
      </div>

      <div class="content-grid">
        <div class="left-col">

          <!-- Items -->
          <div class="card">
            <div class="card-title">Items Ordered</div>
            <div *ngFor="let item of order.items" class="order-item">
              <div class="item-details">
                <div class="item-name-row">
                  <span class="item-name">{{ item.menuItemName }}</span>
                  <span class="item-qty">× {{ item.quantity }}</span>
                </div>
                <span class="item-customizations" *ngIf="item.customizations">{{ item.customizations }}</span>
              </div>
              <span class="item-price">₹{{ item.unitPrice * item.quantity | number:'1.2-2' }}</span>
            </div>
            <div class="price-breakdown">
              <div class="price-row"><span>Subtotal</span><span>₹{{ subtotal | number:'1.2-2' }}</span></div>
              <div class="price-row muted"><span>Delivery fee</span><span>₹30.00</span></div>
              <div class="price-row muted"><span>Tax (5%)</span><span>₹{{ tax | number:'1.2-2' }}</span></div>
              <div class="price-row total"><span>Total</span><span>₹{{ order.totalAmount | number:'1.2-2' }}</span></div>
            </div>
          </div>

          <!-- Order details -->
          <div class="card">
            <div class="card-title">Order Details</div>
            <div class="detail-row">
              <span class="detail-label">📍 Delivery Address</span>
              <span class="detail-val">{{ order.deliveryAddress }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">💳 Payment Method</span>
              <span class="detail-val">{{ formatPayment(order.paymentMethod) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">🕐 Placed At</span>
              <span class="detail-val">{{ order.createdAt | date:'medium' }}</span>
            </div>
          </div>

          <!-- About the restaurant -->
          <div class="card about-restaurant-card" *ngIf="restaurant">
            <div class="card-title">About the Restaurant</div>
            <div class="about-body">
              <div class="about-hero">
                <div class="about-logo">
                  <img *ngIf="restaurant.logoUrl" [src]="restaurant.logoUrl" [alt]="restaurant.name" />
                  <span *ngIf="!restaurant.logoUrl" class="about-logo-placeholder">🍽️</span>
                </div>
                <div class="about-info">
                  <div class="about-name">{{ restaurant.name }}</div>
                  <div class="about-cuisine">{{ restaurant.cuisineTypes }}</div>
                  <div class="about-rating" *ngIf="restaurant.averageRating > 0">
                    <span class="star">★</span>
                    <span class="rating-val">{{ restaurant.averageRating | number:'1.1-1' }}</span>
                    <span class="rating-count">({{ restaurant.totalReviews }} reviews)</span>
                  </div>
                </div>
              </div>

              <p class="about-desc" *ngIf="restaurant.description">{{ restaurant.description }}</p>

              <div class="about-chips">
                <div class="about-chip" *ngIf="restaurant.estimatedDeliveryMinutes">
                  <span class="chip-icon">⏱️</span>
                  <span>~{{ restaurant.estimatedDeliveryMinutes }} min delivery</span>
                </div>
                <div class="about-chip" *ngIf="restaurant.minimumOrderAmount">
                  <span class="chip-icon">🛒</span>
                  <span>Min. order ₹{{ restaurant.minimumOrderAmount }}</span>
                </div>
                <div class="about-chip" *ngIf="restaurant.address">
                  <span class="chip-icon">📍</span>
                  <span>{{ restaurant.address }}</span>
                </div>
                <div class="about-chip" *ngIf="restaurant.operatingHours">
                  <span class="chip-icon">🕐</span>
                  <span>{{ restaurant.operatingHours }}</span>
                </div>
              </div>

              <a [routerLink]="['/restaurants', restaurant.id]" class="btn-visit-restaurant">
                View full menu →
              </a>
            </div>
          </div>

          <!-- Agent location -->
          <div class="card location-card" *ngIf="delivery?.currentLat && delivery?.currentLng">
            <div class="card-title">🛵 Agent Location</div>
            <div class="location-body">
              <span class="location-pin">📍</span>
              <div>
                <p class="location-text">{{ delivery!.currentLat | number:'1.4-4' }}, {{ delivery!.currentLng | number:'1.4-4' }}</p>
                <p class="location-hint">Location updates in real-time</p>
              </div>
            </div>
          </div>

        </div>

        <!-- Tracking -->
        <div class="right-col">
          <div class="card tracking-card">
            <div class="card-title">Order Tracking</div>
            <div class="tracking-steps">
              <div class="step done" [class.active]="statusStr === 'Placed'">
                <div class="step-dot"><span>✓</span></div>
                <div class="step-body">
                  <div class="step-name">Order Placed</div>
                  <div class="step-time">{{ order.createdAt | date:'shortTime' }}</div>
                </div>
              </div>
              <div class="step" [class.done]="isAtOrPast('Confirmed')" [class.active]="statusStr === 'Confirmed'">
                <div class="step-dot"><span>✓</span></div>
                <div class="step-body">
                  <div class="step-name">Confirmed</div>
                  <div class="step-sub">Restaurant accepted your order</div>
                </div>
              </div>
              <div class="step" [class.done]="isAtOrPast('Preparing')" [class.active]="statusStr === 'Preparing'">
                <div class="step-dot"><span>🍳</span></div>
                <div class="step-body">
                  <div class="step-name">Preparing</div>
                  <div class="step-sub">Your food is being prepared</div>
                </div>
              </div>
              <div class="step" [class.done]="isAtOrPast('Ready')" [class.active]="statusStr === 'Ready'">
                <div class="step-dot"><span>📦</span></div>
                <div class="step-body">
                  <div class="step-name">Ready for Pickup</div>
                  <div class="step-sub">Waiting for delivery agent</div>
                </div>
              </div>
              <div class="step" [class.done]="isAtOrPast('PickedUp')" [class.active]="statusStr === 'PickedUp'">
                <div class="step-dot"><span>🛵</span></div>
                <div class="step-body">
                  <div class="step-name">Picked Up</div>
                  <div class="step-sub">Agent is on the way</div>
                </div>
              </div>
              <div class="step" [class.done]="statusStr === 'Delivered'" [class.active]="statusStr === 'Delivered'">
                <div class="step-dot"><span>🏠</span></div>
                <div class="step-body">
                  <div class="step-name">Delivered</div>
                  <div class="step-sub" *ngIf="delivery?.completedAt">{{ delivery!.completedAt | date:'shortTime' }}</div>
                </div>
              </div>
              <div class="step cancelled" *ngIf="statusStr === 'Cancelled' || statusStr === 'Rejected'">
                <div class="step-dot cancelled"><span>✕</span></div>
                <div class="step-body">
                  <div class="step-name">{{ statusStr }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Review section — only for delivered orders -->
      <div class="review-section" *ngIf="statusStr === 'Delivered'">
        <div *ngIf="!reviewSubmitted" class="review-card">
          <div class="review-card-header">
            <h3>⭐ How was your experience?</h3>
            <p>Your feedback helps others discover great food!</p>
          </div>
          <div class="review-card-body">
            <div class="star-row">
              <button *ngFor="let s of [1,2,3,4,5]" class="star-btn"
                [class.filled]="s <= reviewRating" (click)="reviewRating = s">★</button>
              <span class="rating-label">{{ ratingLabel }}</span>
            </div>
            <textarea [(ngModel)]="reviewText" rows="3"
              placeholder="Tell us what you loved (or didn't)... 😄"></textarea>
            <button class="btn-submit-review" (click)="submitReview()"
              [disabled]="reviewRating === 0 || submittingReview">
              {{ submittingReview ? 'Submitting...' : 'Submit Review' }}
            </button>
            <div class="review-error" *ngIf="reviewError">{{ reviewError }}</div>
          </div>
        </div>
        <div *ngIf="reviewSubmitted" class="review-thanks">
          <div class="thanks-icon">🎉</div>
          <h3>Thanks for your review!</h3>
          <p>Your feedback has been shared with the restaurant.</p>
        </div>
      </div>

    </div>
  `,
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  order?: OrderDto;
  delivery?: DeliveryDto;
  restaurant?: RestaurantDto;
  loading = true;
  cancelling = false;

  // Review
  reviewRating = 0;
  reviewText = '';
  reviewSubmitted = false;
  submittingReview = false;
  reviewError = '';

  private hub?: signalR.HubConnection;
  private pollInterval?: ReturnType<typeof setInterval>;
  private orderId = '';

  private readonly statusOrder = ['Placed', 'Confirmed', 'Preparing', 'Ready', 'PickedUp', 'Delivered'];
  private readonly terminalStatuses = ['Delivered', 'Cancelled', 'Rejected'];
  private readonly statusMap: Record<number, string> = {
    0: 'Placed', 1: 'Confirmed', 2: 'Preparing', 3: 'Ready', 4: 'PickedUp', 5: 'Delivered', 6: 'Cancelled', 7: 'Rejected'
  };

  get ratingLabel(): string {
    const labels = ['', 'Poor 😞', 'Fair 😐', 'Good 😊', 'Great 😄', 'Excellent 🤩'];
    return labels[this.reviewRating] ?? '';
  }

  constructor(
    private route: ActivatedRoute,
    private location: Location,
    private orderSvc: OrderService,
    private deliverySvc: DeliveryService,
    private restaurantSvc: RestaurantService
  ) {}

  ngOnInit() {
    this.orderId = this.route.snapshot.paramMap.get('id')!;
    if (!this.orderId) { this.loading = false; return; }
    this.orderSvc.getById(this.orderId).subscribe({
      next: o => {
        this.order = o;
        this.loading = false;
        this.loadDelivery(this.orderId);
        this.connectHub(this.orderId);
        this.pollInterval = setInterval(() => this.pollOrder(), 15_000);
        // Fetch full restaurant details for the about card
        if (o.restaurantId) {
          this.restaurantSvc.getById(o.restaurantId).subscribe({
            next: r => this.restaurant = r,
            error: () => {}
          });
        }
      },
      error: () => { this.loading = false; }
    });
  }

  get statusStr(): string {
    if (!this.order) return '';
    const s = this.order.status as unknown;
    return typeof s === 'number' ? (this.statusMap[s] ?? String(s)) : String(s);
  }

  isAtOrPast(status: string): boolean {
    return this.statusOrder.indexOf(this.statusStr) >= this.statusOrder.indexOf(status);
  }

  get subtotal(): number { return this.order?.items.reduce((s, i) => s + i.unitPrice * i.quantity, 0) ?? 0; }
  get tax(): number { return Math.round(this.subtotal * 0.05); }

  goBack() { this.location.back(); }

  formatPayment(method: string): string {
    const map: Record<string, string> = {
      CashOnDelivery: '💵 Cash on Delivery',
      Card: '💳 Card',
    };
    return map[method] ?? method;
  }

  submitReview() {
    if (!this.order || this.reviewRating === 0) return;
    this.submittingReview = true;
    this.reviewError = '';
    this.restaurantSvc.createReview({
      restaurantId: this.order.restaurantId,
      orderId: this.order.id,
      rating: this.reviewRating,
      reviewText: this.reviewText.trim() || undefined
    }).subscribe({
      next: () => { this.reviewSubmitted = true; this.submittingReview = false; },
      error: (err) => {
        this.reviewError = err.error?.message || err.error || 'Failed to submit review. You may have already reviewed this order.';
        this.submittingReview = false;
      }
    });
  }

  cancelOrder() {
    if (!this.order || !confirm('Cancel this order?')) return;
    this.cancelling = true;
    this.orderSvc.cancel(this.order.id).subscribe({
      next: () => {
        // Use numeric value so statusStr resolves correctly via statusMap
        this.order = { ...this.order!, status: 6 as any }; // 6 = Cancelled
        this.cancelling = false;
        this.stopPolling();
      },
      error: () => this.cancelling = false
    });
  }

  private pollOrder() {
    if (!this.orderId) return;
    this.orderSvc.getById(this.orderId).subscribe({
      next: o => {
        this.order = o;
        // Stop polling once we reach a terminal state
        if (this.terminalStatuses.includes(this.statusStr)) this.stopPolling();
      },
      error: () => {}
    });
  }

  private stopPolling() {
    if (this.pollInterval) { clearInterval(this.pollInterval); this.pollInterval = undefined; }
  }

  private loadDelivery(orderId: string) {
    this.deliverySvc.getByOrder(orderId).subscribe({ next: d => this.delivery = d, error: () => {} });
  }

  private connectHub(orderId: string) {
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.deliveryHubUrl).withAutomaticReconnect().build();
    this.hub.on('LocationUpdated', (data: { orderId: string; lat: number; lng: number }) => {
      if (data.orderId === orderId && this.delivery)
        this.delivery = { ...this.delivery, currentLat: data.lat, currentLng: data.lng };
    });
    this.hub.on('DeliveryCompleted', (data: { orderId: string }) => {
      if (data.orderId === orderId) {
        if (this.order) this.order = { ...this.order, status: 5 as any }; // 5 = Delivered
        if (this.delivery) this.delivery = { ...this.delivery, status: 'Delivered', completedAt: new Date().toISOString() };
        this.stopPolling();
      }
    });
    this.hub.start().catch(() => {});
  }

  ngOnDestroy() {
    this.hub?.stop();
    this.stopPolling();
  }
}
