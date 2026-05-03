import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { OrderDto, PlaceOrderRequest, OrderStats } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private base = `${environment.apiUrl}/orders`;
  private adminBase = `${environment.apiUrl}/admin/orders`;

  constructor(private http: HttpClient) {}

  /**
   * Places a new food order.
   * @param req - The order request payload.
   * @returns An observable that emits the created {@link OrderDto}.
   */
  place(req: PlaceOrderRequest) {
    return this.http.post<OrderDto>(this.base, req);
  }

  /**
   * Retrieves a single order by its unique identifier.
   * @param id - The order ID.
   * @returns An observable that emits the {@link OrderDto}.
   */
  getById(id: string) {
    return this.http.get<OrderDto>(`${this.base}/${id}`);
  }

  /**
   * Retrieves the order history for a specific customer.
   * @param customerId - The customer's user ID.
   * @returns An observable that emits an array of {@link OrderDto}.
   */
  getHistory(customerId: string) {
    return this.http.get<OrderDto[]>(`${this.base}/customer/${customerId}`);
  }

  /**
   * Cancels an order by ID.
   * @param id - The order ID to cancel.
   * @returns An observable that emits a confirmation string.
   */
  cancel(id: string) {
    return this.http.post<string>(`${this.base}/${id}/cancel`, {});
  }

  /**
   * Retrieves all orders for a specific restaurant.
   * @param restaurantId - The restaurant ID.
   * @returns An observable that emits an array of {@link OrderDto}.
   */
  getByRestaurant(restaurantId: string) {
    return this.http.get<OrderDto[]>(`${this.base}/restaurant/${restaurantId}`);
  }

  /**
   * Updates the status of an order.
   * @param id - The order ID.
   * @param status - The new status as a numeric enum value.
   * @returns An observable that completes on success.
   */
  updateStatus(id: string, status: number) {
    // Backend expects the integer enum value directly
    return this.http.patch(`${this.base}/${id}/status`, status, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  /**
   * Retrieves all orders for admin use, optionally filtered by status.
   * @param status - Optional status string filter (e.g., "Placed", "Delivered").
   * @returns An observable that emits an array of {@link OrderDto}.
   */
  adminGetAll(status?: string) {
    const params: Record<string, string> = status ? { status } : {};
    return this.http.get<OrderDto[]>(this.adminBase, { params });
  }

  /**
   * Retrieves aggregate order statistics for the admin dashboard.
   * @returns An observable that emits {@link OrderStats}.
   */
  getStats() {
    return this.http.get<OrderStats>(`${this.adminBase}/stats`);
  }
}
