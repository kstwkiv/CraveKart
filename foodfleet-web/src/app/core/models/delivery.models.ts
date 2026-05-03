/** Represents a delivery record returned from the Delivery API. */
export interface DeliveryDto {
  /** The unique identifier of the delivery record. */
  id: string;
  /** The unique identifier of the associated order. */
  orderId: string;
  /** The unique identifier of the assigned delivery agent. */
  agentId: string;
  /** The current delivery status (e.g., "Assigned", "Delivered"). */
  status: string;
  /** The current latitude of the delivery agent. Undefined if not yet updated. */
  currentLat?: number;
  /** The current longitude of the delivery agent. Undefined if not yet updated. */
  currentLng?: number;
  /** ISO timestamp when the delivery was assigned. */
  assignedAt: string;
  /** ISO timestamp when the delivery was completed. Undefined if still in progress. */
  completedAt?: string;
}

/** Represents a delivery agent profile returned from the Delivery API. */
export interface DeliveryAgentDto {
  /** The unique identifier of the agent profile. */
  id: string;
  /** The user account ID linked to this agent profile. */
  userId: string;
  /** The full name of the delivery agent. */
  fullName: string;
  /** The type of vehicle used by the agent (e.g., "Bike", "Car"). */
  vehicleType: string;
  /** Whether the agent is currently available for new deliveries. */
  isAvailable: boolean;
  /** The total number of deliveries completed by this agent. */
  totalDeliveries: number;
  /** The total earnings accumulated by this agent. */
  totalEarnings: number;
  /** The current latitude of the agent's standing location. */
  currentLat?: number;
  /** The current longitude of the agent's standing location. */
  currentLng?: number;
  /** ISO timestamp when the agent profile was created. */
  createdAt: string;
}
