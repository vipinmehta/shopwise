import { Component, effect, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin, of, catchError } from 'rxjs';
import { Api } from '../../core/api.service';
import { CartService } from '../../core/cart.service';
import { ProductListItem } from '../../core/models';
import { ProductCard } from '../../shared/product-card';

@Component({
  selector: 'app-wishlist',
  imports: [ProductCard, RouterLink],
  template: `
    <h1>Your wishlist</h1>
    @if (loading()) { <p class="muted">Loading…</p> }
    @else if (products().length === 0) { <div class="card">Nothing here yet. <a routerLink="/">Browse products</a></div> }
    @else {
      <div class="grid">
        @for (p of products(); track p.id) { <app-product-card [p]="p" /> }
      </div>
    }
  `,
  styles: `.grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 16px; }`
})
export class Wishlist {
  private api = inject(Api);
  private cart = inject(CartService);

  products = signal<ProductListItem[]>([]);
  loading = signal(true);

  constructor() {
    effect(() => {
      const ids = this.cart.cart().wishlistProductIds;
      if (ids.length === 0) {
        this.products.set([]);
        this.loading.set(false);
        return;
      }
      forkJoin(ids.map(id => this.api.product(id).pipe(catchError(() => of(null))))).subscribe(list => {
        this.products.set(
          list.filter((p): p is NonNullable<typeof p> => p !== null).map(p => ({
            id: p.id, name: p.name, slug: p.slug, price: p.price, discountPrice: p.discountPrice,
            thumbnail: p.images[0] ?? null, averageRating: p.averageRating, reviewCount: p.reviewCount,
            inStock: p.variants.some(v => v.stock > 0)
          }))
        );
        this.loading.set(false);
      });
    });
  }
}
