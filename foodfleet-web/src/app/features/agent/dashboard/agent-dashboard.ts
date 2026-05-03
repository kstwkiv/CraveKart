// 'import' — ES module keyword; pulls named exports from another module into this file's scope
// 'Component'  — Angular decorator factory; marks a class as a component and attaches template/style metadata
// 'OnInit'     — Angular lifecycle-hook interface; requires ngOnInit(); called after first ngOnChanges
// 'OnDestroy'  — Angular lifecycle-hook interface; requires ngOnDestroy(); called just before the component is destroyed
import { Component, OnInit, OnDestroy } from '@angular/core';
// 'CommonModule' — provides *ngIf, *ngFor, async pipe, and other structural directives
import { CommonModule } from '@angular/common';
// 'FormsModule' — Angular module that enables template-driven forms and [(ngModel)] two-way binding
import { FormsModule } from '@angular/forms';
// 'DeliveryService' — application service that wraps HTTP calls to the Delivery API
import { DeliveryService } from '../../../core/services/delivery.service';
// 'OrderService' — application service that wraps HTTP calls to the Order API
import { OrderService } from '../../../core/services/order.service';
// 'DeliveryAgentDto' — TypeScript interface (pure type contract) for a delivery agent data transfer object
import { DeliveryAgentDto } from '../../../core/models/delivery.models';
// 'OrderDto' — TypeScript interface (pure type contract) for an order data transfer object
import { OrderDto } from '../../../core/models/order.models';

// 'interface' — TypeScript keyword; defines a pure structural contract with no runtime code
// Used here as a local (non-exported) type for the active delivery shape
interface ActiveDelivery {
  id: string;           // 'string' — TypeScript primitive type: a sequence of UTF-16 characters
  orderId: string;      // 'string' — foreign-key reference stored as text
  status: string;       // 'string' — current delivery status (e.g. "Assigned", "Delivered")
  currentLat?: number;  // '?' — optional property; may be undefined if location has not been shared yet
  currentLng?: number;  // 'number' — TypeScript primitive type: covers both integers and floats
  assignedAt: string;   // 'string' — ISO 8601 timestamp stored as text
}

/**
 * Delivery agent dashboard component.
 * Handles agent registration, availability toggling, vehicle management,
 * real-time location sharing, ready-order queue polling, order pickup,
 * and delivery completion with earnings tracking.
 */
// '@Component' — class decorator; Angular reads this metadata to compile the template and wire up DI
@Component({
  selector: 'app-agent-dashboard',  // CSS selector used in templates/router to render this component
  standalone: true,                 // standalone: true — no NgModule needed; the component manages its own imports
  imports: [CommonModule, FormsModule], // 'imports' — Angular dependencies for this component's template
  template: `
    <div class="page">

      <!-- ── Register ── -->
      <div *ngIf="!profile && !loading" class="register-card">
        <div class="icon">🚴</div>
        <h3>Become a Delivery Agent</h3>
        <p>Register your vehicle and start earning with FoodFleet.</p>
        <div class="field">
          <label>Vehicle Type</label>
          <select [(ngModel)]="vehicleType">
            <option value="Bike">🏍️ Bike</option>
            <option value="Scooter">🛵 Scooter</option>
            <option value="Car">🚗 Car</option>
            <option value="Bicycle">🚲 Bicycle</option>
          </select>
        </div>
        <button class="btn-primary" (click)="register()">Register Now →</button>
      </div>

      <!-- ── Main Dashboard ── -->
      <div *ngIf="profile" class="dashboard">

        <!-- ── Header ── -->
        <div class="dash-header">
          <div class="agent-identity">
            <div class="agent-avatar">{{ profile.fullName.charAt(0) }}</div>
            <div>
              <div class="agent-name">{{ profile.fullName }}</div>
              <div class="agent-meta">
                {{ vehicleEmoji(profile.vehicleType) }} {{ profile.vehicleType }}
                <span class="sep">·</span>
                Agent ID: {{ profile.id | slice:0:8 | uppercase }}
              </div>
            </div>
          </div>
          <div class="header-actions">
            <button class="availability-btn"
              [class.available]="profile.isAvailable"
              [class.unavailable]="!profile.isAvailable"
              (click)="toggleAvailability()">
              <span class="avail-dot"></span>
              {{ profile.isAvailable ? 'Available' : 'Unavailable' }}
            </button>
          </div>
        </div>

        <!-- ── Earnings & Stats ── -->
        <div class="stats-grid">
          <div class="stat-card earnings" [class.active]="activePanel === 'earnings'" (click)="togglePanel('earnings')">
            <div class="stat-icon">💰</div>
            <div class="stat-body">
              <div class="stat-val">₹{{ displayEarnings | number:'1.0-0' }}</div>
              <div class="stat-lbl">Total Earnings</div>
            </div>
            <div class="stat-trend">+₹25 per delivery</div>
            <span class="stat-chevron">{{ activePanel === 'earnings' ? '▲' : '▼' }}</span>
          </div>
          <div class="stat-card deliveries" [class.active]="activePanel === 'deliveries'" (click)="togglePanel('deliveries')">
            <div class="stat-icon">📦</div>
            <div class="stat-body">
              <div class="stat-val">{{ profile.totalDeliveries }}</div>
              <div class="stat-lbl">Deliveries Done</div>
            </div>
            <div class="stat-trend" *ngIf="profile.totalDeliveries > 0">
              Avg ₹{{ avgEarnings | number:'1.0-0' }} / delivery
            </div>
            <span class="stat-chevron">{{ activePanel === 'deliveries' ? '▲' : '▼' }}</span>
          </div>
          <div class="stat-card vehicle" [class.active]="activePanel === 'vehicle'" (click)="togglePanel('vehicle')">
            <div class="stat-icon">{{ vehicleEmoji(profile.vehicleType) }}</div>
            <div class="stat-body">
              <div class="stat-val">{{ profile.vehicleType }}</div>
              <div class="stat-lbl">Vehicle</div>
            </div>
            <span class="stat-chevron">{{ activePanel === 'vehicle' ? '▲' : '▼' }}</span>
          </div>
          <div class="stat-card status" [class.active]="activePanel === 'status'" (click)="togglePanel('status')">
            <div class="stat-icon">{{ profile.isAvailable ? '🟢' : '🔴' }}</div>
            <div class="stat-body">
              <div class="stat-val">{{ profile.isAvailable ? 'Online' : 'Offline' }}</div>
              <div class="stat-lbl">Status</div>
            </div>
            <div class="stat-trend">Since {{ profile.createdAt | date:'MMM y' }}</div>
            <span class="stat-chevron">{{ activePanel === 'status' ? '▲' : '▼' }}</span>
          </div>
        </div>

        <!-- ── Stat detail panels ── -->
        <div class="stat-panel" *ngIf="activePanel === 'earnings'">
          <div class="panel-header">
            <span class="panel-icon">💰</span>
            <h4>Earnings Breakdown</h4>
            <button class="panel-close" (click)="activePanel = null">✕</button>
          </div>
          <div class="panel-body">
            <div class="panel-row highlight">
              <span>Total Earned</span>
              <strong class="green">₹{{ displayEarnings | number:'1.0-0' }}</strong>
            </div>
            <div class="panel-row">
              <span>Deliveries Completed</span>
              <strong>{{ profile.totalDeliveries }}</strong>
            </div>
            <div class="panel-row">
              <span>Earnings per Delivery</span>
              <strong>₹25.00</strong>
            </div>
            <div class="panel-row">
              <span>Average per Delivery</span>
              <strong>₹{{ avgEarnings | number:'1.0-0' }}</strong>
            </div>
            <div class="panel-row muted">
              <span>Next delivery will earn</span>
              <strong class="green">+₹25</strong>
            </div>
            <div class="panel-note">
              💡 Earnings are calculated at ₹25 per completed delivery. Payouts are processed weekly.
            </div>
          </div>
        </div>

        <div class="stat-panel" *ngIf="activePanel === 'deliveries'">
          <div class="panel-header">
            <span class="panel-icon">📦</span>
            <h4>Delivery Stats</h4>
            <button class="panel-close" (click)="activePanel = null">✕</button>
          </div>
          <div class="panel-body">
            <div class="panel-row highlight">
              <span>Total Deliveries</span>
              <strong class="green">{{ profile.totalDeliveries }}</strong>
            </div>
            <div class="panel-row">
              <span>Total Earned</span>
              <strong>₹{{ displayEarnings | number:'1.0-0' }}</strong>
            </div>
            <div class="panel-row">
              <span>Avg Earnings / Delivery</span>
              <strong>₹{{ avgEarnings | number:'1.0-0' }}</strong>
            </div>
            <div class="panel-row">
              <span>Member Since</span>
              <strong>{{ profile.createdAt | date:'mediumDate' }}</strong>
            </div>
            <div class="panel-row muted">
              <span>Current Status</span>
              <strong>{{ profile.isAvailable ? '🟢 Online' : '🔴 Offline' }}</strong>
            </div>
            <div class="panel-note" *ngIf="profile.totalDeliveries === 0">
              🚀 Complete your first delivery to start building your stats!
            </div>
          </div>
        </div>

        <div class="stat-panel" *ngIf="activePanel === 'vehicle'">
          <div class="panel-header">
            <span class="panel-icon">{{ vehicleEmoji(profile.vehicleType) }}</span>
            <h4>Vehicle Details</h4>
            <button class="panel-close" (click)="activePanel = null">✕</button>
          </div>
          <div class="panel-body">
            <div class="panel-row highlight">
              <span>Current Vehicle</span>
              <strong>{{ vehicleEmoji(profile.vehicleType) }} {{ profile.vehicleType }}</strong>
            </div>
            <div class="panel-row">
              <span>Agent ID</span>
              <strong>{{ profile.id | slice:0:8 | uppercase }}</strong>
            </div>
            <div class="panel-row">
              <span>Registered Since</span>
              <strong>{{ profile.createdAt | date:'mediumDate' }}</strong>
            </div>
            <div class="panel-actions">
              <button class="btn-panel-action" (click)="openVehicleEdit(); activePanel = null">
                Change Vehicle Type
              </button>
            </div>
          </div>
        </div>

        <div class="stat-panel" *ngIf="activePanel === 'status'">
          <div class="panel-header">
            <span class="panel-icon">{{ profile.isAvailable ? '🟢' : '🔴' }}</span>
            <h4>Availability Status</h4>
            <button class="panel-close" (click)="activePanel = null">✕</button>
          </div>
          <div class="panel-body">
            <div class="panel-row highlight">
              <span>Current Status</span>
              <strong [class.green]="profile.isAvailable" [class.red]="!profile.isAvailable">
                {{ profile.isAvailable ? '🟢 Online — Accepting orders' : '🔴 Offline — Not accepting orders' }}
              </strong>
            </div>
            <div class="panel-row">
              <span>Member Since</span>
              <strong>{{ profile.createdAt | date:'mediumDate' }}</strong>
            </div>
            <div class="panel-row">
              <span>Total Deliveries</span>
              <strong>{{ profile.totalDeliveries }}</strong>
            </div>
            <div class="panel-row">
              <span>Current Location</span>
              <strong *ngIf="profile.currentLat">{{ profile.currentLat | number:'1.4-4' }}, {{ profile.currentLng | number:'1.4-4' }}</strong>
              <strong *ngIf="!profile.currentLat" class="muted-val">Not set</strong>
            </div>
            <div class="panel-actions">
              <button class="btn-panel-action" (click)="toggleAvailability(); activePanel = null">
                {{ profile.isAvailable ? '🔴 Go Offline' : '🟢 Go Online' }}
              </button>
            </div>
          </div>
        </div>

        <!-- ── Vehicle Edit Modal ── -->
        <div class="modal-backdrop" *ngIf="showVehicleEdit" (click)="closeVehicleEdit()">
          <div class="modal" (click)="$event.stopPropagation()">
            <div class="modal-header">
              <h3>Update Vehicle</h3>
              <button class="modal-close" (click)="closeVehicleEdit()">✕</button>
            </div>
            <div class="modal-body">
              <p class="modal-hint">Choose the vehicle you're currently using for deliveries.</p>
              <div class="vehicle-options">
                <label *ngFor="let v of vehicleOptions" class="vehicle-option"
                  [class.selected]="editVehicleType === v.value">
                  <input type="radio" [value]="v.value" [(ngModel)]="editVehicleType" />
                  <span class="v-emoji">{{ v.emoji }}</span>
                  <span class="v-label">{{ v.label }}</span>
                </label>
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn-cancel-modal" (click)="closeVehicleEdit()">Cancel</button>
              <button class="btn-save-modal" (click)="saveVehicle()" [disabled]="savingVehicle">
                {{ savingVehicle ? 'Saving...' : 'Save Changes' }}
              </button>
            </div>
          </div>
        </div>

        <!-- ── Two-column body ── -->
        <div class="body-grid">

          <!-- Left: active delivery / orders / offline -->
          <div class="main-col">

            <!-- ── Active Delivery ── -->
            <div *ngIf="activeDelivery" class="active-delivery-card">
              <div class="active-header">
                <div class="active-title">
                  <span class="pulse-dot"></span>
                  <h3>Active Delivery</h3>
                </div>
                <span class="active-status-badge">{{ activeDelivery.status }}</span>
              </div>
              <div class="active-details">
                <div class="active-detail-row">
                  <span class="detail-icon">🧾</span>
                  <span class="detail-label">Order</span>
                  <span class="detail-val">#{{ activeDelivery.orderId | slice:0:8 | uppercase }}</span>
                </div>
                <div class="active-detail-row">
                  <span class="detail-icon">🕐</span>
                  <span class="detail-label">Assigned at</span>
                  <span class="detail-val">{{ activeDelivery.assignedAt | date:'shortTime' }}</span>
                </div>
                <div class="active-detail-row" *ngIf="activeDelivery.currentLat">
                  <span class="detail-icon">📍</span>
                  <span class="detail-label">Last location</span>
                  <span class="detail-val">{{ activeDelivery.currentLat | number:'1.4-4' }}, {{ activeDelivery.currentLng | number:'1.4-4' }}</span>
                </div>
              </div>
              <div class="active-actions">
                <button class="btn-share-loc" (click)="shareLocation()" [disabled]="sharingLocation">
                  <span>📍</span>
                  {{ sharingLocation ? 'Sharing...' : 'Share Location' }}
                </button>
                <button class="btn-delivered" (click)="completeDelivery()" [disabled]="completing">
                  <span>✅</span>
                  {{ completing ? 'Marking...' : 'Mark as Delivered' }}
                </button>
              </div>
              <div class="gps-status" *ngIf="gpsStatus">{{ gpsStatus }}</div>
              <div class="earnings-preview">
                <span>🎉 You'll earn</span>
                <strong>₹100</strong>
                <span>for this delivery</span>
              </div>
            </div>

            <!-- ── Ready Orders Queue ── -->
            <div *ngIf="!activeDelivery && profile.isAvailable" class="orders-section">
              <div class="section-header">
                <div class="section-title-wrap">
                  <span class="section-icon"></span>
                  <h3>Ready for Pickup</h3>
                </div>
                <button class="btn-refresh" (click)="loadReadyOrders()" title="Refresh">Refresh</button>
              </div>

              <div *ngIf="readyOrdersLoading" class="loading-hint">
                <span class="spin">⏳</span> Checking for orders...
              </div>

              <div *ngIf="!readyOrdersLoading && readyOrders.length === 0" class="empty-queue">
                <div class="empty-icon">⏳</div>
                <p>No orders ready for pickup right now.</p>
                <span class="empty-sub">We'll auto-refresh every 10 seconds</span>
              </div>

              <div *ngFor="let o of readyOrders" class="order-row">
                <div class="order-row-left">
                  <div class="order-restaurant">{{ o.restaurantName }}</div>
                  <div class="order-meta-row">
                    <span class="order-id">#{{ o.id | slice:0:8 | uppercase }}</span>
                    <span class="meta-dot">·</span>
                    <span>{{ o.items.length }} item(s)</span>
                    <span class="meta-dot">·</span>
                    <span class="order-addr">📍 {{ o.deliveryAddress }}</span>
                  </div>
                </div>
                <div class="order-row-right">
                  <div class="order-earn">+₹100</div>
                  <div class="order-amount">₹{{ o.totalAmount | number:'1.0-0' }}</div>
                  <button class="btn-pickup" (click)="pickupOrder(o)" [disabled]="pickingUp === o.id">
                    {{ pickingUp === o.id ? 'Picking up...' : '🛵 Pick Up' }}
                  </button>
                </div>
              </div>
            </div>

            <!-- ── Offline notice ── -->
            <div *ngIf="!activeDelivery && !profile.isAvailable" class="offline-card">
              <span class="offline-icon">😴</span>
              <div>
                <strong>You're offline</strong>
                <p>Toggle availability to start receiving orders.</p>
              </div>
              <button class="btn-go-online" (click)="toggleAvailability()">Go Online</button>
            </div>

          </div><!-- /main-col -->

          <!-- Right: location panel (always visible) -->
          <div class="side-col">
            <div class="location-card">
              <div class="loc-card-header">
                <span class="section-icon">📍</span>
                <h3>My Location</h3>
              </div>
              <div class="loc-card-body">
                <p class="loc-hint">Keep your location updated so customers can track you.</p>
                <div class="loc-inputs">
                  <div class="loc-field">
                    <label>Latitude</label>
                    <input type="number" [(ngModel)]="lat" placeholder="e.g. 12.9716" />
                  </div>
                  <div class="loc-field">
                    <label>Longitude</label>
                    <input type="number" [(ngModel)]="lng" placeholder="e.g. 77.5946" />
                  </div>
                </div>
                <button class="btn-update-loc" (click)="updateLocation()" [disabled]="updatingLocation">
                  {{ updatingLocation ? "Updating..." : "Update Location" }}
                </button>
                <button class="btn-gps-auto" (click)="useGPS()">Use GPS Automatically</button>
                <div class="loc-success" *ngIf="locationUpdated">Location updated!</div>
                <div class="loc-current" *ngIf="profile.currentLat">
                  <span class="loc-current-label">Last known:</span>
                  <span class="loc-current-val">{{ profile.currentLat | number:'1.4-4' }}, {{ profile.currentLng | number:'1.4-4' }}</span>
                </div>
              </div>
            </div>
          </div><!-- /side-col -->

        </div><!-- /body-grid -->

      </div>
    </div>
  `,
  styleUrl: './agent-dashboard.scss'
})
// 'export' — makes this class importable by the Angular router and other modules
// 'class'  — ES6/TypeScript keyword; defines a reusable blueprint with properties and methods
// 'implements' — TypeScript keyword; enforces that the class satisfies the listed interface contracts
// 'OnInit', 'OnDestroy' — Angular lifecycle interfaces being implemented here
export class AgentDashboardComponent implements OnInit, OnDestroy {
  // '?' — optional property; may be undefined until the HTTP profile response arrives
  // 'DeliveryAgentDto' — TypeScript interface used as the type annotation
  profile?: DeliveryAgentDto;
  activeDelivery?: ActiveDelivery; // '?' — optional; undefined when no delivery is currently assigned
  // 'OrderDto[]' — Array type annotation; an ordered collection of OrderDto objects
  readyOrders: OrderDto[] = [];    // '= []' — initialised to empty array to avoid null-reference errors
  // 'boolean' (inferred) — primitive type; true/false flag to show/hide the loading spinner
  readyOrdersLoading = false;
  loading = true;
  completing = false;
  // 'string | null' — union type; either a string order ID or null when no pickup is in progress
  pickingUp: string | null = null; // 'null' — JS/TS primitive; explicit absence of a value
  // 'string' (inferred) — default vehicle type for the registration form
  vehicleType = 'Bike';
  // 'number' (inferred) — latitude and longitude for manual location update
  lat = 0;
  lng = 0;
  sharingLocation = false;
  gpsStatus = '';
  updatingLocation = false;
  locationUpdated = false;

  // Vehicle edit
  showVehicleEdit = false;
  editVehicleType = 'Bike';
  savingVehicle = false;

  // 'private' — access modifier; only accessible within this class
  // 'readonly' — TypeScript modifier; the property cannot be reassigned after initialisation
  private readonly EARN_PER_DELIVERY = 100; // 'number' (inferred) — constant earnings per delivery

  // Stat panels — union type with null; tracks which expandable panel is open
  // 'null' — JS/TS primitive; used here to represent "no panel open"
  activePanel: 'earnings' | 'deliveries' | 'vehicle' | 'status' | null = null;

  togglePanel(panel: 'earnings' | 'deliveries' | 'vehicle' | 'status') {
    // Ternary operator — if the same panel is clicked again, close it (null); otherwise open the new one
    this.activePanel = this.activePanel === panel ? null : panel;
  }

  // 'readonly' — this array is initialised once and never reassigned; prevents accidental mutation of the reference
  readonly vehicleOptions = [
    { value: 'Bike',    label: 'Bike',    emoji: '🏍️' },
    { value: 'Scooter', label: 'Scooter', emoji: '🛵' },
    { value: 'Car',     label: 'Car',     emoji: '🚗' },
    { value: 'Bicycle', label: 'Bicycle', emoji: '🚲' },
  ];

  // 'private' — only accessible within this class; used to store the polling interval handle
  // 'ReturnType<typeof setInterval>' — TypeScript utility type that infers the return type of setInterval
  private pollInterval?: ReturnType<typeof setInterval>; // '?' — optional; undefined until polling starts

  // 'constructor' — called by Angular's DI system when instantiating this class
  // 'private' — access modifier; these injected services are only accessible within this class
  constructor(private svc: DeliveryService, private orderSvc: OrderService) {}

  // 'ngOnInit' — Angular lifecycle hook method; called once after the component's inputs are first set
  ngOnInit() {
    // 'subscribe' — RxJS method that activates an Observable and registers callbacks for next/error/complete
    this.svc.getMyProfile().subscribe({
      // 'next' — callback invoked when the Observable emits a value (successful HTTP response)
      next: p => {
        this.profile = p;
        this.loading = false;
        this.checkDelivery();
        this.startPolling();
      },
      // 'error' — callback invoked when the Observable errors (HTTP error, network failure, etc.)
      error: () => { this.loading = false; }
    });
  }

  // 'ngOnDestroy' — Angular lifecycle hook method; called just before Angular destroys the component
  // Used here to cancel the polling interval and prevent memory leaks
  ngOnDestroy() {
    // 'if' — guards against calling clearInterval when no interval was set
    if (this.pollInterval) clearInterval(this.pollInterval);
  }

  // 'get' — TypeScript getter; defines a computed property accessed like a field (no parentheses)
  get avgEarnings(): number { // 'number' — return type annotation
    // 'if' — short-circuit guard; returns 0 when there are no deliveries to avoid division by zero
    if (!this.profile?.totalDeliveries) return 0;
    return this.displayEarnings / this.profile.totalDeliveries;
  }

  /**
   * Calculates total earnings from the backend value if available,
   * otherwise falls back to `totalDeliveries × ₹100`.
   */
  // 'get' — TypeScript getter; computed property that reads profile data and returns a number
  get displayEarnings(): number { // 'number' — return type annotation
    // 'if' — guard; returns 0 when profile is not yet loaded
    if (!this.profile) return 0;
    return this.profile.totalEarnings > 0
      ? this.profile.totalEarnings
      : this.profile.totalDeliveries * this.EARN_PER_DELIVERY;
  }

  vehicleEmoji(type: string): string { // 'string' — parameter and return type annotations
    // '??' — nullish coalescing operator; falls back to '🚴' if no matching vehicle option is found
    return this.vehicleOptions.find(v => v.value === type)?.emoji ?? '🚴';
  }

  // 'private' — only accessible within this class; encapsulates the polling setup logic
  private startPolling() {
    // setInterval — schedules a callback to run repeatedly every 10 seconds
    this.pollInterval = setInterval(() => {
      // 'if' — conditional; only polls for orders when the agent is available and has no active delivery
      if (this.profile?.isAvailable && !this.activeDelivery) this.loadReadyOrders();
      else if (this.activeDelivery) this.checkDelivery();
    }, 10_000); // numeric separator '_' — improves readability of large numbers (10_000 = 10000)
  }

  register() {
    // 'subscribe' — activates the Observable returned by registerAgent(); triggers the HTTP POST call
    this.svc.registerAgent(this.vehicleType).subscribe({
      next: p => { this.profile = p; this.loadReadyOrders(); this.startPolling(); },
      error: (err) => {
        // Ternary operator — selects a user-friendly message based on the error status code
        const msg = err.status === 0
          ? 'Delivery service is offline. Please try again later.'
          : err.error?.message || err.error || 'Registration failed'; // '||' — logical OR; falls back through options
        alert(msg);
      }
    });
  }

  toggleAvailability() {
    // 'subscribe' — activates the Observable; triggers the HTTP PATCH call to toggle availability
    this.svc.toggleAvailability().subscribe(res => {
      // Spread operator '...' — creates a new object with all existing profile properties, overriding isAvailable
      // '!' — non-null assertion operator; tells TypeScript that profile is definitely not null/undefined here
      this.profile = { ...this.profile!, isAvailable: res.isAvailable };
      // 'if' — only loads ready orders when the agent goes online
      if (res.isAvailable) this.loadReadyOrders();
    });
  }

  // ── Vehicle edit ──────────────────────────────────────────────────────────
  openVehicleEdit() {
    // '??' — nullish coalescing operator; falls back to 'Bike' if profile is undefined
    this.editVehicleType = this.profile?.vehicleType ?? 'Bike';
    this.showVehicleEdit = true; // 'boolean' — true shows the modal overlay
  }

  closeVehicleEdit() {
    this.showVehicleEdit = false; // 'boolean' — false hides the modal overlay
  }

  saveVehicle() {
    this.savingVehicle = true;
    // 'subscribe' — activates the Observable; triggers the HTTP PATCH call to update the vehicle type
    this.svc.updateVehicle(this.editVehicleType).subscribe({
      next: updated => {
        // Spread operator '...' — creates a new object merging existing profile with the updated vehicleType
        this.profile = { ...this.profile!, vehicleType: updated.vehicleType };
        this.savingVehicle = false;
        this.showVehicleEdit = false;
      },
      error: () => { this.savingVehicle = false; alert('Failed to update vehicle.'); }
    });
  }

  // ── Delivery flow ─────────────────────────────────────────────────────────
  checkDelivery() {
    // 'subscribe' — activates the Observable; triggers the HTTP GET call to check for an active delivery
    this.svc.getMyDelivery().subscribe({
      next: d => { this.activeDelivery = d; },
      // 'undefined' — JS/TS primitive; used here to clear the active delivery when none is found
      error: () => { this.activeDelivery = undefined; this.loadReadyOrders(); }
    });
  }

  loadReadyOrders() {
    this.readyOrdersLoading = true;
    // 'subscribe' — activates the Observable; triggers the HTTP GET call for ready orders
    this.svc.getReadyOrders().subscribe({
      next: orders => { this.readyOrders = orders; this.readyOrdersLoading = false; },
      error: () => { this.readyOrders = []; this.readyOrdersLoading = false; }
    });
  }

  pickupOrder(order: OrderDto) { // 'OrderDto' — type annotation; ensures only valid order objects are passed
    this.pickingUp = order.id; // 'string' — stores the order ID to show a per-row loading state
    // 'subscribe' — activates the Observable; triggers the HTTP POST call to assign the delivery
    this.svc.pickup(order.id).subscribe({
      next: delivery => {
        this.activeDelivery = delivery as any; // 'as any' — type assertion; bypasses strict type-checking for the cast
        this.readyOrders = [];
        // 'null' — JS/TS primitive; clears the per-row loading state
        this.pickingUp = null;
        // 'if' — guards against updating a potentially undefined profile
        if (this.profile) this.profile = { ...this.profile, isAvailable: false };
      },
      error: (err) => {
        this.pickingUp = null;
        alert(err.error?.message || err.error || 'Could not pick up order.');
        this.loadReadyOrders();
      }
    });
  }

  shareLocation() {
    // 'if' — guards against calling GPS API when it is not supported by the browser
    // 'return' — exits the function early when GPS is unavailable
    if (!navigator.geolocation) { this.gpsStatus = 'GPS not supported'; return; }
    this.sharingLocation = true;
    this.gpsStatus = 'Getting location...';
    navigator.geolocation.getCurrentPosition(
      pos => {
        // Destructuring assignment — extracts latitude and longitude from the coords object
        const { latitude, longitude } = pos.coords;
        // 'subscribe' — activates the Observable; triggers the HTTP PATCH call to update the agent's location
        this.svc.updateLocation(this.profile!.id, latitude, longitude).subscribe({
          next: () => {
            this.gpsStatus = `✅ Shared: ${latitude.toFixed(4)}, ${longitude.toFixed(4)}`;
            this.sharingLocation = false;
            // 'if' — only updates the active delivery location when a delivery is in progress
            if (this.activeDelivery)
              // Spread operator '...' — creates a new object merging existing delivery with updated coordinates
              this.activeDelivery = { ...this.activeDelivery, currentLat: latitude, currentLng: longitude };
          },
          error: () => { this.gpsStatus = '❌ Failed to share location'; this.sharingLocation = false; }
        });
      },
      err => { this.gpsStatus = `GPS error: ${err.message}`; this.sharingLocation = false; }
    );
  }

  completeDelivery() {
    // 'if' — guards against completing when there is no active delivery or the user cancels the confirm dialog
    // 'return' — exits the function early in either guard case
    if (!this.activeDelivery || !confirm('Mark this order as delivered?')) return;
    const orderId = this.activeDelivery.orderId;
    this.completing = true;
    // 'subscribe' — activates the Observable; triggers the HTTP POST call to mark the delivery complete
    this.svc.complete(orderId).subscribe({
      next: () => {
        // Also update the order status in the Order API (fire-and-forget; errors are silently ignored)
        this.orderSvc.updateStatus(orderId, 5).subscribe({ error: () => {} });
        // 'undefined' — clears the active delivery reference after completion
        this.activeDelivery = undefined;
        this.completing = false;
        // 'if' — guards against updating a potentially undefined profile
        if (this.profile) {
          // Spread operator '...' — creates a new profile object with updated delivery count and earnings
          this.profile = {
            ...this.profile,
            totalDeliveries: this.profile.totalDeliveries + 1,
            totalEarnings: this.displayEarnings + this.EARN_PER_DELIVERY,
            isAvailable: true
          };
        }
        this.loadReadyOrders();
      },
      error: () => { alert('Failed to complete delivery'); this.completing = false; }
    });
  }

  updateLocation() {
    // 'if' — guards against sending a location update when both lat and lng are 0 (default/unset)
    if (!this.lat && !this.lng) return;
    this.updatingLocation = true;
    this.locationUpdated = false;
    // 'subscribe' — activates the Observable; triggers the HTTP PATCH call to update the agent's location
    this.svc.updateAgentLocation(this.lat, this.lng).subscribe({
      next: updated => {
        // Spread operator '...' — creates a new profile object with updated location coordinates
        this.profile = { ...this.profile!, currentLat: updated.currentLat, currentLng: updated.currentLng };
        this.updatingLocation = false;
        this.locationUpdated = true;
        // Auto-hide the success message after 3 seconds
        setTimeout(() => this.locationUpdated = false, 3000);
      },
      error: () => {
        this.updatingLocation = false;
        alert('Failed to update location. Make sure the delivery service is running.');
      }
    });
  }

  useGPS() {
    // 'if' — guards against calling GPS API when it is not supported by the browser
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(pos => {
      // Destructuring — extracts latitude and longitude from the GeolocationCoordinates object
      this.lat = pos.coords.latitude;
      this.lng = pos.coords.longitude;
    });
  }
}
