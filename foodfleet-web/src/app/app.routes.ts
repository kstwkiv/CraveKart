import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/landing/landing').then(m => m.LandingComponent) },

  // Auth
  {
    path: 'auth',
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent) },
      { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPasswordComponent) },
    ]
  },

  // Public
  { path: 'restaurants', loadComponent: () => import('./features/restaurants/restaurant-list/restaurant-list').then(m => m.RestaurantListComponent) },
  { path: 'restaurants/:id', loadComponent: () => import('./features/restaurants/restaurant-detail/restaurant-detail').then(m => m.RestaurantDetailComponent) },

  // Customer
  { path: 'orders', loadComponent: () => import('./features/orders/order-history/order-history').then(m => m.OrderHistoryComponent), canActivate: [authGuard] },
  { path: 'orders/:id', loadComponent: () => import('./features/orders/order-detail/order-detail').then(m => m.OrderDetailComponent), canActivate: [authGuard] },

  // Restaurant Owner
  { path: 'owner/dashboard', loadComponent: () => import('./features/owner/dashboard/owner-dashboard').then(m => m.OwnerDashboardComponent), canActivate: [roleGuard(['RestaurantOwner'])] },
  { path: 'owner/create-restaurant', loadComponent: () => import('./features/owner/create-restaurant/create-restaurant').then(m => m.CreateRestaurantComponent), canActivate: [roleGuard(['RestaurantOwner'])] },
  { path: 'owner/edit-restaurant/:id', loadComponent: () => import('./features/owner/edit-restaurant/edit-restaurant').then(m => m.EditRestaurantComponent), canActivate: [roleGuard(['RestaurantOwner'])] },
  { path: 'owner/menu/:id', loadComponent: () => import('./features/owner/menu/owner-menu').then(m => m.OwnerMenuComponent), canActivate: [roleGuard(['RestaurantOwner'])] },

  // Admin
  { path: 'admin/dashboard', loadComponent: () => import('./features/admin/dashboard/admin-dashboard').then(m => m.AdminDashboardComponent), canActivate: [roleGuard(['Admin'])] },
  { path: 'admin/restaurants', loadComponent: () => import('./features/admin/restaurants/admin-restaurants').then(m => m.AdminRestaurantsComponent), canActivate: [roleGuard(['Admin'])] },
  { path: 'admin/orders', loadComponent: () => import('./features/admin/orders/admin-orders').then(m => m.AdminOrdersComponent), canActivate: [roleGuard(['Admin'])] },

  // Delivery Agent
  { path: 'agent/dashboard', loadComponent: () => import('./features/agent/dashboard/agent-dashboard').then(m => m.AgentDashboardComponent), canActivate: [roleGuard(['DeliveryAgent'])] },

  { path: '**', redirectTo: '/restaurants' }
];
