import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { DeliveryDto, DeliveryAgentDto } from '../models/delivery.models';
import { OrderDto } from '../models/order.models';

@Injectable({ providedIn: 'root' })
export class DeliveryService {
  private base = `${environment.apiUrl}/delivery`;
  private agentBase = `${environment.apiUrl}/agents`;
  private orderBase = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  getByOrder(orderId: string) {
    return this.http.get<DeliveryDto>(`${this.base}/${orderId}`);
  }

  getMyDelivery() {
    return this.http.get<any>(`${this.base}/my`);
  }

  /** Update agent's own standing location (works without an active delivery) */
  updateAgentLocation(lat: number, lng: number) {
    return this.http.patch<DeliveryAgentDto>(`${this.agentBase}/me/location`, { lat, lng });
  }

  /** Share live location during an active delivery (updates the delivery record + SignalR) */
  updateLocation(agentId: string, lat: number, lng: number) {
    return this.http.patch(`${this.base}/location`, { lat, lng });
  }

  complete(orderId: string) {
    return this.http.patch(`${this.base}/${orderId}/complete`, {});
  }

  /** Agent self-assigns a Ready order */
  pickup(orderId: string) {
    return this.http.post<DeliveryDto>(`${this.base}/pickup/${orderId}`, {});
  }

  /** Fetch all orders currently in Ready status */
  getReadyOrders() {
    return this.http.get<OrderDto[]>(`${this.orderBase}/ready`);
  }

  // Agent profile
  registerAgent(vehicleType: string) {
    return this.http.post<DeliveryAgentDto>(`${this.agentBase}/register`, { vehicleType });
  }

  getMyProfile() {
    return this.http.get<DeliveryAgentDto>(`${this.agentBase}/me`);
  }

  toggleAvailability() {
    return this.http.patch<{ id: string; isAvailable: boolean }>(`${this.agentBase}/me/availability`, {});
  }

  updateVehicle(vehicleType: string) {
    return this.http.patch<DeliveryAgentDto>(`${this.agentBase}/me/vehicle`, { vehicleType });
  }

  getAllAgents() {
    return this.http.get<DeliveryAgentDto[]>(this.agentBase);
  }
}
