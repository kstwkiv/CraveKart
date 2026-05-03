import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { RestaurantDto, CreateRestaurantRequest, ReviewDto, CreateReviewRequest, MenuCategoryDto, MenuItemDto } from '../models/restaurant.models';

@Injectable({ providedIn: 'root' })
export class RestaurantService {
  private base      = `${environment.apiUrl}/restaurants`;
  private reviewBase = `${environment.apiUrl}/reviews`;
  private menuBase  = `${environment.apiUrl}/menu`;
  private adminBase = `${environment.apiUrl}/admin/restaurants`;
  private imageBase = `${environment.apiUrl}/image`;

  constructor(private http: HttpClient) {}

  // ── Image upload ──────────────────────────────────────────────────────────

  /**
   * Uploads an image file and returns its public URL.
   * @param file - The image file to upload.
   * @returns An observable that emits `{ url: string }`.
   */
  uploadImage(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(`${this.imageBase}/upload`, form);
  }

  // ── Restaurants ───────────────────────────────────────────────────────────

  /**
   * Retrieves all active restaurants, optionally filtered by a search term.
   * @param search - Optional search term to filter by name or cuisine type.
   * @returns An observable that emits an array of {@link RestaurantDto}.
   */
  getAll(search?: string) {
    const params: Record<string, string> = search ? { search } : {};
    return this.http.get<RestaurantDto[]>(this.base, { params });
  }

  /**
   * Retrieves all restaurants owned by the authenticated restaurant owner.
   * @returns An observable that emits an array of {@link RestaurantDto}.
   */
  getMyRestaurant() {
    return this.http.get<RestaurantDto[]>(`${this.base}/my`);
  }

  /**
   * Retrieves a single restaurant by its unique identifier.
   * @param id - The restaurant ID.
   * @returns An observable that emits the {@link RestaurantDto}.
   */
  getById(id: string) {
    return this.http.get<RestaurantDto>(`${this.base}/${id}`);
  }

  /**
   * Creates a new restaurant listing with Pending status.
   * @param req - The create restaurant request payload.
   * @returns An observable that emits the created {@link RestaurantDto}.
   */
  create(req: CreateRestaurantRequest) {
    return this.http.post<RestaurantDto>(this.base, req);
  }

  /**
   * Updates an existing restaurant's details.
   * @param id - The restaurant ID to update.
   * @param req - Partial update payload.
   * @returns An observable that emits the updated {@link RestaurantDto}.
   */
  update(id: string, req: Partial<CreateRestaurantRequest>) {
    return this.http.put<RestaurantDto>(`${this.base}/${id}`, req);
  }

  /**
   * Toggles the open/closed availability status of a restaurant.
   * @param id - The restaurant ID.
   * @returns An observable that emits the new `{ id, isOpen }` state.
   */
  toggleAvailability(id: string) {
    return this.http.patch<{ id: string; isOpen: boolean }>(`${this.base}/${id}/availability`, {});
  }

  // ── Menu ──────────────────────────────────────────────────────────────────

  /**
   * Retrieves the full menu for a restaurant, grouped by category.
   * @param restaurantId - The restaurant ID.
   * @returns An observable that emits an array of {@link MenuCategoryDto}.
   */
  getMenu(restaurantId: string) {
    return this.http.get<MenuCategoryDto[]>(`${this.menuBase}/restaurant/${restaurantId}`);
  }

  /**
   * Creates a new menu category for a restaurant.
   * @param restaurantId - The restaurant ID.
   * @param name - The category name.
   * @param sortOrder - The display sort order (default 0).
   * @returns An observable that emits the created {@link MenuCategoryDto}.
   */
  createCategory(restaurantId: string, name: string, sortOrder = 0) {
    return this.http.post<MenuCategoryDto>(
      `${this.menuBase}/restaurant/${restaurantId}/categories`,
      { name, sortOrder }
    );
  }

  /**
   * Creates a new menu item within a category.
   * @param restaurantId - The restaurant ID.
   * @param item - The menu item details.
   * @returns An observable that emits the created {@link MenuItemDto}.
   */
  createMenuItem(restaurantId: string, item: {
    categoryId: string;
    name: string;
    description: string;
    price: number;
    dietaryTags: string;
    imageUrl?: string;
  }) {
    return this.http.post<MenuItemDto>(
      `${this.menuBase}/restaurant/${restaurantId}/items`,
      item
    );
  }

  /**
   * Partially updates an existing menu item.
   * @param itemId - The menu item ID.
   * @param changes - The fields to update.
   * @returns An observable that emits the updated {@link MenuItemDto}.
   */
  updateMenuItem(itemId: string, changes: Partial<{
    name: string;
    description: string;
    price: number;
    isAvailable: boolean;
    imageUrl: string;
    dietaryTags: string;
  }>) {
    return this.http.patch<MenuItemDto>(`${this.menuBase}/items/${itemId}`, changes);
  }

  /**
   * Deletes a menu item.
   * @param itemId - The menu item ID to delete.
   * @returns An observable that completes on success.
   */
  deleteMenuItem(itemId: string) {
    return this.http.delete(`${this.menuBase}/items/${itemId}`);
  }

  // ── Reviews ───────────────────────────────────────────────────────────────

  /**
   * Retrieves all reviews for a specific restaurant.
   * @param restaurantId - The restaurant ID.
   * @returns An observable that emits an array of {@link ReviewDto}.
   */
  getReviews(restaurantId: string) {
    return this.http.get<ReviewDto[]>(`${this.reviewBase}/restaurant/${restaurantId}`);
  }

  /**
   * Submits a new customer review.
   * @param req - The create review request payload.
   * @returns An observable that emits the created {@link ReviewDto}.
   */
  createReview(req: CreateReviewRequest) {
    return this.http.post<ReviewDto>(this.reviewBase, req);
  }

  /**
   * Adds or updates the restaurant owner's response to a review.
   * @param reviewId - The review ID to respond to.
   * @param response - The owner's response text.
   * @returns An observable that emits the updated {@link ReviewDto}.
   */
  respondToReview(reviewId: string, response: string) {
    return this.http.post<ReviewDto>(`${this.reviewBase}/${reviewId}/response`, { response });
  }

  // ── Admin ─────────────────────────────────────────────────────────────────

  /**
   * Retrieves all restaurants for admin use, optionally filtered by status.
   * @param status - Optional status filter (e.g., "Pending", "Active").
   * @returns An observable that emits an array of {@link RestaurantDto}.
   */
  adminGetAll(status?: string) {
    const params: Record<string, string> = status ? { status } : {};
    return this.http.get<RestaurantDto[]>(this.adminBase, { params });
  }

  /**
   * Approves a restaurant application, setting its status to Active.
   * @param id - The restaurant ID to approve.
   * @returns An observable that emits the updated {@link RestaurantDto}.
   */
  approve(id: string) {
    return this.http.patch<RestaurantDto>(`${this.adminBase}/${id}/approve`, {});
  }

  /**
   * Rejects a restaurant application with a reason.
   * @param id - The restaurant ID to reject.
   * @param reason - The reason for rejection.
   * @returns An observable that completes on success.
   */
  reject(id: string, reason: string) {
    return this.http.patch(`${this.adminBase}/${id}/reject`, { reason });
  }

  /**
   * Suspends an active restaurant with a reason.
   * @param id - The restaurant ID to suspend.
   * @param reason - The reason for suspension.
   * @returns An observable that completes on success.
   */
  suspend(id: string, reason: string) {
    return this.http.patch(`${this.adminBase}/${id}/suspend`, { reason });
  }
}
