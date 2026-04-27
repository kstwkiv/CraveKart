import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { RestaurantDto } from '../../../core/models/restaurant.models';

@Component({
  selector: 'app-admin-restaurants',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h2>Restaurant Management</h2>
          <p class="subtitle">Review and manage all restaurants</p>
        </div>
        <a routerLink="/admin/dashboard" class="btn-back">← Back to Dashboard</a>
      </div>

      <div class="filters">
        <button *ngFor="let s of statuses" class="filter-btn"
          [class.active]="activeStatus === s"
          (click)="setStatus(s)">
          {{ s }}
        </button>
      </div>

      <div class="loading" *ngIf="loading">Loading...</div>

      <div class="table-wrap" *ngIf="!loading">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Address</th>
              <th>Cuisine</th>
              <th>Rating</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let r of restaurants">
              <td class="name">{{ r.name }}</td>
              <td class="addr">{{ r.address }}</td>
              <td>{{ r.cuisineTypes }}</td>
              <td>⭐ {{ r.averageRating | number:'1.1-1' }}</td>
              <td><span class="badge" [class]="r.status.toLowerCase()">{{ r.status }}</span></td>
              <td class="actions">
                <button *ngIf="r.status === 'Pending'" class="btn-approve" (click)="approve(r)">Approve</button>
                <button *ngIf="r.status === 'Pending'" class="btn-reject" (click)="reject(r)">Reject</button>
                <button *ngIf="r.status === 'Active'" class="btn-suspend" (click)="suspend(r)">Suspend</button>
                <span *ngIf="r.status === 'Rejected'" class="muted">Rejected</span>
                <span *ngIf="r.status === 'Suspended'" class="muted">Suspended</span>
              </td>
            </tr>
            <tr *ngIf="restaurants.length === 0">
              <td colspan="6" class="empty">No restaurants found</td>
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
    .filter-btn { padding: 0.4rem 1rem; border: 1px solid var(--border); border-radius: 20px; background: var(--surface); cursor: pointer; font-size: 0.85rem; font-weight: 500; color: var(--text-secondary); transition: all 0.15s; }
    .filter-btn.active { background: var(--primary); color: white; border-color: var(--primary); }
    .table-wrap { background: var(--surface); border-radius: 12px; box-shadow: var(--shadow); border: 1px solid var(--border); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; }
    th { background: var(--surface-alt); padding: 0.75rem 1rem; text-align: left; font-size: 0.78rem; font-weight: 700; text-transform: uppercase; color: var(--text-muted); letter-spacing: 0.05em; }
    td { padding: 0.875rem 1rem; border-top: 1px solid var(--border); font-size: 0.875rem; vertical-align: middle; color: var(--text-primary); }
    td.name { font-weight: 600; }
    td.addr { color: var(--text-muted); font-size: 0.8rem; max-width: 180px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .badge { padding: 0.25rem 0.6rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
    .badge.pending   { background: #ede0f8; color: var(--primary); }
    .badge.active    { background: #d0f0f0; color: var(--accent); }
    .badge.rejected  { background: #fce8ee; color: var(--danger); }
    .badge.suspended { background: var(--surface-alt); color: var(--text-secondary); }
    .actions { display: flex; gap: 0.4rem; }
    .btn-approve { padding: 0.3rem 0.7rem; background: var(--accent); color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .btn-reject  { padding: 0.3rem 0.7rem; background: var(--danger); color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .btn-suspend { padding: 0.3rem 0.7rem; background: var(--warning); color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 0.78rem; font-weight: 600; }
    .muted { color: var(--text-muted); font-size: 0.8rem; }
    .empty { text-align: center; color: var(--text-muted); padding: 2rem; }
    .loading { text-align: center; padding: 3rem; color: var(--text-muted); }
  `]
})
export class AdminRestaurantsComponent implements OnInit {
  restaurants: RestaurantDto[] = [];
  loading = false;
  activeStatus = 'Pending';
  statuses = ['All', 'Pending', 'Active', 'Rejected', 'Suspended'];

  constructor(private restaurantSvc: RestaurantService) {}

  ngOnInit() { this.load(); }

  setStatus(status: string) {
    this.activeStatus = status;
    this.load();
  }

  load() {
    this.loading = true;
    const status = this.activeStatus === 'All' ? undefined : this.activeStatus;
    this.restaurantSvc.adminGetAll(status).subscribe({
      next: r => { this.restaurants = r; this.loading = false; },
      error: () => this.loading = false
    });
  }

  approve(r: RestaurantDto) {
    this.restaurantSvc.approve(r.id).subscribe(() => this.load());
  }

  reject(r: RestaurantDto) {
    const reason = prompt('Reason for rejection:');
    if (!reason) return;
    this.restaurantSvc.reject(r.id, reason).subscribe(() => this.load());
  }

  suspend(r: RestaurantDto) {
    const reason = prompt('Reason for suspension:');
    if (!reason) return;
    this.restaurantSvc.suspend(r.id, reason).subscribe(() => this.load());
  }
}
