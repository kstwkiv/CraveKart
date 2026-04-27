import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ToastType = 'success' | 'error';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast" [class.success]="type === 'success'" [class.error]="type === 'error'" [class.visible]="visible">
      <span class="toast-icon">{{ type === 'success' ? '✅' : '⚠️' }}</span>
      <span class="toast-msg">{{ message }}</span>
      <button class="toast-close" (click)="dismiss()">✕</button>
    </div>
  `,
  styles: [`
    .toast {
      position: fixed;
      top: 1.5rem;
      right: 1.5rem;
      z-index: 9999;
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.9rem 1.25rem;
      border-radius: 12px;
      min-width: 280px;
      max-width: 420px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.45);
      backdrop-filter: blur(16px);
      border: 1px solid transparent;
      font-size: 0.9rem;
      font-weight: 500;
      transform: translateX(calc(100% + 2rem));
      opacity: 0;
      transition: transform 0.35s cubic-bezier(0.34, 1.56, 0.64, 1), opacity 0.3s ease;
      pointer-events: none;
    }
    .toast.visible {
      transform: translateX(0);
      opacity: 1;
      pointer-events: all;
    }
    .toast.success {
      background: rgba(26, 144, 144, 0.18);
      border-color: rgba(26, 144, 144, 0.5);
      color: #80e8e8;
    }
    .toast.error {
      background: rgba(192, 64, 96, 0.18);
      border-color: rgba(192, 64, 96, 0.5);
      color: #f09090;
    }
    .toast-icon { font-size: 1.1rem; flex-shrink: 0; }
    .toast-msg { flex: 1; line-height: 1.4; }
    .toast-close {
      background: none;
      border: none;
      cursor: pointer;
      font-size: 0.8rem;
      opacity: 0.6;
      color: inherit;
      padding: 0.1rem 0.3rem;
      border-radius: 4px;
      flex-shrink: 0;
      transition: opacity 0.15s;
      &:hover { opacity: 1; }
    }
  `]
})
export class ToastComponent implements OnInit, OnDestroy {
  @Input() message = '';
  @Input() type: ToastType = 'success';
  @Input() duration = 4000;
  @Output() closed = new EventEmitter<void>();

  visible = false;
  private timer?: ReturnType<typeof setTimeout>;

  ngOnInit() {
    // Slight delay so the enter animation triggers after DOM insertion
    requestAnimationFrame(() => { this.visible = true; });
    if (this.duration > 0) {
      this.timer = setTimeout(() => this.dismiss(), this.duration);
    }
  }

  ngOnDestroy() {
    if (this.timer) clearTimeout(this.timer);
  }

  dismiss() {
    this.visible = false;
    // Wait for exit animation before emitting
    setTimeout(() => this.closed.emit(), 350);
  }
}
