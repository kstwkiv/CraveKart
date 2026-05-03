// 'import' — ES module keyword; pulls named exports from another module into this file's scope
// 'Component' — Angular decorator factory; marks a class as a component and attaches template/style metadata
import { Component } from '@angular/core';
// 'CommonModule' — provides *ngIf, *ngFor, async pipe, and other structural directives
import { CommonModule } from '@angular/common';
// 'FormBuilder'         — Angular service that creates FormGroup/FormControl instances with less boilerplate
// 'FormGroup'           — represents a group of form controls; tracks combined validity and value
// 'ReactiveFormsModule' — Angular module that enables reactive (model-driven) forms in templates
// 'Validators'          — collection of built-in validator functions (required, email, minLength, min, etc.)
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// 'Router'     — Angular service for programmatic navigation between routes
// 'RouterLink' — directive that turns an anchor into a client-side navigation link
import { Router, RouterLink } from '@angular/router';
// 'RestaurantService' — application service that wraps HTTP calls to the Restaurant API
import { RestaurantService } from '../../../core/services/restaurant.service';

/**
 * Create restaurant component.
 * Reactive form for restaurant owners to register a new restaurant.
 * Supports optional logo image upload before submission.
 * The restaurant is created with Pending status awaiting admin approval.
 */
// '@Component' — class decorator; Angular reads this metadata to compile the template and wire up DI
@Component({
  selector: 'app-create-restaurant',  // CSS selector used in templates/router to render this component
  standalone: true,                   // standalone: true — no NgModule needed; the component manages its own imports
  imports: [CommonModule, ReactiveFormsModule, RouterLink], // 'imports' — Angular dependencies for this component's template
  template: `
    <div class="page">
      <div class="page-header">
        <div>
          <h2>Register Your Restaurant</h2>
          <p class="subtitle">Fill in the details below — your restaurant will be reviewed by an admin before going live.</p>
        </div>
        <a routerLink="/owner/dashboard" class="btn-back">← Back</a>
      </div>

      <div class="form-card">
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
          <div class="row-2">
            <div class="field">
              <label>Latitude</label>
              <input formControlName="lat" type="number" placeholder="e.g. 17.3850" />
            </div>
            <div class="field">
              <label>Longitude</label>
              <input formControlName="lng" type="number" placeholder="e.g. 78.4867" />
            </div>
          </div>

          <div class="section-title">Operations</div>
          <div class="row-2">
            <div class="field">
              <label>Operating Hours *</label>
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

          <div class="section-title">Logo (optional)</div>
          <div class="field">
            <label>Logo Image</label>
            <div class="upload-row">
              <input type="file" accept="image/*" (change)="onFile($event)" #fileInput style="display:none" />
              <button type="button" class="btn-upload" (click)="fileInput.click()">📁 Choose Image</button>
              <span class="file-name">{{ fileName || 'No file chosen' }}</span>
            </div>
            <div class="preview" *ngIf="previewUrl">
              <img [src]="previewUrl" alt="Logo preview" />
            </div>
          </div>

          <div class="error" *ngIf="error">{{ error }}</div>

          <button type="submit" class="btn-submit" [disabled]="loading || form.invalid">
            <span *ngIf="!loading">Submit for Review →</span>
            <span *ngIf="loading">Submitting...</span>
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
    .field input, .field textarea, .field select {
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
    .error { background: #fce8ee; border: 1px solid #f0b8c8; color: var(--danger); padding: 0.65rem 0.9rem; border-radius: 6px; font-size: 0.875rem; margin-bottom: 1rem; }
    .btn-submit {
      width: 100%; padding: 0.85rem;
      background: linear-gradient(135deg, var(--primary), var(--accent));
      color: white; border: none; border-radius: 10px; font-weight: 700; font-size: 0.95rem;
      cursor: pointer; margin-top: 1rem; transition: opacity 0.2s;
      &:hover:not(:disabled) { opacity: 0.88; }
      &:disabled { opacity: 0.55; cursor: not-allowed; }
    }
    @media (max-width: 600px) { .row-2 { grid-template-columns: 1fr; } .half { max-width: 100%; } }
  `]
})
// 'export' — makes this class importable by the Angular router and other modules
// 'class'  — ES6/TypeScript keyword; defines a reusable blueprint with properties and methods
export class CreateRestaurantComponent {
  // 'FormGroup' — typed reference to the reactive form group; tracks validity and values of all child controls
  form: FormGroup;
  // 'boolean' (inferred) — primitive type; true/false flag to disable the submit button while the HTTP call is in flight
  loading = false;
  // 'string' (inferred) — holds the error message to display; empty string means no error
  error = '';
  fileName = '';    // 'string' — stores the selected file's name for display in the UI
  previewUrl = '';  // 'string' — base64 data URL for the image preview
  // 'File | null' — union type; either a File object or null when no file is selected
  // 'null' — JS/TS primitive; explicit absence of a value
  selectedFile: File | null = null;

  // 'constructor' — called by Angular's DI system when instantiating this class
  // 'private' — access modifier; these injected services are only accessible within this class
  constructor(private fb: FormBuilder, private restaurantSvc: RestaurantService, private router: Router) {
    // 'this' — refers to the current class instance
    // 'fb.group()' — FormBuilder method that creates a FormGroup from a config object
    this.form = this.fb.group({
      // 'Validators.required' — built-in validator; marks the control invalid if the value is empty
      name:                     ['', Validators.required],
      description:              ['', Validators.required],
      cuisineTypes:             ['', Validators.required],
      address:                  ['', Validators.required],
      lat:                      [0],  // 'number' default — latitude; no validator since it's optional
      lng:                      [0],  // 'number' default — longitude; no validator since it's optional
      operatingHours:           ['', Validators.required],
      // 'Validators.min(1)' — built-in validator; marks the control invalid if the value is less than 1
      estimatedDeliveryMinutes: [30, [Validators.required, Validators.min(1)]],
      // 'Validators.min(0)' — built-in validator; marks the control invalid if the value is negative
      minimumOrderAmount:       [100, [Validators.required, Validators.min(0)]],
    });
  }

  onFile(event: Event) { // 'Event' — DOM Event type; the base interface for all browser events
    // 'as HTMLInputElement' — type assertion; tells TypeScript the event target is an input element
    // '?.' — optional chaining; safely accesses files[0] without throwing if files is null
    const file = (event.target as HTMLInputElement).files?.[0];
    // 'if' — guards against processing when no file was selected
    // 'return' — exits the function early when the guard condition is met
    if (!file) return;
    this.selectedFile = file;
    this.fileName = file.name; // 'string' — the file's name property from the File API
    // FileReader — Web API class that reads file contents asynchronously
    const reader = new FileReader();
    // Arrow function — callback invoked when the file has been read; 'e' is the ProgressEvent
    reader.onload = e => this.previewUrl = e.target?.result as string; // 'as string' — type assertion for the base64 result
    reader.readAsDataURL(file); // readAsDataURL — encodes the file as a base64 data URL string
  }

  submit() {
    // 'if' — short-circuits the function when the form is invalid
    // 'return' — exits the function immediately; no HTTP call is made for an invalid form
    if (this.form.invalid) return;
    this.loading = true;
    this.error = '';

    // Inner function — encapsulates the create call; called after optional image upload
    // 'logoUrl?' — optional parameter; may be 'string' or 'undefined'
    // 'void' — return type annotation; this function does not return a meaningful value
    const doCreate = (logoUrl?: string) => {
      // Spread operator '...' — merges form values with the optional logoUrl into a single object
      // 'subscribe' — activates the Observable; triggers the HTTP POST call to create the restaurant
      this.restaurantSvc.create({ ...this.form.value, logoUrl }).subscribe({
        next: () => this.router.navigate(['/owner/dashboard']),
        error: (err) => {
          // '||' — logical OR; falls back through error message options
          this.error = err.error?.message || err.error || 'Failed to register restaurant';
          this.loading = false;
        }
      });
    };

    // 'if' — checks whether an image file was selected before attempting upload
    if (this.selectedFile) {
      // 'subscribe' — activates the Observable; triggers the HTTP POST call to upload the image
      this.restaurantSvc.uploadImage(this.selectedFile).subscribe({
        next: ({ url }) => doCreate(url),
        error: () => doCreate()   // proceed without image if upload fails
      });
    } else {
      doCreate();
    }
  }
}
