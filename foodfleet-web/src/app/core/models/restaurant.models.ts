export interface RestaurantDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  address: string;
  cuisineTypes: string;
  operatingHours?: string;
  averageRating: number;
  totalReviews: number;
  isOpen: boolean;
  estimatedDeliveryMinutes: number;
  minimumOrderAmount: number;
  status: string;
  logoUrl?: string;
}

export interface CreateRestaurantRequest {
  name: string;
  description: string;
  address: string;
  lat: number;
  lng: number;
  cuisineTypes: string;
  operatingHours: string;
  minimumOrderAmount: number;
  estimatedDeliveryMinutes: number;
  logoUrl?: string;
}

export interface MenuItemDto {
  id: string;
  name: string;
  description: string;
  price: number;
  dietaryTags: string;
  isAvailable: boolean;
  categoryId: string;
  imageUrl?: string;
}

export interface MenuCategoryDto {
  id: string;
  name: string;
  items: MenuItemDto[];
}

export interface ReviewDto {
  id: string;
  restaurantId: string;
  orderId: string;
  customerId: string;
  customerName: string;
  rating: number;
  reviewText?: string;
  ownerResponse?: string;
  createdAt: string;
}

export interface CreateReviewRequest {
  restaurantId: string;
  orderId: string;
  rating: number;
  reviewText?: string;
}
