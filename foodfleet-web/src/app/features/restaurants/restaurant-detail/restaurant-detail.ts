// import: ES module keyword — pulls named exports from external packages/files into this file's scope
// Component: Angular decorator factory — attaches component metadata to the class below
// OnInit: Angular lifecycle interface — requires ngOnInit() to be implemented; called after the first change-detection cycle
import { Component, OnInit } from '@angular/core';
// CommonModule: provides *ngIf, *ngFor, and built-in pipes (| date, | number, | slice) for use in templates
import { CommonModule } from '@angular/common';
// FormsModule: enables template-driven forms and the [(ngModel)] two-way data-binding directive
import { FormsModule } from '@angular/forms';
// ActivatedRoute: service exposing the current route's URL params; Router: service for programmatic navigation
// RouterLink: directive that turns an element into a client-side navigation link (no full page reload)
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
// RestaurantService: injectable service for restaurant-related HTTP calls (menu, reviews, etc.)
import { RestaurantService } from '../../../core/services/restaurant.service';
// OrderService: injectable service for placing and managing orders
import { OrderService } from '../../../core/services/order.service';
// AuthService: injectable service that exposes the current user's authentication state and identity
import { AuthService } from '../../../core/services/auth.service';
// AddressService: injectable service for persisting delivery addresses; SavedAddress: interface describing a saved address record
import { AddressService, SavedAddress } from '../../../core/services/address.service';
// RestaurantDto, MenuCategoryDto, MenuItemDto, ReviewDto: TypeScript interfaces — structural type contracts for API response shapes
import { RestaurantDto, MenuCategoryDto, MenuItemDto, ReviewDto } from '../../../core/models/restaurant.models';

// interface: TypeScript keyword — defines a pure structural type contract (no runtime code generated)
// CartItem: a local interface pairing a menu item with its selected quantity in the cart
/** A cart item pairing a menu item with its selected quantity. */
interface CartItem { item: MenuItemDto; qty: number; }

/**
 * Restaurant detail component.
 * Displays the restaurant hero banner, info bar, paginated menu by category,
 * customer reviews, and a sticky cart panel. Handles cart management,
 * saved delivery addresses, payment method selection, order placement,
 * and a UPI payment simulation modal.
 */
// @Component: Angular decorator — attaches component metadata (selector, template, styles, imports) to the class below
@Component({
  selector: 'app-restaurant-detail', // selector: the custom HTML tag name used to embed this component in a parent template
  standalone: true,                   // standalone: true — self-contained; no NgModule declaration required
  imports: [CommonModule, FormsModule, RouterLink], // imports: Angular modules/directives this standalone component depends on
  template: `
    <div *ngIf="loading" class="loading-screen"><div class="spinner-wrap">🍽️ Loading restaurant...</div></div>
    <div *ngIf="!restaurant && !loading" class="not-found">
      <div class="nf-icon">🍽️</div><h2>Restaurant not found</h2>
      <a routerLink="/restaurants" class="btn-back-home">← Back to restaurants</a>
    </div>

    <div *ngIf="restaurant && !loading" class="detail-page">
      <div class="hero-banner" [style.backgroundImage]="restaurant.logoUrl ? 'url(' + restaurant.logoUrl + ')' : 'none'">
        <div class="hero-overlay">
          <div class="hero-inner">
            <a routerLink="/restaurants" class="back-btn">← Back to restaurants</a>
            <div class="hero-content">
              <div class="restaurant-logo" *ngIf="restaurant.logoUrl"><img [src]="restaurant.logoUrl" [alt]="restaurant.name" /></div>
              <div class="restaurant-logo placeholder" *ngIf="!restaurant.logoUrl">🍽️</div>
              <div class="hero-text">
                <h1>{{ restaurant.name }}</h1>
                <p class="cuisine-tag">🍴 {{ restaurant.cuisineTypes }}</p>
                <p class="description">{{ restaurant.description }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="info-bar">
        <div class="info-bar-inner">
          <div class="info-chip"><span class="chip-icon">⭐</span><div><span class="chip-val">{{ restaurant.averageRating | number:'1.1-1' }}</span><span class="chip-lbl">{{ restaurant.totalReviews }} reviews</span></div></div>
          <div class="info-divider"></div>
          <div class="info-chip"><span class="chip-icon">🕐</span><div><span class="chip-val">{{ restaurant.estimatedDeliveryMinutes }} min</span><span class="chip-lbl">Delivery time</span></div></div>
          <div class="info-divider"></div>
          <div class="info-chip"><span class="chip-icon">₹</span><div><span class="chip-val">₹{{ restaurant.minimumOrderAmount }}</span><span class="chip-lbl">Min order</span></div></div>
          <div class="info-divider"></div>
          <div class="info-chip"><span class="chip-icon">📍</span><div><span class="chip-val address">{{ restaurant.address }}</span><span class="chip-lbl">Address</span></div></div>
          <div class="info-divider"></div>
          <div class="status-pill" [class.open]="restaurant.isOpen" [class.closed]="!restaurant.isOpen">
            <span class="dot"></span>{{ restaurant.isOpen ? 'Open Now' : 'Closed' }}
          </div>
        </div>
      </div>

      <div class="main-content">
        <div class="menu-col">
          <div *ngIf="menu.length === 0" class="empty-menu"><div class="em-icon">🍽️</div><p>Menu not available yet.</p></div>

          <div *ngFor="let cat of menu" class="category-section">
            <div class="category-title"><span>{{ cat.name }}</span><span class="item-count">{{ cat.items.length }} items</span></div>
            <div class="items-grid">
              <div *ngFor="let item of pagedItems(cat)" class="item-card" [class.unavailable]="!item.isAvailable">
                <div class="item-img-wrap">
                  <img *ngIf="item.imageUrl" [src]="item.imageUrl" [alt]="item.name" class="item-img" />
                  <div *ngIf="!item.imageUrl" class="item-img-placeholder">🍽️</div>
                  <div *ngIf="!item.isAvailable" class="unavail-badge">Unavailable</div>
                </div>
                <div class="item-body">
                  <div class="item-name">{{ item.name }}</div>
                  <div class="item-desc" *ngIf="item.description">{{ item.description }}</div>
                  <div class="item-tags" *ngIf="item.dietaryTags">
                    <span *ngFor="let tag of item.dietaryTags.split(',')" class="tag">{{ tag.trim() }}</span>
                  </div>
                  <div class="item-footer">
                    <span class="item-price">₹{{ item.price }}</span>
                    <button class="btn-add" (click)="addToCart(item)" [disabled]="!item.isAvailable">
                      <span *ngIf="!inCart(item)">+ Add</span>
                      <span *ngIf="inCart(item)">{{ getQty(item) }} in cart</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
            <!-- Category pagination -->
            <div class="cat-pagination" *ngIf="catTotalPages(cat) > 1">
              <button class="page-btn" (click)="catGoTo(cat.id, catPage(cat.id) - 1)" [disabled]="catPage(cat.id) === 1">‹</button>
              <span class="page-info">{{ catPage(cat.id) }} / {{ catTotalPages(cat) }}</span>
              <button class="page-btn" (click)="catGoTo(cat.id, catPage(cat.id) + 1)" [disabled]="catPage(cat.id) === catTotalPages(cat)">›</button>
            </div>
          </div>

          <div class="reviews-section">
            <h3>⭐ Customer Reviews <span class="review-count">({{ reviews.length }})</span></h3>
            <div *ngFor="let r of reviews" class="review-card">
              <div class="review-top">
                <div class="reviewer-avatar">{{ r.customerName.charAt(0) }}</div>
                <div class="reviewer-info"><span class="reviewer-name">{{ r.customerName }}</span><span class="review-date">{{ r.createdAt | date:'mediumDate' }}</span></div>
                <div class="stars"><span *ngFor="let s of [1,2,3,4,5]" [class.filled]="s <= r.rating">★</span></div>
              </div>
              <p class="review-text" *ngIf="r.reviewText">{{ r.reviewText }}</p>
              <div class="owner-reply" *ngIf="r.ownerResponse"><span class="reply-label">🏪 Owner replied:</span> {{ r.ownerResponse }}</div>
            </div>
            <div *ngIf="reviews.length === 0" class="no-reviews">No reviews yet — be the first!</div>
          </div>
        </div>

        <div class="cart-col">
          <div class="cart-panel">
            <div class="cart-header">
              <h3>Your Order</h3>
              <span class="cart-badge" *ngIf="cart.length > 0">{{ totalItems }}</span>
            </div>
            <div *ngIf="cart.length === 0" class="cart-empty"><div class="ce-icon">🛒</div><p>Add items from the menu to start your order</p></div>
            <div *ngIf="cart.length > 0">
              <div *ngFor="let c of cart" class="cart-row">
                <div class="cart-item-info"><span class="cart-item-name">{{ c.item.name }}</span><span class="cart-item-price">₹{{ c.item.price * c.qty }}</span></div>
                <div class="qty-ctrl">
                  <button (click)="dec(c)">−</button><span>{{ c.qty }}</span><button (click)="inc(c)">+</button>
                </div>
              </div>
              <div class="cart-summary">
                <div class="summary-row"><span>Subtotal</span><span>₹{{ total }}</span></div>
                <div class="summary-row muted"><span>Delivery fee</span><span>Free</span></div>
                <div class="summary-total"><span>Total</span><span>₹{{ total }}</span></div>
              </div>

              <!-- ── Minimum order warning ── -->
              <div class="min-order-warning" *ngIf="belowMinimum">
                <div class="min-warn-top">
                  <span class="min-warn-icon">⚠️</span>
                  <div class="min-warn-text">
                    <strong>Minimum order not met</strong>
                    <span>Add ₹{{ amountNeeded | number:'1.0-0' }} more to place your order
                      <span class="min-progress-label">(₹{{ total }} / ₹{{ restaurant!.minimumOrderAmount }})</span>
                    </span>
                  </div>
                </div>

                <!-- Progress bar -->
                <div class="min-progress-track">
                  <div class="min-progress-fill" [style.width.%]="progressPercent"></div>
                </div>

                <!-- Suggested dishes -->
                <div class="min-suggestions" *ngIf="suggestedItems.length > 0">
                  <div class="min-sugg-label">🍽️ Add to reach minimum:</div>
                  <div class="min-sugg-list">
                    <div *ngFor="let s of suggestedItems" class="min-sugg-item">
                      <div class="sugg-info">
                        <span class="sugg-name">{{ s.name }}</span>
                        <span class="sugg-price">₹{{ s.price }}</span>
                      </div>
                      <button class="sugg-add-btn" (click)="addToCart(s)">+ Add</button>
                    </div>
                  </div>
                </div>
              </div>

              <div class="addr-section">
                <label>📍 Delivery Address</label>

                <!-- Saved addresses list -->
                <div class="saved-addresses" *ngIf="savedAddresses.length > 0">
                  <div *ngFor="let addr of savedAddresses"
                    class="addr-chip"
                    [class.selected]="deliveryAddress === addr.text"
                    (click)="selectAddress(addr)">
                    <span class="addr-chip-icon">{{ addr.isDefault ? '🏠' : '📍' }}</span>
                    <span class="addr-chip-text">{{ addr.text }}</span>
                    <span class="addr-default-badge" *ngIf="addr.isDefault">Default</span>
                    <button class="addr-chip-remove" (click)="removeAddress($event, addr.id)" title="Remove">×</button>
                  </div>
                </div>

                <!-- Empty state when no saved addresses -->
                <div class="addr-empty-hint" *ngIf="savedAddresses.length === 0 && !showNewAddress">
                  <span class="addr-empty-icon">📭</span>
                  <span>No saved addresses yet</span>
                </div>

                <!-- Add new address toggle button -->
                <button class="btn-add-address" (click)="toggleNewAddress()" *ngIf="!showNewAddress">
                  + Add new address
                </button>

                <!-- Inline new address form -->
                <div class="new-addr-form" *ngIf="showNewAddress">
                  <textarea
                    [(ngModel)]="newAddressText"
                    (ngModelChange)="onNewAddressType($event)"
                    rows="2"
                    placeholder="e.g. 42, MG Road, Bengaluru, Karnataka 560001">
                  </textarea>
                  <div class="new-addr-actions">
                    <button class="btn-save-address"
                      (click)="saveNewAddress()"
                      [disabled]="!newAddressText.trim()">
                      Save &amp; use
                    </button>
                    <button class="btn-cancel-address" (click)="toggleNewAddress()">
                      Cancel
                    </button>
                  </div>
                </div>

                <!-- Currently selected address display -->
                <div class="addr-selected-display" *ngIf="deliveryAddress && !showNewAddress">
                  <span class="addr-selected-label">Delivering to:</span>
                  <span class="addr-selected-val">{{ deliveryAddress }}</span>
                </div>
              </div>
              <div class="payment-section">
                <label>How would you like to pay?</label>
                <div class="payment-options">

                  <!-- UPI Now -->
                  <label class="payment-option" [class.selected]="paymentMethod === 0">
                    <input type="radio" name="payment" [value]="0" [(ngModel)]="paymentMethod" />
                    <div class="po-icon-wrap upi">
                      <span class="po-icon">📲</span>
                    </div>
                    <div class="po-body">
                      <span class="po-title">Pay via UPI</span>
                      <span class="po-sub">Instant · Secure · Confirmed now</span>
                    </div>
                    <span class="po-badge instant">Instant</span>
                  </label>

                  <!-- Cash on Delivery -->
                  <label class="payment-option" [class.selected]="paymentMethod === 1">
                    <input type="radio" name="payment" [value]="1" [(ngModel)]="paymentMethod" />
                    <div class="po-icon-wrap cod">
                      <span class="po-icon">💵</span>
                    </div>
                    <div class="po-body">
                      <span class="po-title">Pay at Doorstep</span>
                      <span class="po-sub">Cash on delivery</span>
                    </div>
                    <span class="po-badge cod">COD</span>
                  </label>

                </div>

                <!-- UPI info hint -->
                <div class="upi-hint" *ngIf="paymentMethod === 0">
                  <span class="upi-hint-icon">ℹ️</span>
                  A UPI payment screen will appear after you place the order.
                </div>
                <div class="cod-hint" *ngIf="paymentMethod === 1">
                  <span class="cod-hint-icon">🏠</span>
                  Keep exact change ready. Payment confirmed after delivery.
                </div>
              </div>
              <div class="error-msg" *ngIf="orderError">{{ orderError }}</div>
              <button class="btn-place-order" (click)="placeOrder()"
                [disabled]="placing || !deliveryAddress.trim() || belowMinimum">
                <span *ngIf="!placing && !belowMinimum">Place Order · ₹{{ total }}</span>
                <span *ngIf="!placing && belowMinimum">Add ₹{{ amountNeeded | number:'1.0-0' }} more to order</span>
                <span *ngIf="placing">Placing order...</span>
              </button>
              <button class="btn-clear" (click)="cart = []">Clear cart</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- ── UPI Payment Modal ── -->
    <div class="upi-modal-backdrop" *ngIf="showUpiModal" (click)="skipUpiPayment()">
      <div class="upi-modal" (click)="$event.stopPropagation()">
        <div class="upi-modal-header">
          <div class="upi-modal-icon">📲</div>
          <h3>Complete UPI Payment</h3>
          <p>₹{{ total | number:'1.2-2' }} · {{ restaurant?.name }}</p>
        </div>

        <div class="upi-modal-body">
          <!-- Success screen -->
          <div class="upi-success" *ngIf="upiSuccess">
            <div class="upi-success-icon">✅</div>
            <h4>Payment Confirmed!</h4>
            <p>₹{{ total | number:'1.2-2' }} paid via UPI</p>
            <p class="upi-success-sub">A confirmation email has been sent to you.</p>
            <div class="upi-success-bar">
              <div class="upi-success-fill"></div>
            </div>
            <p class="upi-success-redirect">Redirecting to your order...</p>
          </div>

          <!-- Payment form -->
          <ng-container *ngIf="!upiSuccess">
            <!-- QR code placeholder -->
            <div class="upi-qr-wrap">
              <div class="upi-qr">
                <div class="upi-qr-inner">
                  <span class="upi-qr-icon">⬛</span>
                  <p class="upi-qr-label">Scan with any UPI app</p>
                  <div class="upi-apps">
                    <span class="upi-app">GPay</span>
                    <span class="upi-app">PhonePe</span>
                    <span class="upi-app">Paytm</span>
                    <span class="upi-app">BHIM</span>
                  </div>
                </div>
              </div>
            </div>

            <div class="upi-divider"><span>or enter UPI ID</span></div>

            <div class="upi-input-wrap">
              <input
                type="text"
                [(ngModel)]="upiId"
                placeholder="yourname@upi"
                class="upi-input"
                [class.error]="upiError"
                (keyup.enter)="confirmUpiPayment()" />
              <span class="upi-input-icon">@</span>
            </div>
            <p class="upi-error" *ngIf="upiError">{{ upiError }}</p>

            <button class="upi-pay-btn" (click)="confirmUpiPayment()" [disabled]="upiVerifying">
              <span *ngIf="!upiVerifying">✅ Pay ₹{{ total | number:'1.2-2' }}</span>
              <span *ngIf="upiVerifying" class="upi-verifying">
                <span class="upi-spinner"></span> Verifying payment...
              </span>
            </button>

            <button class="upi-skip-btn" (click)="skipUpiPayment()">
              Pay later / Skip for now
            </button>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styleUrl: './restaurant-detail.scss' // styleUrl: path to the component-scoped SCSS file; styles are encapsulated and do not leak globally
})
// export: makes this class importable by the router's lazy loadComponent mechanism
// class: TypeScript/ES6 keyword — defines a named reference type with state (fields) and behaviour (methods)
// implements OnInit: TypeScript keyword — enforces the lifecycle interface; the compiler errors if ngOnInit is missing
export class RestaurantDetailComponent implements OnInit {
  // ?: optional property — the type is `RestaurantDto | undefined`; not yet set until the HTTP response arrives
  restaurant?: RestaurantDto;
  // MenuCategoryDto[]: typed array — an ordered list of menu category objects
  menu: MenuCategoryDto[] = [];
  // ReviewDto[]: typed array — an ordered list of customer review objects
  reviews: ReviewDto[] = [];
  // CartItem[]: typed array — the in-memory shopping cart; each element pairs a menu item with a quantity
  cart: CartItem[] = [];
  // string: primitive type — holds the delivery address entered or selected by the user
  deliveryAddress = '';
  // number: primitive type — 0 = UPI, 1 = Cash on Delivery; represents the selected payment method enum value
  paymentMethod = 0;
  // boolean: primitive type — true while the place-order HTTP request is in flight
  placing = false;
  // string: primitive type — holds an error message to display if order placement fails
  orderError = '';
  // boolean: primitive type — true while the initial restaurant data is loading
  loading = true;

  // SavedAddress[]: typed array — the list of addresses previously saved by the user
  savedAddresses: SavedAddress[] = [];
  // boolean: controls visibility of the inline new-address form
  showNewAddress = false;
  // string: holds the text typed into the new-address textarea
  newAddressText = '';

  // Menu item pagination — per category
  // readonly: modifier — this constant cannot be reassigned after declaration
  // number: the maximum number of menu items shown per page per category
  readonly ITEMS_PER_PAGE = 9;
  // private: access modifier — internal state not exposed to the template directly
  // Record<string, number>: TypeScript utility type — maps category IDs (strings) to their current page numbers
  private catPages: Record<string, number> = {};

  // number return type: returns the current page number for a given category (defaults to 1)
  catPage(catId: string): number { return this.catPages[catId] ?? 1; }
  // number return type: calculates total pages by dividing item count by page size (ceiling division)
  catTotalPages(cat: MenuCategoryDto): number { return Math.ceil(cat.items.length / this.ITEMS_PER_PAGE); }
  // MenuItemDto[]: returns the slice of items for the current page of the given category
  pagedItems(cat: MenuCategoryDto): MenuItemDto[] {
    // const: block-scoped constant — the binding cannot be reassigned
    const page = this.catPage(cat.id);
    const start = (page - 1) * this.ITEMS_PER_PAGE;
    // Array.slice(): returns a shallow copy of a portion of the array without mutating the original
    return cat.items.slice(start, start + this.ITEMS_PER_PAGE);
  }
  catGoTo(catId: string, page: number) {
    const cat = this.menu.find(c => c.id === catId);
    // if: guard — exits early if the category is not found or the page is out of bounds
    if (!cat) return;
    const max = this.catTotalPages(cat);
    if (page < 1 || page > max) return;
    // Spread operator (...): creates a new object with all existing catPages entries plus the updated page for catId
    this.catPages = { ...this.catPages, [catId]: page };
  }

  // ── Minimum order helpers ─────────────────────────────────────────────────
  // get: accessor keyword — defines a computed property evaluated on every read (no parentheses needed in template)
  // boolean: return type — answers whether the cart total is below the restaurant's minimum order amount
  get belowMinimum(): boolean {
    // ?: optional chaining — safely accesses minimumOrderAmount without throwing if restaurant is undefined
    if (!this.restaurant?.minimumOrderAmount || this.cart.length === 0) return false;
    return this.total < this.restaurant.minimumOrderAmount;
  }

  // number: return type — the additional amount the user must add to meet the minimum order requirement
  get amountNeeded(): number {
    if (!this.restaurant?.minimumOrderAmount) return 0;
    return Math.max(0, this.restaurant.minimumOrderAmount - this.total);
  }

  // number: return type — percentage of the minimum order already filled (capped at 100)
  get progressPercent(): number {
    if (!this.restaurant?.minimumOrderAmount) return 100;
    return Math.min(100, Math.round((this.total / this.restaurant.minimumOrderAmount) * 100));
  }

  /** Items not already in cart, available, sorted to best fill the gap */
  // MenuItemDto[]: return type — an array of suggested items to help the user reach the minimum order
  get suggestedItems(): MenuItemDto[] {
    // if: early return — no suggestions needed when the minimum is already met
    if (!this.belowMinimum) return [];
    const needed = this.amountNeeded;
    // Set: ES6 built-in collection — provides O(1) membership checks; used to exclude items already in the cart
    const cartIds = new Set(this.cart.map(c => c.item.id));
    const candidates = this.menu
      // .flatMap(): Array method — maps each element to an array and flattens one level deep
      .flatMap(cat => cat.items)
      // .filter(): Array method — returns only elements satisfying the predicate
      .filter(i => i.isAvailable && !cartIds.has(i.id));

    // Prefer items whose price is ≤ needed (fills gap without overshooting too much)
    // Sort: items that fit within the gap first (ascending price), then cheapest above gap
    const fits    = candidates.filter(i => i.price <= needed).sort((a, b) => b.price - a.price);
    const above   = candidates.filter(i => i.price > needed).sort((a, b) => a.price - b.price);
    // Spread operator (...): merges two arrays into one; .slice(0, 4) limits to 4 suggestions
    return [...fits, ...above].slice(0, 4);
  }

  // constructor: called once by Angular's DI system; each private parameter is automatically injected by type
  constructor(
    private route: ActivatedRoute, private router: Router,   // ActivatedRoute: current URL params; Router: programmatic navigation
    private svc: RestaurantService, private orderSvc: OrderService, // service injections for restaurant and order HTTP calls
    private auth: AuthService, private addressSvc: AddressService   // auth state and address persistence services
  ) {}

  // ngOnInit: Angular lifecycle hook — the recommended place to trigger initial data fetching after inputs are set
  ngOnInit() {
    // !: non-null assertion — tells TypeScript the value is definitely not null/undefined here
    const id = this.route.snapshot.paramMap.get('id')!;
    // .subscribe(): Observable consumer — registers next/error callbacks to handle the async HTTP response
    this.svc.getById(id).subscribe({ next: r => { this.restaurant = r; this.loading = false; }, error: () => this.loading = false });
    this.svc.getMenu(id).subscribe({ next: m => this.menu = m, error: () => {} });
    this.svc.getReviews(id).subscribe({ next: r => this.reviews = r, error: () => {} });

    this.savedAddresses = this.addressSvc.getAll();
    this.deliveryAddress = this.addressSvc.getDefault();
  }

  // void return type: side-effect method — sets the selected delivery address and hides the new-address form
  selectAddress(addr: SavedAddress) {
    this.deliveryAddress = addr.text;
    this.showNewAddress = false;
  }

  // void return type: toggles the new-address form visibility and resets the input field
  toggleNewAddress() {
    // !: logical NOT — flips the boolean flag
    this.showNewAddress = !this.showNewAddress;
    if (this.showNewAddress) this.newAddressText = '';
  }

  // void return type: persists the new address and selects it as the active delivery address
  saveNewAddress() {
    // const: block-scoped constant — holds the trimmed address text
    const text = this.newAddressText.trim();
    // if: guard — prevents saving an empty address
    if (!text) return;
    // Persist immediately so it appears in the saved list
    this.addressSvc.saveUsed(text);
    this.savedAddresses = this.addressSvc.getAll();
    this.deliveryAddress = text;
    this.showNewAddress = false;
    this.newAddressText = '';
  }

  /** Live-update deliveryAddress as the user types so Place Order unlocks immediately */
  // void return type: called on every keystroke via (ngModelChange) to keep deliveryAddress in sync
  onNewAddressType(value: string) {
    this.deliveryAddress = value.trim();
  }

  // void return type: confirms the typed address without persisting it to the saved list
  confirmNewAddress() {
    const text = this.newAddressText.trim();
    if (!text) return;
    this.deliveryAddress = text;
    this.showNewAddress = false;
    this.newAddressText = '';
  }

  // void return type: removes a saved address and resets the selected address if it was the removed one
  removeAddress(event: Event, id: string) {
    // .stopPropagation(): prevents the click event from bubbling up to the parent address chip (which would re-select it)
    event.stopPropagation();
    this.addressSvc.remove(id);
    this.savedAddresses = this.addressSvc.getAll();
    // .some(): Array method — returns true if at least one element satisfies the predicate
    const stillExists = this.savedAddresses.some(a => a.text === this.deliveryAddress);
    if (!stillExists) this.deliveryAddress = this.addressSvc.getDefault();
  }

  addToCart(item: MenuItemDto) {
    // if: guard — prevents adding unavailable items to the cart
    if (!item.isAvailable) return;
    // .find(): Array method — returns the first element matching the predicate, or undefined
    const existing = this.cart.find(c => c.item.id === item.id);
    // if/else: either increments the existing cart entry or pushes a new one
    if (existing) existing.qty++; else this.cart.push({ item, qty: 1 });
  }
  // void return type: these methods produce side effects (mutating cart state) and return no value
  inc(c: CartItem) { c.qty++; }
  dec(c: CartItem) { c.qty > 1 ? c.qty-- : this.cart.splice(this.cart.indexOf(c), 1); }
  // boolean return type: checks whether a given menu item is already in the cart
  inCart(item: MenuItemDto) { return this.cart.some(c => c.item.id === item.id); }
  // number return type: returns the quantity of a specific item in the cart (0 if not present)
  getQty(item: MenuItemDto) { return this.cart.find(c => c.item.id === item.id)?.qty ?? 0; }
  // get total: computed property — sums price × quantity for all cart items
  get total() { return this.cart.reduce((s, c) => s + c.item.price * c.qty, 0); }
  // get totalItems: computed property — sums all quantities across cart entries
  get totalItems() { return this.cart.reduce((s, c) => s + c.qty, 0); }

  // boolean: UPI modal visibility flag — true shows the modal overlay
  showUpiModal = false;
  // string: holds the UPI ID entered by the user in the payment modal
  upiId = '';
  // string: holds a validation error message for the UPI ID input
  upiError = '';
  // boolean: true while the simulated UPI verification delay is running
  upiVerifying = false;
  // boolean: true after the simulated payment succeeds, showing the success screen
  upiSuccess = false;
  // private: internal field — stores the order ID while the UPI modal is open, used for navigation after payment
  private pendingOrderId = '';

  placeOrder() {
    // .isLoggedIn(): checks authentication state — redirects to login if the user is not authenticated
    if (!this.auth.isLoggedIn()) { this.router.navigate(['/auth/login']); return; }
    // .trim(): removes leading/trailing whitespace — guards against blank address submissions
    if (!this.deliveryAddress.trim()) { this.orderError = 'Please enter a delivery address'; return; }
    if (this.belowMinimum) { this.orderError = `Minimum order is ₹${this.restaurant!.minimumOrderAmount}`; return; }
    this.placing = true; this.orderError = '';
    // !: non-null assertion — asserts restaurant is defined at this point (guarded by the template *ngIf)
    this.orderSvc.place({
      restaurantId: this.restaurant!.id,
      restaurantName: this.restaurant!.name,
      restaurantLogoUrl: this.restaurant!.logoUrl ?? '',
      deliveryAddress: this.deliveryAddress,
      paymentMethod: this.paymentMethod,
      // .map(): Array method — transforms each CartItem into the DTO shape expected by the API
      items: this.cart.map(c => ({ menuItemId: c.item.id, menuItemName: c.item.name, quantity: c.qty, unitPrice: c.item.price }))
    }).subscribe({
      // next: callback invoked when the Observable emits the newly created order
      next: (order) => {
        this.addressSvc.saveUsed(this.deliveryAddress);
        this.placing = false;
        // if/else: branches on payment method — UPI shows a modal; COD navigates directly to the order detail page
        if (this.paymentMethod === 0) {
          // UPI — show payment modal before navigating
          this.pendingOrderId = order.id;
          this.showUpiModal = true;
        } else {
          // COD — go straight to order detail
          // .navigate(): Router method — performs programmatic client-side navigation to the given URL segments
          this.router.navigate(['/orders', order.id]);
        }
      },
      error: (err) => { this.orderError = err.error?.message || err.error || 'Failed to place order'; this.placing = false; }
    });
  }

  confirmUpiPayment() {
    // if: validation guards — return early with an error message if the UPI ID is missing or malformed
    if (!this.upiId.trim()) { this.upiError = 'Please enter your UPI ID'; return; }
    if (!this.upiId.includes('@')) { this.upiError = 'Enter a valid UPI ID (e.g. name@upi)'; return; }
    this.upiVerifying = true;
    this.upiError = '';
    // setTimeout: browser API — schedules a callback after a delay (ms); simulates async UPI verification
    // Simulate UPI verification (1.5s), then show success screen (2s), then navigate
    setTimeout(() => {
      this.upiVerifying = false;
      this.upiSuccess = true;
      // Nested setTimeout: chains a second delay to show the success screen before navigating away
      setTimeout(() => {
        this.showUpiModal = false;
        this.upiSuccess = false;
        this.router.navigate(['/orders', this.pendingOrderId]);
      }, 2200);
    }, 1800);
  }

  skipUpiPayment() {
    this.showUpiModal = false;
    this.upiSuccess = false;
    // Navigate to the order detail page even if the user skips UPI payment
    this.router.navigate(['/orders', this.pendingOrderId]);
  }
}


