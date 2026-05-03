// import: ES module keyword — pulls named exports from external packages/files into this file's scope
// Routes: TypeScript type alias exported by @angular/router — it is an Array of Route objects that the Angular router consumes
import { Routes } from '@angular/router';
// authGuard, roleGuard: route guard functions — they implement CanActivateFn and return true/false (or a UrlTree redirect)
// to control whether a navigation to a protected route is allowed
import { authGuard, roleGuard } from './core/guards/auth.guard';

/**
 * Application route definitions.
 * All feature routes are lazy-loaded for optimal bundle splitting.
 * Protected routes use {@link authGuard} or {@link roleGuard} to enforce access control.
 */
// export: makes the `routes` constant importable by the Angular bootstrapping code (provideRouter in main.ts)
// const: block-scoped constant — the binding cannot be reassigned; the array itself is still mutable
// Routes: type annotation — tells TypeScript (and the Angular compiler) that this value must be a valid route configuration array
export const routes: Routes = [
  // path: '': the default/root route — matched when the URL has no path segment after the origin
  // loadComponent: lazy-loading function — returns a dynamic import() Promise; Angular splits this into a separate JS chunk
  // import(): ES dynamic import — defers loading the module until the route is first navigated to (code splitting)
  // .then(m => m.LandingComponent): resolves the module and picks the named export to use as the routed component
  { path: '', loadComponent: () => import('./features/landing/landing').then(m => m.LandingComponent) },

  // Auth
  {
    path: 'auth',
    // children: nested route array — these routes are matched relative to the parent 'auth' path segment
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent) },
      { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPasswordComponent) },
    ]
  },

  // Public routes — no authentication required
  { path: 'restaurants', loadComponent: () => import('./features/restaurants/restaurant-list/restaurant-list').then(m => m.RestaurantListComponent) },
  // :id — route parameter token; Angular captures the URL segment and makes it available via ActivatedRoute.snapshot.paramMap
  { path: 'restaurants/:id', loadComponent: () => import('./features/restaurants/restaurant-detail/restaurant-detail').then(m => m.RestaurantDetailComponent) },

  // Customer routes — protected by authGuard (user must be authenticated)
  // canActivate: route property — an array of guard functions run before the route is activated; navigation is blocked if any returns false
  { path: 'orders', loadComponent: () => import('./features/orders/order-history/order-history').then(m => m.OrderHistoryComponent), canActivate: [authGuard] },
  { path: 'orders/:id', loadComponent: () => import('./features/orders/order-detail/order-detail').then(m => m.OrderDetailComponent), canActivate: [authGuard] },
  { path: 'payments', loadComponent: () => import('./features/payments/payment-history/payment-history').then(m => m.PaymentHistoryComponent), canActivate: [authGuard] },

  // Restaurant Owner routes — protected by roleGuard; only users with the 'RestaurantOwner' role may access these
  // roleGuard(['RestaurantOwner']): higher-order guard factory — returns a CanActivateFn that checks the user's role claim
  { path: 'owner/dashboard', loadComponent: () => import('./features/owner/dashboard/owner-dashboard').then(m => m.OwnerDashboardComponent), canActivate: [roleGuard(['RestaurantOwner'])] },
  { path: 'owner/create-restaurant', loadComponent: () => import('./features/owner/create-restaurant/create-restaurant').then(m => m.CreateRestaurantComponent), canActivate: [roleGuard(['RestaurantOwner'])] },
  { path: 'owner/edit-restaurant/:id', loadComponent: () => import('./features/owner/edit-restaurant/edit-restaurant').then(m => m.EditRestaurantComponent), canActivate: [roleGuard(['RestaurantOwner'])] },
  { path: 'owner/menu/:id', loadComponent: () => import('./features/owner/menu/owner-menu').then(m => m.OwnerMenuComponent), canActivate: [roleGuard(['RestaurantOwner'])] },

  // Admin routes — protected by roleGuard; only users with the 'Admin' role may access these
  { path: 'admin/dashboard', loadComponent: () => import('./features/admin/dashboard/admin-dashboard').then(m => m.AdminDashboardComponent), canActivate: [roleGuard(['Admin'])] },
  { path: 'admin/restaurants', loadComponent: () => import('./features/admin/restaurants/admin-restaurants').then(m => m.AdminRestaurantsComponent), canActivate: [roleGuard(['Admin'])] },
  { path: 'admin/orders', loadComponent: () => import('./features/admin/orders/admin-orders').then(m => m.AdminOrdersComponent), canActivate: [roleGuard(['Admin'])] },

  // Delivery Agent routes — protected by roleGuard; only users with the 'DeliveryAgent' role may access these
  { path: 'agent/dashboard', loadComponent: () => import('./features/agent/dashboard/agent-dashboard').then(m => m.AgentDashboardComponent), canActivate: [roleGuard(['DeliveryAgent'])] },

  // **: wildcard route — matches any URL that did not match a previous route; acts as a 404 fallback
  // redirectTo: route property — instructs the router to navigate to the specified path instead of rendering a component
  { path: '**', redirectTo: '/restaurants' }
];
