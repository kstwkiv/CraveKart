import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { RestaurantService } from '../../../core/services/restaurant.service';
import { RestaurantDto } from '../../../core/models/restaurant.models';
import { ToastComponent, ToastType } from '../../../shared/toast/toast';

@Component({
  selector: 'app-edit-restaurant',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ToastComponent],
  template: `
    <div class="page">
      <!-- Toast notifications -->
      <app-toast *ngIf="toast" [message]="toast.message" [type]="toast.type" (closed)="toast = null"></app-toast>

      <div class="page-header">
        <div>
          <h2>Edit Restaurant</h2>
          <p class="subtitle">Update your restaurant details. Changes go live immediately.</p>
        </div>
        <a routerLink="/owner/dashboard" class="btn-back">← Back</a>
      </div>

      <div *ngIf="loadingRestaurant" class="loading-state">
        <div class="icon">⏳</div><p>Loading restaurant...</p>
      </div>

      <div class="form-card" *ngIf="!loadingRestaurant">
        <form [formGroup]="form" (ngSubmit)="submit()">

          <div class="section-title">Basic Info</div>
          <div class="row-2">
            <div class="field">
              <label>Restaurant Name *</label>
              <input formControlName="name" placeholder="e.g. Spice Garden" />
            </div>
            <div class="field">
              <label>Cuisine Types *</label>
              <input formControlName="cuisineTypes" placeholder="e.g. Indian, Chinese" />
            </div>
          </div>

          <div class="field">
            <label>Description *</label>
            <textarea formControlName="description" rows="3" placeholder="Tell customers what makes your restaurant special..."></textarea>
          </div>

          <div class="section-title">Location</div>
          <div class="field">
            <label>Address *</label>
            <input formControlName="address" placeholder="Full address" />
          </div>

          <div class="section-title">Operations</div>
          <div class="row-2">
            <div class="field">
              <label>Operating Hours</label>
              <input formControlName="operatingHours" placeholder="e.g. 9:00 AM – 10:00 PM" />
            </div>
            <div class="field">
              <label>Estimated Delivery (mins) *</label>
              <input formControlName="estimatedDeliveryMinutes" type="number" placeholder="e.g. 30" />
            </div>
          </div>
          <div class="field half">
            <label>Minimum Order Amount (₹) *</label>
            <input formControlName="minimumOrderAmount" type="number" placeholder="e.g. 150" />
          </div>

          <div class="section-title">Logo</div>
          <div class="field">
            <label>Logo Image</label>
            <div class="upload-row">
              <input type="file" accept="image/*" (change)="onFile($event)" #fileInput style="display:none" />
              <button type="button" class="btn-upload" (click)="fileInput.click()">📁 Choose Image</button>
              <span class="file-name">{{ fileName || 'Keep existing' }}</span>
            </div>
            <div class="preview" *ngIf="previewUrl">
              <img [src]="previewUrl" alt="Logo preview" />
            </div>
          </div>

          <button type="submit" class="btn-submit" [disabled]="loading || form.invalid">
            <span *ngIf="!loading">Save Changes</span>
            <span *ngIf="loading">Saving...</span>
          </button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .page { max-width: 760px; margin: 0 auto; padding: 2rem 1.5rem; }
    .page-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 1.5rem; }
    .page-header h2 { font-size: 1.6rem; font-weight: 800; margin: 0; color: var(--text-primary); }
    .subtitle { color: var(--text-muted); font-size: 0.9rem; margin-top: 0.3rem; max-width: 500px; }
    .btn-back { padding: 0.5rem 1rem; background: var(--surface-alt); border-radius: 8px; text-decoration: none; color: var(--text-primary); font-size: 0.875rem; font-weight: 600; border: 1px solid var(--border); }
    .form-card { background: var(--surface); border-radius: var(--radius-lg); padding: 2rem; box-shadow: var(--shadow); border: 1px solid var(--border); }
    .section-title { font-size: 0.75rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.08em; color: var(--primary); margin: 1.5rem 0 1rem; padding-bottom: 0.4rem; border-bottom: 2px solid var(--surface-alt); }
    .section-title:first-child { margin-top: 0; }
    .row-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .half { max-width: 50%; }
    .field { margin-bottom: 1rem; }
    .field label { display: block; font-weight: 500; font-size: 0.875rem; margin-bottom: 0.4rem; color: var(--text-secondary); }
    .field input, .field textarea {
      width: 100%; padding: 0.7rem 1rem; border: 1.5px solid var(--border);
      border-radius: 8px; font-size: 0.9rem; background: var(--bg); color: var(--text-primary);
      transition: border-color 0.2s, box-shadow 0.2s; resize: vertical;
      &:focus { outline: none; border-color: var(--primary); box-shadow: 0 0 0 3px rgba(123,63,181,0.12); }
      &::placeholder { color: var(--text-muted); }
    }
    .upload-row { display: flex; align-items: center; gap: 0.75rem; }
    .btn-upload { padding: 0.5rem 1rem; background: var(--surface-alt); border: 1.5px solid var(--border); border-radius: 8px; cursor: pointer; font-size: 0.875rem; font-weight: 600; color: var(--text-primary); }
    .file-name { font-size: 0.85rem; color: var(--text-muted); }
    .preview { margin-top: 0.75rem; }
    .preview img { width: 100px; height: 100px; object-fit: cover; border-radius: 10px; border: 2px solid var(--border); }
    .error { display: none; } /* handled by toast */
    .success { display: none; } /* handled by toast */
    .btn-submit {
      width: 100%; padding: 0.85rem;
      background: linear-gradient(135deg, var(--primary), var(--accent));
      color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 0.95rem;
      cursor: pointer; margin-top: 1rem; transition: opacity 0.2s;
      &:hover:not(:disabled) { opacity: 0.88; }
      &:disabled { opacity: 0.55; cursor: not-allowed; }
    }
    .loading-state { text-align: center; padding: 4rem; color: var(--text-muted); .icon { font-size: 2.5rem; margin-bottom: 1rem; } }
    @media (max-width: 600px) { .row-2 { grid-template-columns: 1fr; } .half { max-width: 100%; } }
  `]
})
export class EditRestaurantComponent implements OnInit {
  form: FormGroup;
  loading = false;
  loadingRestaurant = true;
  toast: { message: string; type: ToastType } | null = null;
  fileName = '';
  previewUrl = '';
  selectedFile: File | null = null;
  restaurantId = '';
  existingLogoUrl = '';

  constructor(
    private fb: FormBuilder,
    private restaurantSvc: RestaurantService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      name:                     ['', Validators.required],
      description:              ['', Validators.required],
      cuisineTypes:             ['', Validators.required],
      address:                  ['', Validators.required],
      operatingHours:           [''],
      estimatedDeliveryMinutes: [30, [Validators.required, Validators.min(1)]],
      minimumOrderAmount:       [100, [Validators.required, Validators.min(0)]],
    });
  }

  ngOnInit() {
    this.restaurantId = this.route.snapshot.paramMap.get('id')!;
    this.restaurantSvc.getById(this.restaurantId).subscribe({
      next: (r: RestaurantDto) => {
        this.existingLogoUrl = r.logoUrl ?? '';
        this.previewUrl = r.logoUrl ?? '';
        this.form.patchValue({
          name: r.name,
          description: r.description,
          cuisineTypes: r.cuisineTypes,
          address: r.address,
          operatingHours: r.operatingHours ?? '',
          estimatedDeliveryMinutes: r.estimatedDeliveryMinutes,
          minimumOrderAmount: r.minimumOrderAmount,
        });
        this.loadingRestaurant = false;
      },
      error: () => {
        this.showToast('Could not load restaurant details.', 'error');
        this.loadingRestaurant = false;
      }
    });
  }

  showToast(message: string, type: ToastType) {
    this.toast = { message, type };
  }

  onFile(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.selectedFile = file;
    this.fileName = file.name;
    const reader = new FileReader();
    reader.onload = e => this.previewUrl = e.target?.result as string;
    reader.readAsDataURL(file);
  }

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.toast = null;

    const doUpdate = (logoUrl?: string) => {
      this.restaurantSvc.update(this.restaurantId, {
        ...this.form.value,
        logoUrl: logoUrl ?? this.existingLogoUrl,
        lat: 0,
        lng: 0,
      }).subscribe({
        next: () => {
          this.loading = false;
          this.showToast('Restaurant updated successfully!', 'success');
          setTimeout(() => this.router.navigate(['/owner/dashboard']), 2000);
        },
        error: (err) => {
          this.showToast(err.error?.message || err.error || 'Failed to update restaurant', 'error');
          this.loading = false;
        }
      });
    };

    if (this.selectedFile) {
      this.restaurantSvc.uploadImage(this.selectedFile).subscribe({
        next: ({ url }) => doUpdate(url),
        error: () => doUpdate()
      });
    } else {
      doUpdate();
    }
  }
}
