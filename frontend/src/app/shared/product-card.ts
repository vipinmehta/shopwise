import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CurrencyPipe } from '@angular/common';
import { ProductListItem } from '../core/models';
import { CartService } from '../core/cart.service';
import { AuthService } from '../core/auth.service';
import { Router } from '@angular/router';
import { StarRating } from './star-rating';

@Component({
  selector: 'app-product-card',
  imports: [RouterLink, CurrencyPipe, StarRating],
  template: `
    <div class="pc card">
      <button class="heart" type="button" (click)="wish()" [title]="cart.isWishlisted(p().id) ? 'Remove from wishlist' : 'Add to wishlist'">
        {{ cart.isWishlisted(p().id) ? '♥' : '♡' }}
      </button>
      <a [routerLink]="['/product', p().id]" class="img">
        @if (p().thumbnail) { <img [src]="p().thumbnail" [alt]="p().name" loading="lazy" /> }
        @else { <div class="noimg">No image</div> }
      </a>
      <a [routerLink]="['/product', p().id]" class="name">{{ p().name }}</a>
      <div class="row small">
        <app-stars [value]="p().averageRating" />
        <span class="muted">({{ p().reviewCount }})</span>
      </div>
      <div class="row spread">
        <span class="price">
          {{ (p().discountPrice ?? p().price) | currency: 'INR' : 'symbol' : '1.0-0' }}
          @if (p().discountPrice) { <span class="strike">{{ p().price | currency: 'INR' : 'symbol' : '1.0-0' }}</span> }
        </span>
        @if (!p().inStock) { <span class="badge bad">Out of stock</span> }
      </div>
    </div>
  `,
  styles: `
    .pc { position: relative; padding: 12px; display: flex; flex-direction: column; gap: 6px; height: 100%; }
    .img { display: block; aspect-ratio: 3 / 4; background: #eef0f4; border-radius: 8px; overflow: hidden; }
    img { width: 100%; height: 100%; object-fit: cover; display: block; }
    .noimg { height: 100%; display: grid; place-items: center; color: var(--muted); }
    .name { color: var(--text); font-weight: 600; }
    .heart { position: absolute; top: 18px; right: 18px; z-index: 1; border-radius: 50%; width: 34px; height: 34px; padding: 0; font-size: 1.1rem; color: var(--danger); }
  `
})
export class ProductCard {
  p = input.required<ProductListItem>();
  cart = inject(CartService);
  private auth = inject(AuthService);
  private router = inject(Router);

  wish() {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }
    this.cart.toggleWishlist(this.p().id);
  }
}
