// 'import' — ES module keyword; pulls named exports from another module into this file's scope
// 'Component' — Angular decorator factory that marks a class as an Angular component and supplies metadata
// '@angular/core' — Angular's core package; provides decorators, lifecycle hooks, and DI primitives
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
// 'Input'       — decorator that marks a class property as a data-binding input (parent → child)
// 'Output'      — decorator that marks a class property as an event-binding output (child → parent)
// 'EventEmitter'— Angular class that wraps RxJS Subject; used with @Output to emit custom DOM-like events
// 'OnInit'      — lifecycle-hook interface; requires ngOnInit() to be implemented; called after first ngOnChanges
// 'OnDestroy'   — lifecycle-hook interface; requires ngOnDestroy() to be implemented; called just before the component is destroyed
// 'CommonModule'— Angular module that provides common directives like *ngIf, *ngFor, async pipe, etc.
import { CommonModule } from '@angular/common';

// 'export' — makes this type alias available to other modules
// 'type'   — TypeScript keyword for creating a type alias; here it creates a union of two string literals
/** The visual style variant of the toast notification. */
export type ToastType = 'success' | 'error'; // union type: the value must be exactly one of these two strings

/**
 * Reusable toast notification component.
 * Slides in from the top-right corner, auto-dismisses after `duration` ms,
 * and emits a `closed` event when dismissed (either automatically or by the user).
 */
// '@Component' — class decorator; Angular reads its metadata to compile the component's template, styles, and DI
@Component({
  selector: 'app-toast',   // CSS selector used in templates to instantiate this component: <app-toast>
  standalone: true,        // standalone: true — component does not belong to an NgModule; imports its own dependencies
  imports: [CommonModule], // 'imports' — declares which Angular modules/components this standalone component depends on
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
// 'export' — makes this class importable by other Angular modules/components
// 'class'  — ES6/TypeScript keyword; defines a blueprint for objects with properties and methods
// 'implements' — TypeScript keyword; enforces that the class satisfies the listed interface contracts
// 'OnInit', 'OnDestroy' — Angular lifecycle interfaces being implemented here
export class ToastComponent implements OnInit, OnDestroy {
  /** The message text to display in the toast. */
  // '@Input()' — property decorator; Angular's change-detection will update this when the parent binding changes
  @Input() message = '';          // 'string' (inferred) — default empty string
  /** The visual style variant of the toast. */
  @Input() type: ToastType = 'success'; // 'ToastType' — the union type alias defined above
  /** Duration in milliseconds before the toast auto-dismisses. Set to 0 to disable auto-dismiss. */
  @Input() duration = 4000;       // 'number' (inferred) — milliseconds; default 4 seconds
  /** Emitted when the toast has finished its exit animation and is fully dismissed. */
  // '@Output()' — property decorator; exposes an event stream that parent components can listen to with (closed)="..."
  // 'EventEmitter<void>' — generic class; <void> means the event carries no payload
  // 'void' — TypeScript type meaning "no value"; used when a function/event returns/emits nothing meaningful
  @Output() closed = new EventEmitter<void>();

  // 'boolean' — TypeScript primitive type: true or false; controls CSS class binding in the template
  visible = false;
  // 'private' — access modifier; this property is only accessible within this class, not from outside
  // 'ReturnType<typeof setTimeout>' — TypeScript utility type that infers the return type of setTimeout
  private timer?: ReturnType<typeof setTimeout>; // '?' — optional; may be undefined if no timer is running

  // 'ngOnInit' — Angular lifecycle hook method; called once after the component's inputs are first set
  ngOnInit() {
    // Slight delay so the enter animation triggers after DOM insertion
    requestAnimationFrame(() => { this.visible = true; });
    // 'if' — conditional control-flow keyword; executes the block only when the condition is truthy
    if (this.duration > 0) {
      this.timer = setTimeout(() => this.dismiss(), this.duration);
    }
  }

  // 'ngOnDestroy' — Angular lifecycle hook method; called just before Angular destroys the component
  // Used here to cancel the timer and prevent memory leaks / callbacks on unmounted components
  ngOnDestroy() {
    // 'if' — guards against calling clearTimeout when no timer was set
    if (this.timer) clearTimeout(this.timer);
  }

  dismiss() {
    // 'false' — boolean literal; hides the toast by removing the CSS 'visible' class
    this.visible = false;
    // Wait for exit animation before emitting
    // Arrow function '() =>' — concise anonymous function; captures 'this' from the enclosing scope (lexical this)
    setTimeout(() => this.closed.emit(), 350);
    // '.emit()' — EventEmitter method; fires the output event so parent components receive the notification
  }
}
