// 'export' — makes this interface available to other modules that import this file
// 'interface' — defines a pure TypeScript contract (shape) with no runtime code; only used for type-checking
/** Represents a single line item within an order. */
export interface OrderItemDto {
  /** The unique identifier of the menu item. */
  menuItemId: string;       // 'string' — TypeScript primitive type: a sequence of UTF-16 characters
  /** The display name of the menu item at the time of ordering. */
  menuItemName: string;     // 'string' — primitive type for text values
  /** The quantity of this item ordered. */
  quantity: number;         // 'number' — TypeScript primitive type: covers both integers and floats (IEEE 754)
  /** The unit price of the item at the time of ordering. */
  unitPrice: number;        // 'number' — used here for monetary/decimal values
  /** Optional special customization notes for this item. */
  customizations?: string;  // '?' — optional property; the value may be 'string' or 'undefined' at runtime
}

// 'export' — exposes this interface for use in services, components, and other models
// 'interface' — structural typing contract; any object matching this shape satisfies the type
/** Represents a food order returned from the Order API. */
export interface OrderDto {
  /** The unique identifier of the order. */
  id: string;                   // 'string' — unique identifier, typically a UUID
  /** The unique identifier of the customer who placed the order. */
  customerId: string;           // 'string' — foreign-key reference stored as text
  /** The unique identifier of the restaurant. */
  restaurantId: string;         // 'string' — foreign-key reference stored as text
  /** The display name of the restaurant at the time of ordering. */
  restaurantName: string;       // 'string' — snapshot of the name at order time
  /** The URL of the restaurant's logo image. */
  restaurantLogoUrl: string;    // 'string' — URL is represented as a plain string in TypeScript
  /** The full delivery address for the order. */
  deliveryAddress: string;      // 'string' — free-form text address
  /** The current status of the order (string or numeric enum value). */
  status: string;               // 'string' — could be an enum value serialised as text by the API
  /** The total amount charged including fees and tax. */
  totalAmount: number;          // 'number' — monetary value; TypeScript has no dedicated decimal type
  /** The payment method used (e.g., "UpiNow", "CashOnDelivery"). */
  paymentMethod: string;        // 'string' — enum value sent as text from the backend
  /** ISO timestamp when the order was created. */
  createdAt: string;            // 'string' — ISO 8601 date-time; kept as string to avoid timezone issues
  /** The list of items included in the order. */
  items: OrderItemDto[];        // 'Array' ([] shorthand) — ordered, indexed collection of OrderItemDto objects
}

// 'export' — makes the request shape importable by services that call the Order API
// 'interface' — defines the data contract for the HTTP request body
/** Request payload for placing a new food order. */
export interface PlaceOrderRequest {
  /** The unique identifier of the restaurant to order from. */
  restaurantId: string;         // 'string' — identifies the target restaurant
  /** The display name of the restaurant. */
  restaurantName: string;       // 'string' — denormalised snapshot stored with the order
  /** The URL of the restaurant's logo image. */
  restaurantLogoUrl: string;    // 'string' — URL reference to the logo asset
  /** The full delivery address for the order. */
  deliveryAddress: string;      // 'string' — where the order should be delivered
  /** The payment method enum value (0 = UpiNow, 1 = CashOnDelivery). */
  paymentMethod: number;        // 'number' — numeric enum value expected by the backend API
  /** The list of menu items to include in the order. */
  // Inline object type — an anonymous interface literal used directly in the array type
  items: { menuItemId: string; menuItemName: string; quantity: number; unitPrice: number; customizations?: string }[];
  // '[]' — Array shorthand; each element must match the inline object shape above
}

// 'export' — exposes the stats shape for the admin dashboard component
// 'interface' — pure type contract; no class overhead, no constructor
/** Aggregate order statistics returned from the admin stats endpoint. */
export interface OrderStats {
  /** Total number of orders across all statuses. */
  total: number;          // 'number' — integer count
  /** Number of orders in Placed status. */
  placed: number;         // 'number' — integer count per status bucket
  /** Number of orders in Confirmed status. */
  confirmed: number;      // 'number' — integer count per status bucket
  /** Number of orders in Preparing status. */
  preparing: number;      // 'number' — integer count per status bucket
  /** Number of orders in Ready status. */
  ready: number;          // 'number' — integer count per status bucket
  /** Number of orders in Delivered status. */
  delivered: number;      // 'number' — integer count per status bucket
  /** Number of orders in Cancelled status. */
  cancelled: number;      // 'number' — integer count per status bucket
  /** Total revenue from all delivered orders. */
  totalRevenue: number;   // 'number' — sum of monetary values; floating-point in JS/TS
}
