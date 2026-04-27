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
  uploadImage(file: File) {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<{ url: string }>(`${this.imageBase}/upload`, form);
  }

  // ── Restaurants ───────────────────────────────────────────────────────────
  getAll(search?: string) {
    const params: Record<string, string> = search ? { search } : {};
    return this.http.get<RestaurantDto[]>(this.base, { params });
  }

  getMyRestaurant() {
    return this.http.get<RestaurantDto[]>(`${this.base}/my`);
  }

  getById(id: string) {
    return this.http.get<RestaurantDto>(`${this.base}/${id}`);
  }

  create(req: CreateRestaurantRequest) {
    return this.http.post<RestaurantDto>(this.base, req);
  }

  update(id: string, req: Partial<CreateRestaurantRequest>) {
    return this.http.put<RestaurantDto>(`${this.base}/${id}`, req);
  }

  toggleAvailability(id: string) {
    return this.http.patch<{ id: string; isOpen: boolean }>(`${this.base}/${id}/availability`, {});
  }

  // ── Menu ──────────────────────────────────────────────────────────────────
  getMenu(restaurantId: string) {
    return this.http.get<MenuCategoryDto[]>(`${this.menuBase}/restaurant/${restaurantId}`);
  }

  createCategory(restaurantId: string, name: string, sortOrder = 0) {
    return this.http.post<MenuCategoryDto>(
      `${this.menuBase}/restaurant/${restaurantId}/categories`,
      { name, sortOrder }
    );
  }

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

  deleteMenuItem(itemId: string) {
    return this.http.delete(`${this.menuBase}/items/${itemId}`);
  }

  // ── Reviews ───────────────────────────────────────────────────────────────
  getReviews(restaurantId: string) {
    return this.http.get<ReviewDto[]>(`${this.reviewBase}/restaurant/${restaurantId}`);
  }

  createReview(req: CreateReviewRequest) {
    return this.http.post<ReviewDto>(this.reviewBase, req);
  }

  respondToReview(reviewId: string, response: string) {
    return this.http.post<ReviewDto>(`${this.reviewBase}/${reviewId}/response`, { response });
  }

  // ── Admin ─────────────────────────────────────────────────────────────────
  adminGetAll(status?: string) {
    const params: Record<string, string> = status ? { status } : {};
    return this.http.get<RestaurantDto[]>(this.adminBase, { params });
  }

  approve(id: string) {
    return this.http.patch<RestaurantDto>(`${this.adminBase}/${id}/approve`, {});
  }

  reject(id: string, reason: string) {
    return this.http.patch(`${this.adminBase}/${id}/reject`, { reason });
  }

  suspend(id: string, reason: string) {
    return this.http.patch(`${this.adminBase}/${id}/suspend`, { reason });
  }
}
