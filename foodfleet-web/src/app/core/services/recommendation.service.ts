import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { OrderService } from './order.service';
import { RestaurantService } from './restaurant.service';
import { RestaurantDto } from '../models/restaurant.models';

export interface Recommendation {
  restaurant: RestaurantDto;
  reason: string;       // e.g. "Because you ordered Indian food"
  matchScore: number;   // higher = better match
}

@Injectable({ providedIn: 'root' })
export class RecommendationService {

  constructor(
    private orderSvc: OrderService,
    private restaurantSvc: RestaurantService
  ) {}

  /**
   * Returns up to `limit` restaurant recommendations for a customer,
   * derived purely from their order history + the restaurant catalogue.
   */
  getRecommendations(customerId: string, limit = 6): Observable<Recommendation[]> {
    return forkJoin({
      orders:      this.orderSvc.getHistory(customerId).pipe(catchError(() => of([]))),
      restaurants: this.restaurantSvc.getAll().pipe(catchError(() => of([])))
    }).pipe(
      map(({ orders, restaurants }) => this.compute(orders, restaurants, limit))
    );
  }

  // ── Core logic ────────────────────────────────────────────────────────────
  private compute(
    orders: any[],
    restaurants: RestaurantDto[],
    limit: number
  ): Recommendation[] {

    if (!orders.length || !restaurants.length) return [];

    // Only consider delivered orders for preference signals
    const delivered = orders.filter(o => {
      const s = typeof o.status === 'number' ? o.status : -1;
      const str = typeof o.status === 'string' ? o.status : '';
      return s === 5 || str === 'Delivered';
    });

    // Restaurants the customer has already ordered from (any status)
    const orderedRestaurantIds = new Set(orders.map((o: any) => o.restaurantId));

    // Build cuisine frequency map from delivered orders
    const cuisineFreq: Record<string, number> = {};
    for (const o of delivered) {
      const cuisines = (o.restaurantCuisineTypes ?? '')
        .split(',')
        .map((c: string) => c.trim().toLowerCase())
        .filter(Boolean);
      for (const c of cuisines) {
        cuisineFreq[c] = (cuisineFreq[c] ?? 0) + 1;
      }
    }

    // Build restaurant-name frequency map (for "order again" signal)
    const restaurantFreq: Record<string, number> = {};
    for (const o of delivered) {
      if (o.restaurantId) {
        restaurantFreq[o.restaurantId] = (restaurantFreq[o.restaurantId] ?? 0) + 1;
      }
    }

    // Top cuisines ordered (sorted by frequency)
    const topCuisines = Object.entries(cuisineFreq)
      .sort((a, b) => b[1] - a[1])
      .map(([c]) => c);

    const scored: Recommendation[] = [];

    for (const r of restaurants) {
      // Skip closed restaurants
      if (!r.isOpen) continue;

      const rCuisines = r.cuisineTypes
        .split(',')
        .map(c => c.trim().toLowerCase())
        .filter(Boolean);

      let score = 0;
      let reason = '';

      // Signal 1: cuisine match (weighted by how often they ordered that cuisine)
      let bestCuisineMatch = '';
      let bestCuisineScore = 0;
      for (const rc of rCuisines) {
        const freq = cuisineFreq[rc] ?? 0;
        if (freq > bestCuisineScore) {
          bestCuisineScore = freq;
          bestCuisineMatch = rc;
        }
      }
      if (bestCuisineScore > 0) {
        score += bestCuisineScore * 10;
        reason = `Because you love ${this.capitalize(bestCuisineMatch)} food`;
      }

      // Signal 2: high rating bonus
      if (r.averageRating >= 4.5) {
        score += 8;
        if (!reason) reason = 'Highly rated by customers';
      } else if (r.averageRating >= 4.0) {
        score += 4;
      }

      // Signal 3: "order again" — already ordered from here and rated well
      if (orderedRestaurantIds.has(r.id)) {
        const freq = restaurantFreq[r.id] ?? 0;
        if (freq >= 2) {
          score += freq * 5;
          reason = `You've ordered here ${freq} times`;
        } else {
          // Already tried once — slight boost but don't over-prioritise
          score += 2;
          if (!reason) reason = 'You ordered here before';
        }
      }

      // Signal 4: fast delivery bonus
      if (r.estimatedDeliveryMinutes <= 25) score += 3;

      // Only include if there's a meaningful reason
      if (score > 0 && reason) {
        scored.push({ restaurant: r, reason, matchScore: score });
      }
    }

    // Sort by score desc, then by rating as tiebreaker
    scored.sort((a, b) =>
      b.matchScore - a.matchScore ||
      b.restaurant.averageRating - a.restaurant.averageRating
    );

    return scored.slice(0, limit);
  }

  private capitalize(s: string): string {
    return s.charAt(0).toUpperCase() + s.slice(1);
  }
}
