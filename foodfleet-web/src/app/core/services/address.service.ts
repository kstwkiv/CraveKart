import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

/** Represents a saved delivery address stored in localStorage for the current user. */
export interface SavedAddress {
  /** A unique identifier for this saved address entry. */
  id: string;
  /** The full address text. */
  text: string;
  /** Whether this is the user's default delivery address. */
  isDefault: boolean;
}

@Injectable({ providedIn: 'root' })
export class AddressService {
  constructor(private auth: AuthService) {}

  /** Returns the localStorage key scoped to the current user's ID. */
  private key(): string {
    const uid = this.auth.currentUser()?.id ?? 'guest';
    return `addresses_${uid}`;
  }

  /**
   * Returns all saved addresses for the current user.
   * @returns An array of {@link SavedAddress} objects.
   */
  getAll(): SavedAddress[] {
    try {
      return JSON.parse(localStorage.getItem(this.key()) ?? '[]');
    } catch {
      return [];
    }
  }

  /**
   * Returns the text of the current user's default address, or an empty string.
   */
  getDefault(): string {
    return this.getAll().find(a => a.isDefault)?.text ?? '';
  }

  /**
   * Saves an address after a successful order and sets it as the default.
   * If the address already exists it is promoted to default; otherwise it is
   * prepended to the list. The list is capped at 5 entries.
   * @param text - The full address text to save.
   */
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

  /**
   * Sets the specified address as the default.
   * @param id - The ID of the address to promote.
   */
  setDefault(id: string): void {
    const list = this.getAll().map(a => ({ ...a, isDefault: a.id === id }));
    localStorage.setItem(this.key(), JSON.stringify(list));
  }

  /**
   * Removes a saved address by ID. If the removed address was the default,
   * the first remaining address is promoted to default.
   * @param id - The ID of the address to remove.
   */
  remove(id: string): void {
    const list = this.getAll().filter(a => a.id !== id);
    // If we removed the default, promote the first remaining one
    if (list.length > 0 && !list.some(a => a.isDefault)) {
      list[0].isDefault = true;
    }
    localStorage.setItem(this.key(), JSON.stringify(list));
  }
}
