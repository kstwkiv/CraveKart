import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { RestaurantDto } from '../../../core/models/restaurant.models';

/** Sort key options for the restaurant list. */
type SortKey = 'rating' | 'delivery' | 'minOrder' | '';

/**
 * Restaurant list component.
 * Displays all active restaurants with a hero section, cuisine filter chips,
 * open-now toggle, sort options, and client-side pagination.
 * Computes hero stats (avg rating, avg delivery time) from the loaded data.
 */
@Component({
  selector: 'app-restaurant-list',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="page">

      <!-- ── Hero ── -->
      <div class="hero">
        <div class="hero-content">
          <div class="hero-tag">🔥 Trending near you</div>
          <h1>What are you<br><span>craving today?</span></h1>
          <p class="hero-sub">From biryani to burgers — your next favourite meal is just a tap away.</p>
          <div class="search-bar">
            <input [(ngModel)]="query" placeholder="🔍  Pizza, sushi, biryani..." />
            <button (click)="clearSearch()" *ngIf="query" class="btn-clear-search" title="Clear">✕</button>
          </div>
        </div>
        <div class="hero-stats">
          <div class="stat"><div class="stat-num">{{ allRestaurants.length }}+</div><div class="stat-lbl">Restaurants</div></div>
          <div class="stat"><div class="stat-num">~{{ avgDeliveryTime }} min</div><div class="stat-lbl">Avg Delivery</div></div>
          <div class="stat"><div class="stat-num">{{ avgRating }} ★</div><div class="stat-lbl">Avg Rating</div></div>
        </div>
      </div>

      <div class="container">
        <div *ngIf="loading" class="loading-state">
          <div class="icon">🍽️</div>
          <p>Finding the best spots for you...</p>
        </div>

        <ng-container *ngIf="!loading">

          <!-- ── Filter bar ── -->
          <div class="filter-bar">

            <!-- Cuisine chips -->
            <div class="filter-group">
              <button class="cuisine-chip" [class.active]="!activeCuisine" (click)="activeCuisine = ''">All</button>
              <button *ngFor="let c of cuisineOptions" class="cuisine-chip" [class.active]="activeCuisine === c" (click)="toggleCuisine(c)">
                {{ c }}
              </button>
            </div>

            <!-- Right-side controls -->
            <div class="filter-controls">
              <!-- Open now toggle -->
              <label class="toggle-pill" [class.active]="onlyOpen">
                <input type="checkbox" [(ngModel)]="onlyOpen" />
                <span class="toggle-dot"></span>
                <span>Open now</span>
              </label>

              <!-- Sort -->
              <div class="sort-wrap">
                <select [(ngModel)]="sortBy" class="sort-select">
                  <option value="">Sort: Default</option>
                  <option value="rating">⭐ Top Rated</option>
                  <option value="delivery">🕐 Fastest Delivery</option>
                  <option value="minOrder"> ₹ Min Order</option>
                </select>
              </div>

              <!-- Clear all -->
              <button class="btn-clear-filters" *ngIf="hasActiveFilters" (click)="clearFilters()">
                ✕ Clear filters
              </button>
            </div>
          </div>

          <!-- ── Results header ── -->
          <div class="section-header">
            <h2>
              <ng-container *ngIf="query">🔍 Results for "{{ query }}"</ng-container>
              <ng-container *ngIf="!query && activeCuisine">🍴 {{ activeCuisine }}</ng-container>
              <ng-container *ngIf="!query && !activeCuisine">🍴 All Restaurants</ng-container>
            </h2>
            <span class="count">{{ filtered.length }} place{{ filtered.length !== 1 ? 's' : '' }}</span>
          </div>

          <!-- ── Grid ── -->
          <div class="grid">
            <a *ngFor="let r of paged" [routerLink]="['/restaurants', r.id]" class="card">
              <div class="card-img">
                <img *ngIf="r.logoUrl" [src]="r.logoUrl" [alt]="r.name" />
                <div *ngIf="!r.logoUrl" class="placeholder-img">🍽️</div>
                <span class="card-badge" [class.open]="r.isOpen" [class.closed]="!r.isOpen">
                  {{ r.isOpen ? '● Open now' : '● Closed' }}
                </span>
              </div>
              <div class="card-body">
                <h3>{{ r.name }}</h3>
                <p class="cuisine">{{ r.cuisineTypes }}</p>
                <div class="card-meta">
                  <div class="meta-item">
                    <span class="meta-val">⭐ {{ r.averageRating | number:'1.1-1' }}</span>
                    <span class="meta-lbl">{{ r.totalReviews }} reviews</span>
                  </div>
                  <div class="meta-item">
                    <span class="meta-val">🕐 {{ r.estimatedDeliveryMinutes }} min</span>
                  </div>
                  <div class="meta-item">
                    <span class="meta-val">₹{{ r.minimumOrderAmount }}+</span>
                  </div>
                </div>
              </div>
            </a>
          </div>

          <!-- ── Pagination ── -->
          <div class="pagination" *ngIf="totalPages > 1">
            <button class="page-btn" (click)="goTo(currentPage - 1)" [disabled]="currentPage === 1">‹</button>
            <button *ngFor="let p of pageNumbers" class="page-btn"
              [class.active]="p === currentPage"
              (click)="goTo(p)">{{ p }}</button>
            <button class="page-btn" (click)="goTo(currentPage + 1)" [disabled]="currentPage === totalPages">›</button>
          </div>

          <!-- ── Empty state ── -->
          <div *ngIf="filtered.length === 0" class="empty-state">
            <div class="icon">😕</div>
            <p *ngIf="hasActiveFilters">No restaurants match your filters.<br>
              <button class="btn-link" (click)="clearFilters()">Clear all filters</button>
            </p>
            <p *ngIf="!hasActiveFilters">No restaurants available right now.</p>
          </div>

        </ng-container>
      </div>
    </div>
  `,
  styleUrl: './restaurant-list.scss'
})
export class RestaurantListComponent implements OnInit {
  allRestaurants: RestaurantDto[] = [];
  loading = true;

  // Filter state
  query = '';
  activeCuisine = '';
  onlyOpen = false;
  sortBy: SortKey = '';

  // Pagination
  readonly PAGE_SIZE = 6;
  currentPage = 1;

  constructor(private svc: RestaurantService) {}

  ngOnInit() {
    this.svc.getAll().subscribe({
      next: data => { this.allRestaurants = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  // ── Hero stats computed from real data ────────────────────────────────
  get avgRating(): string {
    const rated = this.allRestaurants.filter(r => r.averageRating > 0);
    if (!rated.length) return '—';
    const avg = rated.reduce((s, r) => s + r.averageRating, 0) / rated.length;
    return avg.toFixed(1);
  }

  get avgDeliveryTime(): number {
    if (!this.allRestaurants.length) return 30;
    const avg = this.allRestaurants.reduce((s, r) => s + r.estimatedDeliveryMinutes, 0) / this.allRestaurants.length;
    return Math.round(avg);
  }

  // ── Derived cuisine list from loaded data ──────────────────────────────
  get cuisineOptions(): string[] {
    const all = this.allRestaurants.flatMap(r =>
      r.cuisineTypes.split(',').map(c => c.trim()).filter(Boolean)
    );
    return [...new Set(all)].sort();
  }

  // ── Main filtered + sorted list ────────────────────────────────────────
  get filtered(): RestaurantDto[] {
    let list = this.allRestaurants;

    if (this.query.trim()) {
      const q = this.query.toLowerCase();
      list = list.filter(r =>
        r.name.toLowerCase().includes(q) ||
        r.cuisineTypes.toLowerCase().includes(q) ||
        r.description?.toLowerCase().includes(q)
      );
    }

    if (this.activeCuisine) {
      list = list.filter(r =>
        r.cuisineTypes.split(',').map(c => c.trim()).includes(this.activeCuisine)
      );
    }

    if (this.onlyOpen) {
      list = list.filter(r => r.isOpen);
    }

    if (this.sortBy === 'rating') {
      list = [...list].sort((a, b) => b.averageRating - a.averageRating);
    } else if (this.sortBy === 'delivery') {
      list = [...list].sort((a, b) => a.estimatedDeliveryMinutes - b.estimatedDeliveryMinutes);
    } else if (this.sortBy === 'minOrder') {
      list = [...list].sort((a, b) => a.minimumOrderAmount - b.minimumOrderAmount);
    }

    return list;
  }

  // ── Pagination ─────────────────────────────────────────────────────────
  get totalPages(): number {
    return Math.ceil(this.filtered.length / this.PAGE_SIZE);
  }

  get paged(): RestaurantDto[] {
    const start = (this.currentPage - 1) * this.PAGE_SIZE;
    return this.filtered.slice(start, start + this.PAGE_SIZE);
  }

  get pageNumbers(): number[] {
    const total = this.totalPages;
    const cur = this.currentPage;
    // Show at most 7 page buttons with ellipsis logic
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const pages: number[] = [];
    for (let i = Math.max(1, cur - 2); i <= Math.min(total, cur + 2); i++) pages.push(i);
    if (pages[0] > 1) pages.unshift(1);
    if (pages[pages.length - 1] < total) pages.push(total);
    return pages;
  }

  goTo(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // Reset to page 1 whenever filters change
  get hasActiveFilters(): boolean {
    return !!(this.query || this.activeCuisine || this.onlyOpen || this.sortBy);
  }

  toggleCuisine(c: string) {
    this.activeCuisine = this.activeCuisine === c ? '' : c;
    this.currentPage = 1;
  }

  clearSearch() { this.query = ''; this.currentPage = 1; }

  clearFilters() {
    this.query = '';
    this.activeCuisine = '';
    this.onlyOpen = false;
    this.sortBy = '';
    this.currentPage = 1;
  }

  cuisineEmoji(cuisine: string): string {
    const map: Record<string, string> = {
      pizza: '🍕', burger: '🍔', biryani: '🍛', sushi: '🍣',
      chinese: '🥡', italian: '🍝', indian: '🌶️', mexican: '🌮',
      thai: '🍜', desserts: '🍰', cafe: '☕', sandwich: '🥪',
      kebab: '🥙', seafood: '🦞', vegan: '🥗', south: '🍚',
    };
    const key = cuisine.toLowerCase();
    return Object.entries(map).find(([k]) => key.includes(k))?.[1] ?? '🍴';
  }
}
