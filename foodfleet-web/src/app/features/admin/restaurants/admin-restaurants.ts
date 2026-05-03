// 'import' — ES module keyword; pulls named exports from another module into this file's scope
// 'Component' — Angular decorator factory; marks a class as a component and attaches template/style metadata
// 'OnInit'    — Angular lifecycle-hook interface; requires ngOnInit() to be implemented; called after first ngOnChanges
import { Component, OnInit } from '@angular/core';
// 'CommonModule' — provides *ngIf, *ngFor, async pipe, and other structural directives
import { CommonModule } from '@angular/common';
// 'FormsModule' — Angular module that enables template-driven forms and [(ngModel)] two-way binding
import { FormsModule } from '@angular/forms';
// 'RouterLink' — directive that turns an anchor into a client-side navigation link
import { RouterLink } from '@angular/router';
// 'RestaurantService' — application service that wraps HTTP calls to the Restaurant API
import { RestaurantService } from '../../../core/services/restaurant.service';
// 'RestaurantDto' — TypeScript interface (pure type contract) for a restaurant data transfer object
import { RestaurantDto } from '../../../core/models/restaurant.models';

/**
 * Admin restaurant management component.
 * Lists all restaurants with status filter tabs and provides approve,
 * reject, and suspend actions for each restaurant.
 */
// '@Component' — class decorator; Angular reads this metadata to compile the template and wire up DI
@Component({
  selector: 'app-admin-restaurants',  // CSS selector used in templates/router to render this component
  standalone: true,                   // standalone: true — no NgModule needed; the component manages its own imports
  imports: [CommonModule, FormsModule, RouterLink], // 'imports' — Angular dependencies for this component's template
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
// 'export' — makes this class importable by the Angular router and other modules
// 'class'  — ES6/TypeScript keyword; defines a reusable blueprint with properties and methods
// 'implements' — TypeScript keyword; enforces that the class satisfies the listed interface contracts
// 'OnInit' — Angular lifecycle interface being implemented; requires ngOnInit() method
export class AdminRestaurantsComponent implements OnInit {
  // 'RestaurantDto[]' — Array type annotation; an ordered collection of RestaurantDto objects
  restaurants: RestaurantDto[] = []; // '= []' — initialised to an empty array to avoid null-reference errors in the template
  // 'boolean' (inferred) — primitive type; true/false flag to show/hide the loading indicator
  loading = false;
  // 'string' (inferred) — tracks which status filter tab is currently active
  activeStatus = 'Pending';
  // Array of string literals — the available filter tab labels
  statuses = ['All', 'Pending', 'Active', 'Rejected', 'Suspended'];

  // 'constructor' — called by Angular's DI system when instantiating this class
  // 'private' — access modifier; the injected service is only accessible within this class
  constructor(private restaurantSvc: RestaurantService) {}

  // 'ngOnInit' — Angular lifecycle hook method; called once after the component's inputs are first set
  ngOnInit() { this.load(); }

  setStatus(status: string) { // 'string' — parameter type annotation
    this.activeStatus = status;
    this.load();
  }

  load() {
    this.loading = true;
    // 'undefined' — JS/TS primitive; used here to signal "no filter" to the service when status is 'All'
    const status = this.activeStatus === 'All' ? undefined : this.activeStatus;
    // 'subscribe' — RxJS method that activates an Observable and registers callbacks for next/error/complete
    this.restaurantSvc.adminGetAll(status).subscribe({
      // 'next' — callback invoked when the Observable emits a value (successful HTTP response)
      next: r => { this.restaurants = r; this.loading = false; },
      // 'error' — callback invoked when the Observable errors (HTTP error, network failure, etc.)
      error: () => this.loading = false
    });
  }

  approve(r: RestaurantDto) { // 'RestaurantDto' — type annotation; ensures only valid restaurant objects are passed
    // 'subscribe' — activates the Observable; triggers the HTTP call and reloads the list on success
    this.restaurantSvc.approve(r.id).subscribe(() => this.load());
  }

  reject(r: RestaurantDto) {
    // 'null' — JS/TS primitive; prompt() returns null when the user cancels the dialog
    // 'if' — guards against sending a rejection with no reason
    // 'return' — exits the function early if no reason was provided
    const reason = prompt('Reason for rejection:');
    if (!reason) return;
    this.restaurantSvc.reject(r.id, reason).subscribe(() => this.load());
  }

  suspend(r: RestaurantDto) {
    const reason = prompt('Reason for suspension:');
    // 'if' — guards against sending a suspension with no reason
    if (!reason) return;
    this.restaurantSvc.suspend(r.id, reason).subscribe(() => this.load());
  }
}
