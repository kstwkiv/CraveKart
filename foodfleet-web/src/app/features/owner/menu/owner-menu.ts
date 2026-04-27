import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { MenuCategoryDto, MenuItemDto } from '../../../core/models/restaurant.models';

@Component({
  selector: 'app-owner-menu',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h2>Menu Management</h2>
          <p class="subtitle">Add categories and dishes to your restaurant</p>
        </div>
        <a routerLink="/owner/dashboard" class="btn-back">← Dashboard</a>
      </div>

      <!-- Add Category -->
      <div class="card">
        <h3>Add Category</h3>
        <div class="inline-form">
          <input [(ngModel)]="newCategoryName" placeholder="e.g. Starters, Main Course, Desserts" />
          <button class="btn-primary" (click)="addCategory()" [disabled]="!newCategoryName.trim()">+ Add</button>
        </div>
      </div>

      <!-- Categories & Items -->
      <div *ngFor="let cat of categories" class="category-block">
        <div class="category-header">
          <h3>{{ cat.name }}</h3>
          <span class="item-count">{{ cat.items.length }} items</span>
        </div>

        <!-- Existing items -->
        <div *ngFor="let item of cat.items" class="item-row-wrap">
          <div class="item-row">
            <img *ngIf="item.imageUrl" [src]="item.imageUrl" class="item-img" />
            <div class="item-img placeholder" *ngIf="!item.imageUrl">🍽️</div>
            <div class="item-info">
              <div class="item-name">{{ item.name }}</div>
              <div class="item-desc">{{ item.description }}</div>
              <div class="item-tags" *ngIf="item.dietaryTags">🏷️ {{ item.dietaryTags }}</div>
            </div>
            <div class="item-price">₹{{ item.price }}</div>
            <div class="item-actions">
              <button class="btn-toggle-avail" (click)="toggleAvail(item)"
                [class.unavail]="!item.isAvailable">
                {{ item.isAvailable ? 'Available' : 'Unavailable' }}
              </button>
              <button class="btn-edit-item" (click)="startEdit(item)" title="Edit item">✏️</button>
              <button class="btn-delete" (click)="deleteItem(item, cat)" title="Delete item">🗑️</button>
            </div>
          </div>

          <!-- Inline edit form -->
          <div class="edit-item-form" *ngIf="editingItemId === item.id">
            <div class="edit-form-title">✏️ Edit Item</div>
            <div class="form-grid">
              <input [(ngModel)]="editItem.name" placeholder="Item name *" />
              <input [(ngModel)]="editItem.description" placeholder="Description" />
              <input [(ngModel)]="editItem.price" type="number" placeholder="Price (₹) *" />
              <input [(ngModel)]="editItem.dietaryTags" placeholder="Tags (Veg, Spicy, etc.)" />
            </div>

            <!-- Image picker -->
            <div class="image-picker">
              <label class="image-picker-label">
                <span class="picker-icon">📁</span>
                <span>{{ editItem.imageUrl ? 'Change Image' : 'Choose Image' }}</span>
                <input type="file" accept="image/*" (change)="onEditImageSelected($event)" hidden />
              </label>
              <div class="image-preview" *ngIf="editItem.imageUrl">
                <img [src]="editItem.imageUrl" alt="Preview" />
                <button class="btn-remove-img" (click)="editItem.imageUrl = ''" title="Remove image">✕</button>
              </div>
              <span class="picker-hint" *ngIf="!editItem.imageUrl">No image selected</span>
            </div>

            <div class="form-actions">
              <button class="btn-primary" (click)="saveEdit(item)"
                [disabled]="!editItem.name.trim() || !editItem.price || savingEdit">
                {{ savingEdit ? 'Saving...' : 'Save Changes' }}
              </button>
              <button class="btn-cancel" (click)="cancelEdit()">Cancel</button>
            </div>
          </div>
        </div>

        <!-- Add item form -->
        <div class="add-item-form" *ngIf="addingTo === cat.id; else addBtn">
          <div class="form-grid">
            <input [(ngModel)]="newItem.name" placeholder="Item name *" />
            <input [(ngModel)]="newItem.description" placeholder="Description" />
            <input [(ngModel)]="newItem.price" type="number" placeholder="Price (₹) *" />
            <input [(ngModel)]="newItem.dietaryTags" placeholder="Tags (Veg, Spicy, etc.)" />
          </div>

          <!-- Image picker -->
          <div class="image-picker">
            <label class="image-picker-label">
              <span class="picker-icon">📁</span>
              <span>{{ newItem.imageUrl ? 'Change Image' : 'Choose Image from Folder' }}</span>
              <input type="file" accept="image/*" (change)="onImageSelected($event)" hidden />
            </label>
            <div class="image-preview" *ngIf="newItem.imageUrl">
              <img [src]="newItem.imageUrl" alt="Preview" />
              <button class="btn-remove-img" (click)="newItem.imageUrl = ''" title="Remove image">✕</button>
            </div>
            <span class="picker-hint" *ngIf="!newItem.imageUrl">No image selected</span>
          </div>

          <div class="form-actions">
            <button class="btn-primary" (click)="addItem(cat)" [disabled]="!newItem.name || !newItem.price">Save Item</button>
            <button class="btn-cancel" (click)="addingTo = null">Cancel</button>
          </div>
        </div>
        <ng-template #addBtn>
          <button class="btn-add-item" (click)="startAddItem(cat.id)">+ Add Item</button>
        </ng-template>
      </div>

      <div *ngIf="categories.length === 0" class="empty">
        No categories yet — add one above to get started.
      </div>
    </div>
  `,
  styles: [`
    .page { max-width: 900px; margin: 0 auto; padding: 2rem 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
    .page-header h2 { font-size: 1.6rem; font-weight: 800; margin: 0; color: var(--text-primary); }
    .subtitle { color: var(--text-muted); font-size: 0.9rem; margin-top: 0.25rem; }
    .btn-back { padding: 0.5rem 1rem; background: var(--surface-alt); border: 1px solid var(--border); border-radius: 8px; text-decoration: none; color: var(--text-secondary); font-size: 0.875rem; font-weight: 600; transition: background 0.15s; }
    .btn-back:hover { background: var(--border); }
    .card { background: var(--surface); border: 1px solid var(--border); border-radius: 12px; padding: 1.25rem 1.5rem; box-shadow: var(--shadow); margin-bottom: 1.5rem; }
    .card h3 { margin: 0 0 1rem; font-size: 1rem; font-weight: 700; color: var(--text-primary); }
    .inline-form { display: flex; gap: 0.75rem; }
    .inline-form input { flex: 1; padding: 0.6rem 0.875rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.9rem; background: var(--bg); color: var(--text-primary); font-family: inherit; }
    .inline-form input:focus { outline: none; border-color: var(--primary); box-shadow: 0 0 0 3px rgba(123,63,181,0.12); }
    .inline-form input::placeholder { color: var(--text-muted); }
    .btn-primary { padding: 0.6rem 1.25rem; background: var(--primary); color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.875rem; font-family: inherit; transition: opacity 0.15s; }
    .btn-primary:hover:not(:disabled) { opacity: 0.88; }
    .btn-primary:disabled { opacity: 0.45; cursor: not-allowed; }
    .category-block { background: var(--surface); border: 1px solid var(--border); border-radius: 12px; padding: 1.25rem 1.5rem; box-shadow: var(--shadow); margin-bottom: 1.25rem; }
    .category-header { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 1rem; }
    .category-header h3 { margin: 0; font-size: 1.05rem; font-weight: 700; color: var(--text-primary); }
    .item-count { background: var(--surface-alt); color: var(--text-muted); padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600; border: 1px solid var(--border); }
    .item-row { display: flex; align-items: center; gap: 0.875rem; padding: 0.75rem 0; border-top: 1px solid var(--border); }
    .item-img { width: 48px; height: 48px; border-radius: 8px; object-fit: cover; }
    .item-img.placeholder { width: 48px; height: 48px; border-radius: 8px; background: var(--surface-alt); border: 1px solid var(--border); display: flex; align-items: center; justify-content: center; font-size: 1.25rem; }
    .item-info { flex: 1; }
    .item-name { font-weight: 600; font-size: 0.9rem; color: var(--text-primary); }
    .item-desc { color: var(--text-muted); font-size: 0.8rem; }
    .item-tags { color: var(--text-muted); font-size: 0.75rem; margin-top: 0.2rem; }
    .item-price { font-weight: 700; font-size: 0.95rem; min-width: 60px; text-align: right; color: var(--accent); }
    .item-actions { display: flex; gap: 0.4rem; align-items: center; }
    .btn-toggle-avail { padding: 0.25rem 0.6rem; border: none; border-radius: 6px; cursor: pointer; font-size: 0.75rem; font-weight: 600; background: rgba(26,144,144,0.15); color: var(--accent); font-family: inherit; }
    .btn-toggle-avail.unavail { background: rgba(192,64,96,0.15); color: var(--danger); }
    .btn-delete { background: none; border: none; cursor: pointer; font-size: 1rem; opacity: 0.4; transition: opacity 0.15s; }
    .btn-delete:hover { opacity: 1; }
    .add-item-form { margin-top: 1rem; padding: 1rem; background: var(--surface-alt); border: 1px solid var(--border); border-radius: 8px; }
    .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 0.6rem; margin-bottom: 0.75rem; }
    .form-grid input { padding: 0.55rem 0.75rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.875rem; background: var(--bg); color: var(--text-primary); font-family: inherit; }
    .form-grid input:focus { outline: none; border-color: var(--primary); }
    .form-grid input::placeholder { color: var(--text-muted); }
    .form-actions { display: flex; gap: 0.5rem; }
    .btn-cancel { padding: 0.6rem 1rem; background: var(--surface-alt); color: var(--text-secondary); border: 1px solid var(--border); border-radius: 8px; cursor: pointer; font-weight: 600; font-size: 0.875rem; font-family: inherit; transition: background 0.15s; }
    .btn-cancel:hover { background: var(--border); }
    .btn-add-item { margin-top: 0.75rem; padding: 0.5rem 1rem; background: var(--surface-alt); color: var(--text-secondary); border: 1.5px dashed var(--border); border-radius: 8px; cursor: pointer; font-size: 0.85rem; font-weight: 600; width: 100%; font-family: inherit; transition: all 0.15s; }
    .btn-add-item:hover { border-color: var(--primary); color: var(--primary); background: rgba(123,63,181,0.05); }
    .empty { text-align: center; color: var(--text-muted); padding: 3rem; background: var(--surface); border: 1px solid var(--border); border-radius: 12px; }
    .image-picker { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; flex-wrap: wrap; }
    .image-picker-label { display: inline-flex; align-items: center; gap: 0.4rem; padding: 0.5rem 1rem; background: var(--surface); border: 1.5px dashed var(--border); border-radius: 8px; cursor: pointer; font-size: 0.85rem; font-weight: 600; color: var(--text-secondary); font-family: inherit; transition: all 0.15s; }
    .image-picker-label:hover { border-color: var(--primary); color: var(--primary); background: rgba(123,63,181,0.05); }
    .picker-icon { font-size: 1rem; }
    .picker-hint { font-size: 0.8rem; color: var(--text-muted); }
    .image-preview { position: relative; display: inline-flex; }
    .image-preview img { width: 64px; height: 64px; border-radius: 8px; object-fit: cover; border: 1.5px solid var(--border); }
    .btn-remove-img { position: absolute; top: -6px; right: -6px; width: 18px; height: 18px; border-radius: 50%; background: var(--danger, #c04060); color: white; border: none; cursor: pointer; font-size: 0.65rem; display: flex; align-items: center; justify-content: center; line-height: 1; padding: 0; }
    /* ── Edit item ── */
    .item-row-wrap { border-top: 1px solid var(--border); }
    .item-row-wrap .item-row { border-top: none; }
    .btn-edit-item { background: rgba(45,106,79,0.1); border: 1px solid rgba(45,106,79,0.25); color: var(--primary); border-radius: 6px; padding: 0.25rem 0.55rem; cursor: pointer; font-size: 0.8rem; font-weight: 600; transition: all 0.15s; }
    .btn-edit-item:hover { background: rgba(45,106,79,0.2); }
    .edit-item-form { margin: 0 0 0.75rem; padding: 1rem 1rem 1rem; background: #f0f7f2; border: 1.5px solid var(--primary); border-radius: 0 0 10px 10px; animation: slideDown 0.15s ease; }
    .edit-form-title { font-size: 0.82rem; font-weight: 700; color: var(--primary); text-transform: uppercase; letter-spacing: 0.05em; margin-bottom: 0.75rem; }
    @keyframes slideDown { from { opacity: 0; transform: translateY(-6px); } to { opacity: 1; transform: translateY(0); } }
  `]
})
export class OwnerMenuComponent implements OnInit {
  restaurantId = '';
  categories: MenuCategoryDto[] = [];
  newCategoryName = '';
  addingTo: string | null = null;
  newItem = { name: '', description: '', price: 0, dietaryTags: '', imageUrl: '' };

  // Edit state
  editingItemId: string | null = null;
  editItem = { name: '', description: '', price: 0, dietaryTags: '', imageUrl: '' };
  savingEdit = false;

  constructor(private route: ActivatedRoute, private restaurantSvc: RestaurantService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.restaurantId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
  }

  load() {
    this.restaurantSvc.getMenu(this.restaurantId).subscribe(c => { this.categories = c; this.cdr.markForCheck(); });
  }

  addCategory() {
    if (!this.newCategoryName.trim()) return;
    this.restaurantSvc.createCategory(this.restaurantId, this.newCategoryName.trim()).subscribe(cat => {
      this.categories.push(cat);
      this.newCategoryName = '';
      this.cdr.markForCheck();
    });
  }

  startAddItem(categoryId: string) {
    this.addingTo = categoryId;
    this.newItem = { name: '', description: '', price: 0, dietaryTags: '', imageUrl: '' };
  }

  addItem(cat: MenuCategoryDto) {
    this.restaurantSvc.createMenuItem(this.restaurantId, {
      categoryId: cat.id,
      name: this.newItem.name,
      description: this.newItem.description,
      price: this.newItem.price,
      dietaryTags: this.newItem.dietaryTags,
      imageUrl: this.newItem.imageUrl || undefined
    }).subscribe(item => {
      cat.items.push(item);
      this.addingTo = null;
      this.cdr.markForCheck();
    });
  }

  toggleAvail(item: MenuItemDto) {
    this.restaurantSvc.updateMenuItem(item.id, { isAvailable: !item.isAvailable }).subscribe(updated => {
      item.isAvailable = updated.isAvailable;
      this.cdr.markForCheck();
    });
  }

  deleteItem(item: MenuItemDto, cat: MenuCategoryDto) {
    if (!confirm(`Delete "${item.name}"?`)) return;
    this.restaurantSvc.deleteMenuItem(item.id).subscribe(() => {
      cat.items = cat.items.filter(i => i.id !== item.id);
      this.cdr.markForCheck();
    });
  }

  startEdit(item: MenuItemDto) {
    // Close add form if open
    this.addingTo = null;
    // Toggle: clicking edit on the same item closes it
    if (this.editingItemId === item.id) { this.cancelEdit(); return; }
    this.editingItemId = item.id;
    this.editItem = {
      name: item.name,
      description: item.description ?? '',
      price: item.price,
      dietaryTags: item.dietaryTags ?? '',
      imageUrl: item.imageUrl ?? ''
    };
    this.savingEdit = false;
  }

  cancelEdit() {
    this.editingItemId = null;
    this.savingEdit = false;
  }

  saveEdit(item: MenuItemDto) {
    if (!this.editItem.name.trim() || !this.editItem.price) return;
    this.savingEdit = true;
    this.restaurantSvc.updateMenuItem(item.id, {
      name: this.editItem.name.trim(),
      description: this.editItem.description.trim(),
      price: Number(this.editItem.price),
      dietaryTags: this.editItem.dietaryTags.trim(),
      imageUrl: this.editItem.imageUrl || undefined
    }).subscribe({
      next: updated => {
        item.name        = updated.name;
        item.description = updated.description;
        item.price       = updated.price;
        item.dietaryTags = updated.dietaryTags;
        item.imageUrl    = updated.imageUrl;
        this.cancelEdit();
        this.cdr.markForCheck();
      },
      error: () => { this.savingEdit = false; }
    });
  }

  onEditImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      this.editItem.imageUrl = reader.result as string;
      this.cdr.markForCheck();
    };
    reader.readAsDataURL(file);
  }

  onImageSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      this.newItem.imageUrl = reader.result as string;
      this.cdr.markForCheck();
    };
    reader.readAsDataURL(file);
  }
}
