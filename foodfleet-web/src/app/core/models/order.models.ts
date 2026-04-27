export interface OrderItemDto {
  menuItemId: string;
  menuItemName: string;
  quantity: number;
  unitPrice: number;
  customizations?: string;
}

export interface OrderDto {
  id: string;
  customerId: string;
  restaurantId: string;
  restaurantName: string;
  restaurantLogoUrl: string;
  deliveryAddress: string;
  status: string;
  totalAmount: number;
  paymentMethod: string;
  createdAt: string;
  items: OrderItemDto[];
}

export interface PlaceOrderRequest {
  restaurantId: string;
  restaurantName: string;
  restaurantLogoUrl: string;
  deliveryAddress: string;
  paymentMethod: number;
  items: { menuItemId: string; menuItemName: string; quantity: number; unitPrice: number; customizations?: string }[];
}

export interface OrderStats {
  total: number;
  placed: number;
  confirmed: number;
  preparing: number;
  ready: number;
  delivered: number;
  cancelled: number;
  totalRevenue: number;
}
