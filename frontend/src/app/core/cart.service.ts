import { Injectable, computed, effect, inject, signal } from '@angular/core';
import { Api } from './api.service';
import { AuthService } from './auth.service';
import { Cart } from './models';
import { ToastService } from './toast.service';

const EMPTY: Cart = { items: [], savedForLater: [], wishlistProductIds: [], subtotal: 0 };

@Injectable({ providedIn: 'root' })
export class CartService {
  private api = inject(Api);
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  readonly cart = signal<Cart>(EMPTY);
  readonly count = computed(() => this.cart().items.reduce((n, i) => n + i.quantity, 0));
  readonly wishlistCount = computed(() => this.cart().wishlistProductIds.length);

  constructor() {
    effect(() => {
      if (this.auth.isLoggedIn()) {
        this.refresh();
      } else {
        this.cart.set(EMPTY);
      }
    });
  }

  refresh() {
    this.api.cart().subscribe({ next: c => this.cart.set(c), error: () => { /* ignore */ } });
  }

  isWishlisted(productId: string) { return this.cart().wishlistProductIds.includes(productId); }

  private apply(obs: ReturnType<Api['cart']>, okMessage?: string) {
    obs.subscribe({
      next: c => { this.cart.set(c); if (okMessage) this.toast.ok(okMessage); },
      error: e => this.toast.fromError(e)
    });
  }

  add(productId: string, size: string | null, quantity = 1) { this.apply(this.api.addToCart(productId, size, quantity), 'Added to cart'); }
  setQuantity(productId: string, size: string | null | undefined, quantity: number) { this.apply(this.api.updateCartItem(productId, size, quantity)); }
  remove(productId: string, size?: string | null) { this.apply(this.api.removeCartItem(productId, size), 'Removed from cart'); }
  saveForLater(productId: string, size?: string | null) { this.apply(this.api.saveForLater(productId, size), 'Saved for later'); }
  moveToCart(productId: string, size?: string | null) { this.apply(this.api.moveToCart(productId, size), 'Moved to cart'); }
  toggleWishlist(productId: string) {
    const was = this.isWishlisted(productId);
    this.apply(this.api.toggleWishlist(productId), was ? 'Removed from wishlist' : 'Added to wishlist');
  }
}
