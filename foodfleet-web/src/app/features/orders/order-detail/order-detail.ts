// import: ES module keyword — pulls named exports from external modules into this file's scope
import { Component, OnInit, OnDestroy } from '@angular/core';
// CommonModule: provides Angular built-in directives like *ngIf, *ngFor, pipes like | date, | number
import { CommonModule } from '@angular/common';
// RouterLink: directive that turns an anchor/button into a client-side navigation link (no full page reload)
import { RouterLink, ActivatedRoute } from '@angular/router';
// Location: Angular service wrapping the browser History API — used for programmatic back navigation
import { Location } from '@angular/common';
// FormsModule: enables template-driven forms and the [(ngModel)] two-way data binding directive
import { FormsModule } from '@angular/forms';
// Service imports: each import brings a singleton injectable class into scope for dependency injection
import { OrderService } from '../../../core/services/order.service';
import { DeliveryService } from '../../../core/services/delivery.service';
import { RestaurantService } from '../../../core/services/restaurant.service';
// PaymentDto: a TypeScript interface (data shape contract) exported alongside the service
import { PaymentService, PaymentDto } from '../../../core/services/payment.service';
// OrderDto, DeliveryDto, RestaurantDto: plain TypeScript interfaces describing API response shapes
import { OrderDto } from '../../../core/models/order.models';
import { DeliveryDto } from '../../../core/models/delivery.models';
import { RestaurantDto } from '../../../core/models/restaurant.models';
// import * as: namespace import — all exports of the signalR package are accessible under the `signalR` object
import * as signalR from '@microsoft/signalr';
// environment: a compile-time constant object that switches between dev/prod configuration values
import { environment } from '../../../../environments/environment';

/**
 * Order detail component.
 * Displays full order information including items, price breakdown, delivery
 * address, payment status, and a live tracking timeline. Connects to the
 * SignalR delivery hub for real-time location updates. Polls the order and
 * payment status until a terminal state is reached. Allows customers to
 * cancel eligible orders and submit a review after delivery.
 */
// @Component: Angular decorator — metadata annotation that tells Angular's compiler this class is a component
@Component({
  selector: 'app-order-detail',   // selector: the custom HTML tag name used to embed this component in templates
  standalone: true,               // standalone: true means this component does not belong to an NgModule; it manages its own imports
  imports: [CommonModule, RouterLink, FormsModule], // imports: declares which Angular modules/directives this standalone component depends on
  styleUrl: './order-detail.scss', // styleUrl: path to the component-scoped CSS file (styles are encapsulated by default)
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

          <!-- Payment status card — shows when record loaded -->
          <div class="card payment-card" *ngIf="payment">
            <div class="card-title">💳 Payment</div>
            <div class="payment-body">
              <div class="payment-status-row">
                <span class="payment-status-badge" [class]="payment.status.toLowerCase()">
                  {{ paymentStatusIcon }} {{ payment.status }}
                </span>
                <span class="payment-amount">₹{{ payment.amount | number:'1.2-2' }}</span>
              </div>
              <div class="payment-meta">
                <div class="payment-meta-row">
                  <span class="pm-label">Method</span>
                  <span class="pm-val">{{ formatPayment(payment.paymentMethod) }}</span>
                </div>
                <div class="payment-meta-row">
                  <span class="pm-label">Currency</span>
                  <span class="pm-val">{{ payment.currency }}</span>
                </div>
                <div class="payment-meta-row">
                  <span class="pm-label">Processed</span>
                  <span class="pm-val">{{ payment.processedAt | date:'medium' }}</span>
                </div>
                <div class="payment-meta-row" *ngIf="payment.status === 'Refunded'">
                  <span class="pm-label refund-label">Refund</span>
                  <span class="pm-val refund-val">₹{{ payment.amount | number:'1.2-2' }} refunded</span>
                </div>
              </div>
            </div>
          </div>

          <!-- No payment record yet — loading or polling -->
          <div class="card payment-card" *ngIf="!payment">
            <div class="card-title">💳 Payment</div>
            <div class="payment-pending upi-confirming">
              <span class="pending-spinner" [class.upi-color]="isUpiOrder"></span>
              <div>
                <div class="upi-confirm-text">
                  {{ paymentLoading ? 'Loading payment...' : (isUpiOrder ? 'Confirming UPI payment...' : 'Awaiting payment...') }}
                </div>
                <div class="upi-confirm-sub">
                  {{ isUpiOrder ? 'This usually takes a few seconds' : 'Payment will be collected on delivery' }}
                </div>
              </div>
            </div>
          </div>
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
// export: makes this class available for import in other modules/files
// class: TypeScript/ES6 keyword — defines a blueprint (reference type) with properties and methods
// implements: TypeScript keyword — enforces that the class satisfies the listed interface contracts
// OnInit: Angular lifecycle interface — requires ngOnInit() to be defined; called once after first change detection
// OnDestroy: Angular lifecycle interface — requires ngOnDestroy() to be defined; called just before the component is removed from the DOM
export class OrderDetailComponent implements OnInit, OnDestroy {
  // ?: optional property — the type is `OrderDto | undefined`; the property may not be set yet
  order?: OrderDto;
  delivery?: DeliveryDto;
  restaurant?: RestaurantDto;
  // PaymentDto: a TypeScript interface used as a type annotation — describes the shape of a payment object
  payment?: PaymentDto;
  // boolean: primitive type — can only be true or false; used for flags/toggles
  paymentLoading = true;
  loading = true;
  cancelling = false;

  // Review state fields
  // number: primitive type — represents IEEE 754 double-precision floating-point values
  reviewRating = 0;
  // string: primitive type — represents a sequence of UTF-16 characters
  reviewText = '';
  reviewSubmitted = false;
  submittingReview = false;
  reviewError = '';

  // private: access modifier — restricts visibility to within this class only; not accessible from outside
  private hub?: signalR.HubConnection;
  // ReturnType<typeof setInterval>: utility type — infers the return type of setInterval (a numeric timer ID)
  private pollInterval?: ReturnType<typeof setInterval>;
  private paymentPollInterval?: ReturnType<typeof setInterval>;
  private orderId = '';

  // readonly: modifier — the property can be assigned once (at declaration or in the constructor) but never mutated afterward
  // Array (string[]): ordered collection type — elements are accessed by numeric index
  private readonly statusOrder = ['Placed', 'Confirmed', 'Preparing', 'Ready', 'PickedUp', 'Delivered'];
  private readonly terminalStatuses = ['Delivered', 'Cancelled', 'Rejected'];
  // Record<number, string>: TypeScript utility type — creates an object type whose keys are numbers and values are strings
  private readonly statusMap: Record<number, string> = {
    0: 'Placed', 1: 'Confirmed', 2: 'Preparing', 3: 'Ready', 4: 'PickedUp', 5: 'Delivered', 6: 'Cancelled', 7: 'Rejected'
  };

  // get: TypeScript accessor keyword — defines a computed property that is evaluated on every read
  // string: return type annotation — this getter always returns a string value
  get ratingLabel(): string {
    // const: block-scoped constant — the binding cannot be reassigned after initialisation
    const labels = ['', 'Poor 😞', 'Fair 😐', 'Good 😊', 'Great 😄', 'Excellent 🤩'];
    // return: exits the function and sends the specified value back to the caller
    // ??: nullish coalescing operator — returns the right-hand side when the left is null or undefined
    return labels[this.reviewRating] ?? '';
  }

  // constructor: special method called once when Angular's DI system instantiates the class
  // private: DI shorthand — declares and assigns the injected service as a private class field simultaneously
  constructor(
    private route: ActivatedRoute,       // ActivatedRoute: provides access to the current URL params and query params
    private location: Location,           // Location: Angular wrapper around the browser History API
    private orderSvc: OrderService,       // OrderService: injectable service for order-related HTTP calls
    private deliverySvc: DeliveryService, // DeliveryService: injectable service for delivery-related HTTP calls
    private restaurantSvc: RestaurantService, // RestaurantService: injectable service for restaurant HTTP calls
    private paymentSvc: PaymentService    // PaymentService: injectable service for payment HTTP calls
  ) {}

  // ngOnInit: Angular lifecycle hook — called by the framework after the component's inputs are first set
  ngOnInit() {
    // !: non-null assertion operator — tells TypeScript the value is definitely not null/undefined here
    this.orderId = this.route.snapshot.paramMap.get('id')!;
    // if: conditional control-flow — executes the block only when the condition is truthy
    if (!this.orderId) { this.loading = false; return; }
    // .subscribe(): Observable method — registers observer callbacks (next/error/complete) to consume the async stream
    this.orderSvc.getById(this.orderId).subscribe({
      // next: callback invoked for each emitted value from the Observable
      next: o => {
        this.order = o;
        this.loading = false;
        this.loadDelivery(this.orderId);
        this.loadPayment(this.orderId);
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
      // error: callback invoked when the Observable emits an error notification
      error: () => { this.loading = false; }
    });
  }

  // get statusStr: computed property — converts the numeric status enum to a human-readable string
  get statusStr(): string {
    // null check: if order is falsy, return an empty string to avoid runtime errors
    if (!this.order) return '';
    // as unknown: type assertion — widens the type to `unknown` before narrowing, bypassing strict checks
    const s = this.order.status as unknown;
    // typeof: runtime type-check operator — returns a string describing the operand's primitive type
    return typeof s === 'number' ? (this.statusMap[s] ?? String(s)) : String(s);
  }

  // boolean return type: this method answers a yes/no question about ordering in the status sequence
  isAtOrPast(status: string): boolean {
    return this.statusOrder.indexOf(this.statusStr) >= this.statusOrder.indexOf(status);
  }

  // number return type: computed property returning a numeric subtotal
  get subtotal(): number { return this.order?.items.reduce((s, i) => s + i.unitPrice * i.quantity, 0) ?? 0; }
  get tax(): number { return Math.round(this.subtotal * 0.05); }

  // void: return type indicating the function produces no value (side-effect only)
  goBack() { this.location.back(); }

  // Record<string, string>: maps payment method keys to display strings
  formatPayment(method: string): string {
    const map: Record<string, string> = {
      UpiNow: '📲 UPI (Paid)',
      CashOnDelivery: '💵 Cash on Delivery',
      Card: '💳 Card',
    };
    return map[method] ?? method;
  }

  submitReview() {
    // null/undefined guard: early return prevents calling methods on an undefined object
    if (!this.order || this.reviewRating === 0) return;
    this.submittingReview = true;
    this.reviewError = '';
    this.restaurantSvc.createReview({
      restaurantId: this.order.restaurantId,
      orderId: this.order.id,
      rating: this.reviewRating,
      // undefined: the absence of a value — used here to omit the field when the text is empty
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

  // private: this method is an internal implementation detail not exposed to the template or parent components
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

  private loadPayment(orderId: string) {
    this.paymentSvc.getByOrder(orderId).subscribe({
      next: p => {
        this.payment = p;
        this.paymentLoading = false;
        // If UPI payment is still Pending, poll until Confirmed
        if (p.status === 'Pending' && this.isUpiOrder) {
          this.pollPayment(orderId);
        }
      },
      error: () => {
        this.paymentLoading = false;
        // Payment record not yet created — poll for it (covers race condition)
        this.pollPayment(orderId);
      }
    });
  }

  // Poll every 2s until payment record is found and confirmed (max 20s)
  private pollPayment(orderId: string) {
    // let: block-scoped variable — unlike const, its value can be reassigned
    let attempts = 0;
    this.paymentPollInterval = setInterval(() => {
      attempts++;
      this.paymentSvc.getByOrder(orderId).subscribe({
        next: p => {
          this.payment = p;
          this.paymentLoading = false;
          if (p.status === 'Confirmed' || p.status === 'Failed' || attempts >= 10) {
            clearInterval(this.paymentPollInterval);
          }
        },
        error: () => {
          this.paymentLoading = false;
          if (attempts >= 10) clearInterval(this.paymentPollInterval);
        }
      });
    }, 2000);
  }

  // boolean getter: returns true/false based on the payment method of the current order
  get isUpiOrder(): boolean {
    // UpiNow = new name, Card = old enum value 0 (same position), both mean instant payment
    const m = this.order?.paymentMethod;
    return m === 'UpiNow' || m === 'Card';
  }

  // string getter: maps payment status to a display icon character
  get paymentStatusIcon(): string {
    // Record<string, string>: object literal used as a lookup table (dictionary pattern)
    const icons: Record<string, string> = {
      Confirmed: '✅', Pending: '⏳', Failed: '❌', Refunded: '💸'
    };
    return icons[this.payment?.status ?? ''] ?? '💳';
  }

  private connectHub(orderId: string) {
    // new: instantiation keyword — allocates memory and calls the constructor of the given class
    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(environment.deliveryHubUrl).withAutomaticReconnect().build();
    // .on(): SignalR method — registers a handler for a named server-push event (pub/sub pattern)
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
    // .start(): returns a Promise — initiates the WebSocket/long-polling connection to the SignalR hub
    this.hub.start().catch(() => {});
  }

  // ngOnDestroy: Angular lifecycle hook — called just before Angular destroys the component; ideal for cleanup
  ngOnDestroy() {
    // ?.: optional chaining — calls .stop() only if hub is not null/undefined, preventing a runtime error
    this.hub?.stop();
    this.stopPolling();
    if (this.paymentPollInterval) clearInterval(this.paymentPollInterval);
  }
}
