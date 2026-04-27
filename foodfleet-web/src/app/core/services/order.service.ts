import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { OrderDto, PlaceOrderRequest, OrderStats } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private base = `${environment.apiUrl}/orders`;
  private adminBase = `${environment.apiUrl}/admin/orders`;

  constructor(private http: HttpClient) {}

  place(req: PlaceOrderRequest) {
    return this.http.post<OrderDto>(this.base, req);
  }

  getById(id: string) {
    return this.http.get<OrderDto>(`${this.base}/${id}`);
  }

  getHistory(customerId: string) {
    return this.http.get<OrderDto[]>(`${this.base}/customer/${customerId}`);
  }

  cancel(id: string) {
    return this.http.post<string>(`${this.base}/${id}/cancel`, {});
  }

  getByRestaurant(restaurantId: string) {
    return this.http.get<OrderDto[]>(`${this.base}/restaurant/${restaurantId}`);
  }

  updateStatus(id: string, status: number) {
    // Backend expects the integer enum value directly
    return this.http.patch(`${this.base}/${id}/status`, status, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // Admin
  adminGetAll(status?: string) {
    const params: Record<string, string> = status ? { status } : {};
    return this.http.get<OrderDto[]>(this.adminBase, { params });
  }

  getStats() {
    return this.http.get<OrderStats>(`${this.adminBase}/stats`);
  }
}
