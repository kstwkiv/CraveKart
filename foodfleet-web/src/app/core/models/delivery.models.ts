export interface DeliveryDto {
  id: string;
  orderId: string;
  agentId: string;
  status: string;
  currentLat?: number;
  currentLng?: number;
  assignedAt: string;
  completedAt?: string;
}

export interface DeliveryAgentDto {
  id: string;
  userId: string;
  fullName: string;
  vehicleType: string;
  isAvailable: boolean;
  totalDeliveries: number;
  totalEarnings: number;
  currentLat?: number;
  currentLng?: number;
  createdAt: string;
}
