import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DeliveryService } from '../../../core/services/delivery.service';
import { OrderService } from '../../../core/services/order.service';
import { DeliveryAgentDto } from '../../../core/models/delivery.models';
import { OrderDto } from '../../../core/models/order.models';

interface ActiveDelivery {
  id: string;
  orderId: string;
  status: string;
  currentLat?: number;
  currentLng?: number;
  assignedAt: string;
}

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
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
            <div class="stat-trend">+₹100 per delivery</div>
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
              <strong>₹100.00</strong>
            </div>
            <div class="panel-row">
              <span>Average per Delivery</span>
              <strong>₹{{ avgEarnings | number:'1.0-0' }}</strong>
            </div>
            <div class="panel-row muted">
              <span>Next delivery will earn</span>
              <strong class="green">+₹100</strong>
            </div>
            <div class="panel-note">
              💡 Earnings are calculated at ₹100 per completed delivery. Payouts are processed weekly.
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
export class AgentDashboardComponent implements OnInit, OnDestroy {
  profile?: DeliveryAgentDto;
  activeDelivery?: ActiveDelivery;
  readyOrders: OrderDto[] = [];
  readyOrdersLoading = false;
  loading = true;
  completing = false;
  pickingUp: string | null = null;
  vehicleType = 'Bike';
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

  private readonly EARN_PER_DELIVERY = 100;

  // Stat panels
  activePanel: 'earnings' | 'deliveries' | 'vehicle' | 'status' | null = null;

  togglePanel(panel: 'earnings' | 'deliveries' | 'vehicle' | 'status') {
    this.activePanel = this.activePanel === panel ? null : panel;
  }

  readonly vehicleOptions = [
    { value: 'Bike',    label: 'Bike',    emoji: '🏍️' },
    { value: 'Scooter', label: 'Scooter', emoji: '🛵' },
    { value: 'Car',     label: 'Car',     emoji: '🚗' },
    { value: 'Bicycle', label: 'Bicycle', emoji: '🚲' },
  ];

  private pollInterval?: ReturnType<typeof setInterval>;

  constructor(private svc: DeliveryService, private orderSvc: OrderService) {}

  ngOnInit() {
    this.svc.getMyProfile().subscribe({
      next: p => {
        this.profile = p;
        this.loading = false;
        this.checkDelivery();
        this.startPolling();
      },
      error: () => { this.loading = false; }
    });
  }

  ngOnDestroy() {
    if (this.pollInterval) clearInterval(this.pollInterval);
  }

  get avgEarnings(): number {
    if (!this.profile?.totalDeliveries) return 0;
    return this.displayEarnings / this.profile.totalDeliveries;
  }

  /** Auto-calculate earnings from deliveries × ₹100 if backend hasn't stored it yet */
  get displayEarnings(): number {
    if (!this.profile) return 0;
    return this.profile.totalEarnings > 0
      ? this.profile.totalEarnings
      : this.profile.totalDeliveries * this.EARN_PER_DELIVERY;
  }

  vehicleEmoji(type: string): string {
    return this.vehicleOptions.find(v => v.value === type)?.emoji ?? '🚴';
  }

  private startPolling() {
    this.pollInterval = setInterval(() => {
      if (this.profile?.isAvailable && !this.activeDelivery) this.loadReadyOrders();
      else if (this.activeDelivery) this.checkDelivery();
    }, 10_000);
  }

  register() {
    this.svc.registerAgent(this.vehicleType).subscribe({
      next: p => { this.profile = p; this.loadReadyOrders(); this.startPolling(); },
      error: (err) => {
        const msg = err.status === 0
          ? 'Delivery service is offline. Please try again later.'
          : err.error?.message || err.error || 'Registration failed';
        alert(msg);
      }
    });
  }

  toggleAvailability() {
    this.svc.toggleAvailability().subscribe(res => {
      this.profile = { ...this.profile!, isAvailable: res.isAvailable };
      if (res.isAvailable) this.loadReadyOrders();
    });
  }

  // ── Vehicle edit ──────────────────────────────────────────────────────────
  openVehicleEdit() {
    this.editVehicleType = this.profile?.vehicleType ?? 'Bike';
    this.showVehicleEdit = true;
  }

  closeVehicleEdit() {
    this.showVehicleEdit = false;
  }

  saveVehicle() {
    this.savingVehicle = true;
    this.svc.updateVehicle(this.editVehicleType).subscribe({
      next: updated => {
        this.profile = { ...this.profile!, vehicleType: updated.vehicleType };
        this.savingVehicle = false;
        this.showVehicleEdit = false;
      },
      error: () => { this.savingVehicle = false; alert('Failed to update vehicle.'); }
    });
  }

  // ── Delivery flow ─────────────────────────────────────────────────────────
  checkDelivery() {
    this.svc.getMyDelivery().subscribe({
      next: d => { this.activeDelivery = d; },
      error: () => { this.activeDelivery = undefined; this.loadReadyOrders(); }
    });
  }

  loadReadyOrders() {
    this.readyOrdersLoading = true;
    this.svc.getReadyOrders().subscribe({
      next: orders => { this.readyOrders = orders; this.readyOrdersLoading = false; },
      error: () => { this.readyOrders = []; this.readyOrdersLoading = false; }
    });
  }

  pickupOrder(order: OrderDto) {
    this.pickingUp = order.id;
    this.svc.pickup(order.id).subscribe({
      next: delivery => {
        this.activeDelivery = delivery as any;
        this.readyOrders = [];
        this.pickingUp = null;
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
    if (!navigator.geolocation) { this.gpsStatus = 'GPS not supported'; return; }
    this.sharingLocation = true;
    this.gpsStatus = 'Getting location...';
    navigator.geolocation.getCurrentPosition(
      pos => {
        const { latitude, longitude } = pos.coords;
        this.svc.updateLocation(this.profile!.id, latitude, longitude).subscribe({
          next: () => {
            this.gpsStatus = `✅ Shared: ${latitude.toFixed(4)}, ${longitude.toFixed(4)}`;
            this.sharingLocation = false;
            if (this.activeDelivery)
              this.activeDelivery = { ...this.activeDelivery, currentLat: latitude, currentLng: longitude };
          },
          error: () => { this.gpsStatus = '❌ Failed to share location'; this.sharingLocation = false; }
        });
      },
      err => { this.gpsStatus = `GPS error: ${err.message}`; this.sharingLocation = false; }
    );
  }

  completeDelivery() {
    if (!this.activeDelivery || !confirm('Mark this order as delivered?')) return;
    const orderId = this.activeDelivery.orderId;
    this.completing = true;
    this.svc.complete(orderId).subscribe({
      next: () => {
        this.orderSvc.updateStatus(orderId, 5).subscribe({ error: () => {} });
        this.activeDelivery = undefined;
        this.completing = false;
        if (this.profile) {
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
    if (!this.lat && !this.lng) return;
    this.updatingLocation = true;
    this.locationUpdated = false;
    this.svc.updateAgentLocation(this.lat, this.lng).subscribe({
      next: updated => {
        this.profile = { ...this.profile!, currentLat: updated.currentLat, currentLng: updated.currentLng };
        this.updatingLocation = false;
        this.locationUpdated = true;
        setTimeout(() => this.locationUpdated = false, 3000);
      },
      error: () => {
        this.updatingLocation = false;
        alert('Failed to update location. Make sure the delivery service is running.');
      }
    });
  }

  useGPS() {
    if (!navigator.geolocation) return;
    navigator.geolocation.getCurrentPosition(pos => {
      this.lat = pos.coords.latitude;
      this.lng = pos.coords.longitude;
    });
  }
}
