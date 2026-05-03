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

  /**
   * Retrieves the delivery record for a specific order.
   * @param orderId - The order ID.
   * @returns An observable that emits the {@link DeliveryDto}.
   */
  getByOrder(orderId: string) {
    return this.http.get<DeliveryDto>(`${this.base}/${orderId}`);
  }

  /**
   * Retrieves the active delivery assigned to the authenticated agent.
   * @returns An observable that emits the active delivery details.
   */
  getMyDelivery() {
    return this.http.get<any>(`${this.base}/my`);
  }

  /**
   * Updates the agent's own standing location (works without an active delivery).
   * @param lat - The new latitude coordinate.
   * @param lng - The new longitude coordinate.
   * @returns An observable that emits the updated {@link DeliveryAgentDto}.
   */
  updateAgentLocation(lat: number, lng: number) {
    return this.http.patch<DeliveryAgentDto>(`${this.agentBase}/me/location`, { lat, lng });
  }

  /**
   * Shares the agent's live location during an active delivery.
   * Updates the delivery record and broadcasts via SignalR.
   * @param agentId - The agent's ID (unused by the endpoint but kept for clarity).
   * @param lat - The current latitude.
   * @param lng - The current longitude.
   * @returns An observable that completes on success.
   */
  updateLocation(agentId: string, lat: number, lng: number) {
    return this.http.patch(`${this.base}/location`, { lat, lng });
  }

  /**
   * Marks a delivery as completed for the given order.
   * @param orderId - The order ID to complete.
   * @returns An observable that completes on success.
   */
  complete(orderId: string) {
    return this.http.patch(`${this.base}/${orderId}/complete`, {});
  }

  /**
   * Agent self-assigns a Ready order without admin intervention.
   * @param orderId - The order ID to pick up.
   * @returns An observable that emits the created {@link DeliveryDto}.
   */
  pickup(orderId: string) {
    return this.http.post<DeliveryDto>(`${this.base}/pickup/${orderId}`, {});
  }

  /**
   * Fetches all orders currently in Ready status for agents to pick up.
   * @returns An observable that emits an array of {@link OrderDto}.
   */
  getReadyOrders() {
    return this.http.get<OrderDto[]>(`${this.orderBase}/ready`);
  }

  // ── Agent profile ─────────────────────────────────────────────────────────

  /**
   * Registers a new delivery agent profile for the authenticated user.
   * @param vehicleType - The type of vehicle (e.g., "Bike", "Car").
   * @returns An observable that emits the created {@link DeliveryAgentDto}.
   */
  registerAgent(vehicleType: string) {
    return this.http.post<DeliveryAgentDto>(`${this.agentBase}/register`, { vehicleType });
  }

  /**
   * Retrieves the authenticated agent's own profile.
   * @returns An observable that emits the {@link DeliveryAgentDto}.
   */
  getMyProfile() {
    return this.http.get<DeliveryAgentDto>(`${this.agentBase}/me`);
  }

  /**
   * Toggles the availability status of the authenticated agent.
   * @returns An observable that emits `{ id, isAvailable }`.
   */
  toggleAvailability() {
    return this.http.patch<{ id: string; isAvailable: boolean }>(`${this.agentBase}/me/availability`, {});
  }

  /**
   * Updates the vehicle type for the authenticated agent.
   * @param vehicleType - The new vehicle type.
   * @returns An observable that emits the updated {@link DeliveryAgentDto}.
   */
  updateVehicle(vehicleType: string) {
    return this.http.patch<DeliveryAgentDto>(`${this.agentBase}/me/vehicle`, { vehicleType });
  }

  /**
   * Retrieves all registered delivery agents (admin use).
   * @returns An observable that emits an array of {@link DeliveryAgentDto}.
   */
  getAllAgents() {
    return this.http.get<DeliveryAgentDto[]>(this.agentBase);
  }
}
