// 'import' — ES module keyword that brings exported members from another module into this file's scope
// 'Component' — Angular decorator factory; marks a class as a component and attaches metadata (selector, template, styles)
// 'OnInit' — Angular lifecycle-hook interface; requires implementing ngOnInit(), called once after the first change detection
// 'OnDestroy' — Angular lifecycle-hook interface; requires implementing ngOnDestroy(), called just before the component is removed from the DOM
import { Component, OnInit, OnDestroy } from '@angular/core';

// 'CommonModule' — re-exports Angular built-in directives like *ngIf, *ngFor, and pipes like | async, | number, | slice
import { CommonModule } from '@angular/common';

// 'RouterLink' — Angular directive that turns an anchor tag (or any element) into a declarative navigation link
import { RouterLink } from '@angular/router';

// Service imports — Angular uses constructor injection; importing the class token here lets the DI system resolve it
import { RestaurantService } from '../../../core/services/restaurant.service';
import { OrderService } from '../../../core/services/order.service';

// DTO (Data Transfer Object) interfaces — plain TypeScript types that mirror the shape of API response payloads
import { RestaurantDto } from '../../../core/models/restaurant.models';
import { OrderDto } from '../../../core/models/order.models';

/**
 * Restaurant owner dashboard component.
 * Lists all restaurants owned by the authenticated user, allows toggling
 * open/closed status, and displays incoming orders for the selected restaurant
 * with real-time polling and status update actions.
 */

// '@Component' — TypeScript decorator (a function called at class-definition time) that attaches Angular metadata
@Component({
  // 'selector' — the custom HTML tag name Angular uses to instantiate this component in templates
  selector: 'app-owner-dashboard',

  // 'standalone: true' — opts this component out of NgModules; it declares its own dependencies via 'imports'
  standalone: true,

  // 'imports' — array of standalone components, directives, and pipes this component's template depends on
  // CommonModule provides *ngIf, *ngFor, pipes; RouterLink enables [routerLink] bindings
  imports: [CommonModule, RouterLink],

  // 'template' — inline HTML template string; backtick (`) uses a JS template literal for multi-line strings
  template: `
    <!-- 'class' — standard HTML attribute; Angular also supports [class] binding for dynamic CSS classes -->
    <div class="page">
      <div class="page-header">
        <h2>My Restaurants</h2>

        <!-- 'routerLink' — Angular directive; navigates to the given route path without a full page reload -->
        <a routerLink="/owner/create-restaurant" class="btn-add">+ Add Restaurant</a>
      </div>

      <!-- '*ngIf' — structural directive; adds/removes the host element from the DOM based on the expression -->
      <div *ngIf="loading" class="loading-state">
        <div class="icon">⏳</div><p>Loading your restaurants...</p>
      </div>

      <!-- Restaurant cards -->
      <!-- '&&' — logical AND; both conditions must be truthy for the element to render -->
      <div class="restaurants-grid" *ngIf="!loading && restaurants.length > 0">

        <!-- '*ngFor' — structural directive that stamps out one DOM node per item in an iterable -->
        <!-- 'let r of restaurants' — loop variable 'r' holds the current RestaurantDto on each iteration -->
        <div *ngFor="let r of restaurants" class="restaurant-card"

          <!-- '[class.selected]' — property binding; adds the CSS class 'selected' when the expression is truthy -->
          [class.selected]="selected?.id === r.id"

          <!-- '(click)' — event binding; listens for the native DOM click event and calls the handler -->
          (click)="select(r)">

          <!-- '*ngIf' on logo — conditionally renders the real image or a placeholder emoji -->
          <div class="card-logo" *ngIf="r.logoUrl"><img [src]="r.logoUrl" [alt]="r.name" /></div>
          <div class="card-logo placeholder" *ngIf="!r.logoUrl">🍽️</div>

          <div class="card-body">
            <!-- '{{ }}' — Angular interpolation; evaluates the expression and inserts the result as text -->
            <div class="card-name">{{ r.name }}</div>
            <div class="card-cuisine">{{ r.cuisineTypes }}</div>
            <div class="card-meta">

              <!-- '[class]' — dynamic class binding; sets the element's class to the evaluated string value -->
              <!-- '.toLowerCase()' — JS String method; converts to lowercase to match CSS class names -->
              <span class="status-badge" [class]="r.status.toLowerCase()">{{ r.status }}</span>

              <!-- '[class.open]' / '[class.closed]' — conditionally apply a single CSS class -->
              <span class="open-badge" *ngIf="r.status === 'Active'" [class.open]="r.isOpen" [class.closed]="!r.isOpen">
                <!-- Ternary operator '? :' — inline conditional expression; returns one of two values -->
                {{ r.isOpen ? '● Open' : '● Closed' }}
              </span>
            </div>
          </div>

          <!-- '$event.stopPropagation()' — prevents the click from bubbling up to the parent card's (click) handler -->
          <div class="card-actions" (click)="$event.stopPropagation()">

            <!-- '[routerLink]' — property binding form of routerLink; accepts an array for parameterised routes -->
            <a [routerLink]="['/owner/menu', r.id]" class="btn-menu">🍽️ Menu</a>
            <a [routerLink]="['/owner/edit-restaurant', r.id]" class="btn-edit">✏️ Edit</a>

            <!-- '*ngIf' with '===' — strict equality check; only shows the toggle button for Active restaurants -->
            <button *ngIf="r.status === 'Active'" class="btn-toggle" (click)="toggleOpen(r)">
              {{ r.isOpen ? 'Close' : 'Open' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Empty state — shown when loading is done but the array is empty -->
      <div *ngIf="!loading && restaurants.length === 0" class="no-restaurant">
        <div class="icon">🏪</div>
        <p>You haven't registered any restaurants yet.</p>
        <a routerLink="/owner/create-restaurant" class="btn-create">Register Your First Restaurant</a>
      </div>

      <!-- '*ngIf="selected"' — truthy check; renders the detail panel only when a restaurant is selected -->
      <div *ngIf="selected" class="selected-detail">
        <div class="detail-header">
          <h3>{{ selected.name }}</h3>
          <div class="detail-meta">
            <span>📍 {{ selected.address }}</span>

            <!-- '| number' — Angular built-in pipe; formats a numeric value; '1.1-1' means min 1 integer digit, 1–1 decimal -->
            <span>⭐ {{ selected.averageRating | number:'1.1-1' }} ({{ selected.totalReviews }} reviews)</span>
            <span>🕐 {{ selected.estimatedDeliveryMinutes }} min</span>
          </div>
        </div>

        <!-- Status-specific notice banners -->
        <div *ngIf="selected.status === 'Pending'" class="notice pending-notice">
          ⏳ Under review — an admin will approve it shortly.
        </div>
        <div *ngIf="selected.status === 'Rejected'" class="notice rejected-notice">
          Rejected. Please contact support or register a new restaurant.
        </div>

        <!-- Orders section — only visible when the restaurant is Active -->
        <div class="orders-section" *ngIf="selected.status === 'Active'">
          <div class="orders-header">
            <div class="orders-title-wrap">
              <span class="orders-icon">📋</span>
              <h4>Incoming Orders</h4>

              <!-- '.length > 0' — Array property; shows the badge only when there is at least one active order -->
              <span class="orders-count" *ngIf="activeOrders.length > 0">{{ activeOrders.length }}</span>
            </div>
            <button class="btn-refresh-orders" (click)="refreshOrders()">↻ Refresh</button>
          </div>

          <div *ngIf="ordersLoading" class="orders-loading">
            <span>⏳</span> Loading orders...
          </div>
          <div *ngIf="!ordersLoading && activeOrders.length === 0" class="empty">
            <span class="empty-icon">🎉</span>
            <span>No active orders right now.</span>
          </div>

          <!-- Iterates over the filtered activeOrders getter result -->
          <div *ngFor="let o of activeOrders" class="order-row">
            <div class="order-row-main">
              <div class="order-row-top">

                <!-- '| slice:0:8' — Angular slice pipe; extracts characters 0–7 of the UUID for a short display ID -->
                <!-- '| uppercase' — Angular pipe; converts the sliced string to uppercase -->
                <span class="order-id">#{{ o.id | slice:0:8 | uppercase }}</span>

                <!-- 'getStatus(o)' — method call in template; resolves numeric enum to a human-readable string -->
                <span class="status-badge" [class]="getStatus(o).toLowerCase()">{{ getStatus(o) }}</span>
              </div>
              <div class="order-row-meta">
                <!-- '.length' — Array property; counts the number of line items in the order -->
                <span>{{ o.items.length }} item(s)</span>
                <span class="meta-dot">·</span>
                <span>📍 {{ o.deliveryAddress }}</span>
                <span class="meta-dot">·</span>

                <!-- '| number:'1.0-0'' — formats the amount with no decimal places (Indian Rupee display) -->
                <span class="amount">₹{{ o.totalAmount | number:'1.0-0' }}</span>
              </div>
            </div>

            <div class="order-actions">
              <!-- Each button uses *ngIf to show only the action relevant to the current order status -->
              <!-- 'updateStatus(o, 1)' — passes numeric status code matching the server-side OrderStatus enum -->
              <button *ngIf="getStatus(o) === 'Placed'" (click)="updateStatus(o, 1)" class="btn-sm green">✓ Confirm</button>
              <button *ngIf="getStatus(o) === 'Confirmed'" (click)="updateStatus(o, 2)" class="btn-sm orange">Preparing</button>
              <button *ngIf="getStatus(o) === 'Preparing'" (click)="updateStatus(o, 3)" class="btn-sm blue">Ready</button>

              <!-- '.includes()' — Array method; returns true if the value exists in the array (multi-status check) -->
              <button *ngIf="['Placed','Confirmed'].includes(getStatus(o))" (click)="updateStatus(o, 6)" class="btn-sm red">✕</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  // 'styleUrl' — points to an external SCSS file; Angular compiles and scopes it to this component
  styleUrl: './owner-dashboard.scss'
})

// 'export' — makes the class available for import in other modules (e.g., the router config)
// 'class' — ES6 class declaration; encapsulates component state and behaviour
// 'implements OnInit, OnDestroy' — TypeScript interface enforcement; compiler errors if lifecycle methods are missing
export class OwnerDashboardComponent implements OnInit, OnDestroy {

  // 'restaurants: RestaurantDto[]' — typed array property; holds the list fetched from the API
  restaurants: RestaurantDto[] = [];

  // '?' — TypeScript optional property; 'selected' may be undefined when no restaurant is chosen
  selected?: RestaurantDto;

  // 'orders: OrderDto[]' — holds raw orders for the selected restaurant before filtering
  orders: OrderDto[] = [];

  // 'loading' / 'ordersLoading' — boolean flags that drive *ngIf loading-state UI
  loading = true;
  ordersLoading = false;

  // 'private' — TypeScript access modifier; hides the property from outside the class
  // 'pollInterval?' — optional; stores the interval ID so it can be cleared on destroy
  // 'ReturnType<typeof setInterval>' — utility type that infers the return type of setInterval (NodeJS.Timeout or number)
  private pollInterval?: ReturnType<typeof setInterval>;

  // 'private readonly' — the map is internal and must not be reassigned after initialisation
  // 'Record<number, string>' — TypeScript utility type; an object whose keys are numbers and values are strings
  // Maps server-side OrderStatus enum integers to display strings
  private readonly statusMap: Record<number, string> = {
    0: 'Placed', 1: 'Confirmed', 2: 'Preparing', 3: 'Ready', 4: 'PickedUp', 5: 'Delivered', 6: 'Cancelled', 7: 'Rejected'
  };

  /**
   * Filters orders to only those that still require action,
   * hiding terminal statuses (Delivered, Cancelled, PickedUp).
   */
  // 'get' — ES6 getter; exposes a computed value as a property so templates can use 'activeOrders' without calling a method
  get activeOrders(): OrderDto[] {
    // 'const' — block-scoped, immutable binding; the variable cannot be reassigned
    const terminal = ['Delivered', 'Cancelled', 'PickedUp'];

    // '.filter()' — Array higher-order method; returns a new array containing only elements that pass the predicate
    // '!' — logical NOT; inverts the boolean returned by .includes()
    return this.orders.filter(o => !terminal.includes(this.getStatus(o)));
  }

  // 'constructor' — special method called by Angular's DI system when creating the component instance
  // Angular injects service singletons by matching the parameter type tokens
  constructor(
    private restaurantSvc: RestaurantService,
    private orderSvc: OrderService
  ) {}

  // 'ngOnInit' — Angular lifecycle hook; runs after the component's inputs are set, safe to call services here
  ngOnInit() {
    // '.getMyRestaurant()' — returns an RxJS Observable; '.subscribe()' triggers the HTTP request
    this.restaurantSvc.getMyRestaurant().subscribe({
      // 'next' — callback invoked for each emitted value (the successful API response)
      next: list => {
        this.restaurants = list;
        this.loading = false;

        // Auto-select the only restaurant to improve UX when the owner has exactly one
        if (list.length === 1) this.select(list[0]);
      },
      // 'error' — callback invoked if the Observable errors (e.g., HTTP 4xx/5xx)
      error: (err) => {
        console.error('Failed to load restaurants:', err.status, err.message);
        this.loading = false;
      }
    });
  }

  // 'ngOnDestroy' — Angular lifecycle hook; called before the component is removed from the DOM
  // Used here to prevent memory leaks by clearing the polling interval
  ngOnDestroy() {
    this.stopPolling();
  }

  // 'select' — called when the user clicks a restaurant card; resets orders and starts polling
  select(r: RestaurantDto) {
    this.stopPolling();   // clear any existing interval before starting a new one
    this.selected = r;
    this.orders = [];     // reset so stale orders from the previous selection are not shown

    if (r.status === 'Active') {
      this.loadOrders(r.id);

      // 'setInterval' — Web API / Node API; repeatedly calls the callback after the given delay (ms)
      // '15_000' — numeric separator syntax (ES2021); equals 15000 ms = 15 seconds
      this.pollInterval = setInterval(() => this.loadOrders(r.id), 15_000);
    }
  }

  // 'refreshOrders' — manual refresh triggered by the Refresh button in the template
  refreshOrders() {
    // Optional chaining '?.' — safely accesses 'id' only if 'selected' is not null/undefined
    if (this.selected) this.loadOrders(this.selected.id);
  }

  // 'private' — internal helper; not part of the public API of this component
  private loadOrders(restaurantId: string) {
    this.ordersLoading = true;

    // '.getByRestaurant()' — service method returning an Observable of OrderDto[]
    this.orderSvc.getByRestaurant(restaurantId).subscribe({
      // Arrow function '=>' — concise function syntax; 'o' is the emitted array of orders
      next: o => { this.orders = o; this.ordersLoading = false; },
      error: () => { this.ordersLoading = false; }
    });
  }

  // 'stopPolling' — clears the interval to prevent callbacks firing after the component is destroyed
  private stopPolling() {
    if (this.pollInterval) {
      // 'clearInterval' — Web/Node API; cancels a repeating timer created with setInterval
      clearInterval(this.pollInterval);

      // Reset to undefined so the guard in stopPolling works correctly on subsequent calls
      this.pollInterval = undefined;
    }
  }

  // 'toggleOpen' — flips the restaurant's open/closed availability via the API
  toggleOpen(r: RestaurantDto) {
    this.restaurantSvc.toggleAvailability(r.id).subscribe(res => {
      // Mutate the card's isOpen flag so the UI updates immediately (optimistic-style)
      r.isOpen = res.isOpen;

      // Spread operator '...' — creates a shallow copy of 'selected' with the updated isOpen value
      // This triggers Angular change detection because a new object reference is assigned
      if (this.selected?.id === r.id) this.selected = { ...this.selected, isOpen: res.isOpen };
    });
  }

  // 'getStatus' — resolves an order's status to a string regardless of whether the API returns a number or string
  getStatus(order: OrderDto): string {
    // 'as unknown' — TypeScript escape hatch; widens the type to 'unknown' before narrowing with typeof
    const s = order.status as unknown;

    // 'typeof s === 'number'' — runtime type guard; checks if the value is a numeric enum
    // '??' — nullish coalescing operator; falls back to String(s) if the key is not in statusMap
    return typeof s === 'number' ? (this.statusMap[s] ?? String(s)) : String(s);
  }

  // 'updateStatus' — sends a PATCH/PUT request to advance the order through its workflow
  updateStatus(order: OrderDto, status: number) {
    this.orderSvc.updateStatus(order.id, status).subscribe(() => {
      // Lookup array — index matches the numeric OrderStatus enum value on the server
      const statuses = ['Placed', 'Confirmed', 'Preparing', 'Ready', 'PickedUp', 'Delivered', 'Cancelled', 'Rejected'];

      // 'as any' — TypeScript cast; suppresses the type error when assigning a string to the typed status field
      // Optimistically updates the local order object so the UI reflects the new status without a re-fetch
      order.status = statuses[status] as any;
    });
  }
}
