import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

export interface SavedAddress {
  id: string;
  text: string;
  isDefault: boolean;
}

@Injectable({ providedIn: 'root' })
export class AddressService {
  constructor(private auth: AuthService) {}

  private key(): string {
    const uid = this.auth.currentUser()?.id ?? 'guest';
    return `addresses_${uid}`;
  }

  getAll(): SavedAddress[] {
    try {
      return JSON.parse(localStorage.getItem(this.key()) ?? '[]');
    } catch {
      return [];
    }
  }

  getDefault(): string {
    return this.getAll().find(a => a.isDefault)?.text ?? '';
  }

  /** Save an address after a successful order. Sets it as default automatically. */
  saveUsed(text: string): void {
    const trimmed = text.trim();
    if (!trimmed) return;

    const list = this.getAll();
    const existing = list.find(a => a.text.toLowerCase() === trimmed.toLowerCase());

    if (existing) {
      // Already saved — just promote it to default
      list.forEach(a => (a.isDefault = a.id === existing.id));
    } else {
      // New address — add and set as default
      list.forEach(a => (a.isDefault = false));
      list.unshift({ id: crypto.randomUUID(), text: trimmed, isDefault: true });
    }

    // Keep at most 5 saved addresses
    localStorage.setItem(this.key(), JSON.stringify(list.slice(0, 5)));
  }

  setDefault(id: string): void {
    const list = this.getAll().map(a => ({ ...a, isDefault: a.id === id }));
    localStorage.setItem(this.key(), JSON.stringify(list));
  }

  remove(id: string): void {
    const list = this.getAll().filter(a => a.id !== id);
    // If we removed the default, promote the first remaining one
    if (list.length > 0 && !list.some(a => a.isDefault)) {
      list[0].isDefault = true;
    }
    localStorage.setItem(this.key(), JSON.stringify(list));
  }
}
