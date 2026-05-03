/** Represents a restaurant returned from the Restaurant API. */
export interface RestaurantDto {
  /** The unique identifier of the restaurant. */
  id: string;
  /** The unique identifier of the restaurant owner. */
  ownerId: string;
  /** The display name of the restaurant. */
  name: string;
  /** A description of the restaurant. */
  description: string;
  /** The physical address of the restaurant. */
  address: string;
  /** Comma-separated cuisine types offered (e.g., "Indian,Chinese"). */
  cuisineTypes: string;
  /** The operating hours description (e.g., "9 AM - 10 PM"). */
  operatingHours?: string;
  /** The average customer rating (0–5). */
  averageRating: number;
  /** The total number of customer reviews. */
  totalReviews: number;
  /** Whether the restaurant is currently open for orders. */
  isOpen: boolean;
  /** The estimated delivery time in minutes. */
  estimatedDeliveryMinutes: number;
  /** The minimum order amount required. */
  minimumOrderAmount: number;
  /** The current approval status (e.g., "Pending", "Active"). */
  status: string;
  /** The optional URL of the restaurant's logo image. */
  logoUrl?: string;
}

/** Request payload for creating a new restaurant listing. */
export interface CreateRestaurantRequest {
  /** The display name of the restaurant. */
  name: string;
  /** A description of the restaurant. */
  description: string;
  /** The physical address of the restaurant. */
  address: string;
  /** The latitude coordinate of the restaurant location. */
  lat: number;
  /** The longitude coordinate of the restaurant location. */
  lng: number;
  /** Comma-separated cuisine types (e.g., "Indian,Chinese"). */
  cuisineTypes: string;
  /** The operating hours description. */
  operatingHours: string;
  /** The minimum order amount required. */
  minimumOrderAmount: number;
  /** The estimated delivery time in minutes. */
  estimatedDeliveryMinutes: number;
  /** The optional URL of the restaurant's logo image. */
  logoUrl?: string;
}

/** Represents a menu item returned from the Restaurant API. */
export interface MenuItemDto {
  /** The unique identifier of the menu item. */
  id: string;
  /** The display name of the menu item. */
  name: string;
  /** A description of the menu item. */
  description: string;
  /** The price of the menu item. */
  price: number;
  /** Comma-separated dietary tags (e.g., "Veg,Gluten-Free"). */
  dietaryTags: string;
  /** Whether the menu item is currently available for ordering. */
  isAvailable: boolean;
  /** The unique identifier of the parent menu category. */
  categoryId: string;
  /** The optional URL of the menu item's image. */
  imageUrl?: string;
}

/** Represents a menu category with its items. */
export interface MenuCategoryDto {
  /** The unique identifier of the menu category. */
  id: string;
  /** The display name of the menu category. */
  name: string;
  /** The list of menu items within this category. */
  items: MenuItemDto[];
}

/** Represents a customer review returned from the Restaurant API. */
export interface ReviewDto {
  /** The unique identifier of the review. */
  id: string;
  /** The unique identifier of the reviewed restaurant. */
  restaurantId: string;
  /** The unique identifier of the associated order. */
  orderId: string;
  /** The unique identifier of the customer who wrote the review. */
  customerId: string;
  /** The display name of the customer. */
  customerName: string;
  /** The star rating given (1–5). */
  rating: number;
  /** The optional text content of the review. */
  reviewText?: string;
  /** The optional response from the restaurant owner. */
  ownerResponse?: string;
  /** ISO timestamp when the review was created. */
  createdAt: string;
}

/** Request payload for submitting a new customer review. */
export interface CreateReviewRequest {
  /** The unique identifier of the restaurant being reviewed. */
  restaurantId: string;
  /** The unique identifier of the order associated with this review. */
  orderId: string;
  /** The star rating (must be between 1 and 5). */
  rating: number;
  /** The optional text content of the review. */
  reviewText?: string;
}
