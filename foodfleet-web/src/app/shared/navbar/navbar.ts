import { Component, Signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AuthResponse } from '../../core/models/auth.models';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <a routerLink="/" class="brand">🛒 CraveKart</a>

      <div class="nav-links">
        <ng-container *ngIf="!user()">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Home</a>
          <a routerLink="/restaurants" routerLinkActive="active">Restaurants</a>
          <a routerLink="/auth/login" class="btn-nav-outline">Sign in</a>
          <a routerLink="/auth/register" class="btn-nav">Get started</a>
        </ng-container>

        <ng-container *ngIf="user()">
          <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Home</a>
          <a routerLink="/restaurants" routerLinkActive="active" *ngIf="isCustomer()">Browse</a>
          <a routerLink="/orders" routerLinkActive="active" *ngIf="isCustomer()">My Orders</a>
          <a routerLink="/payments" routerLinkActive="active" *ngIf="isCustomer()">Payments</a>
          <a routerLink="/owner/dashboard" routerLinkActive="active" *ngIf="isOwner()">Dashboard</a>
          <a routerLink="/admin/dashboard" routerLinkActive="active" *ngIf="isAdmin()">Admin</a>
          <a routerLink="/agent/dashboard" routerLinkActive="active" *ngIf="isAgent()">Deliveries</a>

          <div class="user-menu">
            <div class="user-avatar">{{ user()?.fullName?.charAt(0) }}</div>
            <span class="user-name">{{ user()?.fullName }}</span>
            <button (click)="logout()" class="btn-logout">Logout</button>
          </div>
        </ng-container>
      </div>
    </nav>
  `,
  styleUrl: './navbar.scss'
})
export class NavbarComponent {
  user: Signal<AuthResponse | null>;
  isCustomer: Signal<boolean>;
  isOwner: Signal<boolean>;
  isAdmin: Signal<boolean>;
  isAgent: Signal<boolean>;

  constructor(private auth: AuthService) {
    this.user = this.auth.currentUser;
    this.isCustomer = computed(() => this.auth.currentUser()?.role === 'Customer');
    this.isOwner    = computed(() => this.auth.currentUser()?.role === 'RestaurantOwner');
    this.isAdmin    = computed(() => this.auth.currentUser()?.role === 'Admin');
    this.isAgent    = computed(() => this.auth.currentUser()?.role === 'DeliveryAgent');
  }

  logout() { this.auth.logout(); }
}
