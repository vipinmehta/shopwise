import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, KeyValuePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { CartService } from '../../core/cart.service';
import { ToastService } from '../../core/toast.service';
import { ProductDetail as Product, Review } from '../../core/models';
import { StarRating } from '../../shared/star-rating';

@Component({
  selector: 'app-product-detail',
  imports: [CurrencyPipe, DatePipe, KeyValuePipe, FormsModule, RouterLink, StarRating],
  template: `
    @if (product(); as p) {
      <div class="top">
        <div class="gallery">
          <div class="main">
            @if (activeImage()) { <img [src]="activeImage()" [alt]="p.name" /> } @else { <div class="noimg">No image</div> }
          </div>
          <div class="row thumbs">
            @for (img of p.images; track img) {
              <img [src]="img" alt="" [class.sel]="img === activeImage()" (click)="activeImage.set(img)" />
            }
          </div>
        </div>

        <div class="info stack">
          <a routerLink="/" class="small">← Back to products</a>
          <h1>{{ p.name }}</h1>
          <div class="row">
            <app-stars [value]="p.averageRating" />
            <span class="muted small">{{ p.averageRating }} · {{ p.reviewCount }} reviews</span>
          </div>
          <div class="price big">
            {{ (p.discountPrice ?? p.price) | currency: 'INR' : 'symbol' : '1.0-0' }}
            @if (p.discountPrice) {
              <span class="strike">{{ p.price | currency: 'INR' : 'symbol' : '1.0-0' }}</span>
              <span class="badge ok">{{ discountPct() }}% off</span>
            }
          </div>
          <p>{{ p.description }}</p>

          @if (p.variants.length) {
            <div>
              <label>Select size</label>
              <div class="row">
                @for (v of p.variants; track v.sku) {
                  <button type="button" class="size" [class.btn-primary]="selectedSize() === v.size"
                          [disabled]="v.stock <= 0" (click)="selectedSize.set(v.size)">
                    {{ v.size }}
                  </button>
                }
              </div>
              @if (selectedVariant(); as v) {
                <p class="small" [class.error]="v.stock <= 5">{{ v.stock > 5 ? 'In stock' : 'Only ' + v.stock + ' left' }}</p>
              }
            </div>
          }

          <div class="row">
            <input class="qty" type="number" min="1" [ngModel]="qty()" (ngModelChange)="qty.set($event)" />
            <button type="button" class="btn-primary" (click)="addToCart()" [disabled]="!canBuy()">Add to cart</button>
            <button type="button" (click)="wish()">{{ cart.isWishlisted(p.id) ? '♥ Wishlisted' : '♡ Add to wishlist' }}</button>
          </div>
        </div>
      </div>

      @if (p.videos.length) {
        <section class="card sect">
          <h2>Product video</h2>
          <div class="row">
            @for (v of p.videos; track v) { <video [src]="v" controls preload="metadata"></video> }
          </div>
        </section>
      }

      @if (p.sizeChart.length) {
        <section class="card sect">
          <h2>Size chart</h2>
          <table>
            <thead>
              <tr><th>Size</th>@for (m of p.sizeChart[0].measurements | keyvalue; track m.key) { <th>{{ m.key }}</th> }</tr>
            </thead>
            <tbody>
              @for (row of p.sizeChart; track row.size) {
                <tr><td><b>{{ row.size }}</b></td>@for (m of row.measurements | keyvalue; track m.key) { <td>{{ m.value }}</td> }</tr>
              }
            </tbody>
          </table>
        </section>
      }

      <section class="card sect">
        <h2>Reviews ({{ reviews().length }})</h2>
        @if (auth.isLoggedIn()) {
          <form class="stack review-form" (ngSubmit)="submitReview()">
            <div class="row">
              <label for="rating" style="margin:0">Your rating</label>
              <select id="rating" name="rating" [(ngModel)]="newRating" style="width:auto">
                @for (n of [5,4,3,2,1]; track n) { <option [value]="n">{{ n }} ★</option> }
              </select>
            </div>
            <textarea name="comment" rows="3" placeholder="Share your experience" [(ngModel)]="newComment"></textarea>
            <div><button class="btn-primary" type="submit">Submit review</button></div>
          </form>
        } @else {
          <p class="muted"><a routerLink="/login" [queryParams]="{ returnUrl: router.url }">Sign in</a> to write a review.</p>
        }
        @for (r of reviews(); track r.id) {
          <div class="review">
            <div class="row"><b>{{ r.userName }}</b><app-stars [value]="r.rating" /><span class="muted small">{{ r.createdAt | date: 'mediumDate' }}</span></div>
            @if (r.comment) { <p>{{ r.comment }}</p> }
          </div>
        } @empty {
          <p class="muted">No reviews yet.</p>
        }
      </section>
    } @else if (notFound()) {
      <div class="card">Product not found. <a routerLink="/">Back to catalog</a></div>
    } @else {
      <p class="muted">Loading…</p>
    }
  `,
  styles: `
    .top { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 28px; align-items: start; }
    .main { aspect-ratio: 3 / 4; background: #eef0f4; border-radius: var(--radius); overflow: hidden; }
    .main img { width: 100%; height: 100%; object-fit: cover; }
    .noimg { height: 100%; display: grid; place-items: center; color: var(--muted); }
    .thumbs { margin-top: 10px; }
    .thumbs img { width: 64px; height: 84px; object-fit: cover; border-radius: 6px; border: 2px solid transparent; cursor: pointer; }
    .thumbs img.sel { border-color: var(--primary); }
    .big { font-size: 1.6rem; }
    .size { min-width: 48px; }
    .qty { width: 80px; }
    .sect { margin-top: 24px; }
    video { max-width: 100%; width: 420px; border-radius: 8px; }
    .review { border-top: 1px solid var(--border); padding: 12px 0; }
    .review p { margin: 6px 0 0; }
    .review-form { margin-bottom: 16px; }
    @media (max-width: 800px) { .top { grid-template-columns: 1fr; } }
  `
})
export class ProductDetail {
  id = input.required<string>();

  private api = inject(Api);
  private toast = inject(ToastService);
  auth = inject(AuthService);
  cart = inject(CartService);
  router = inject(Router);

  product = signal<Product | null>(null);
  reviews = signal<Review[]>([]);
  notFound = signal(false);
  activeImage = signal<string | null>(null);
  selectedSize = signal<string | null>(null);
  qty = signal(1);
  newRating = '5';
  newComment = '';

  selectedVariant = computed(() => this.product()?.variants.find(v => v.size === this.selectedSize()) ?? null);
  discountPct = computed(() => {
    const p = this.product();
    return p?.discountPrice ? Math.round((1 - p.discountPrice / p.price) * 100) : 0;
  });
  canBuy = computed(() => {
    const p = this.product();
    if (!p) return false;
    return p.variants.length === 0 || (this.selectedVariant()?.stock ?? 0) > 0;
  });

  constructor() {
    effect(() => {
      const id = this.id();
      this.product.set(null);
      this.notFound.set(false);
      this.api.product(id).subscribe({
        next: p => {
          this.product.set(p);
          this.activeImage.set(p.images[0] ?? null);
          this.selectedSize.set(p.variants.find(v => v.stock > 0)?.size ?? null);
        },
        error: () => this.notFound.set(true)
      });
      this.loadReviews(id);
    });
  }

  private loadReviews(id: string) {
    this.api.reviews(id).subscribe(r => this.reviews.set(r));
  }

  private requireLogin(): boolean {
    if (this.auth.isLoggedIn()) return true;
    this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
    return false;
  }

  addToCart() {
    if (!this.requireLogin()) return;
    this.cart.add(this.id(), this.selectedSize(), Math.max(1, Number(this.qty()) || 1));
  }

  wish() {
    if (!this.requireLogin()) return;
    this.cart.toggleWishlist(this.id());
  }

  submitReview() {
    this.api.addReview(this.id(), Number(this.newRating), this.newComment).subscribe({
      next: () => {
        this.newComment = '';
        this.toast.ok('Thanks for your review');
        this.loadReviews(this.id());
        this.api.product(this.id()).subscribe(p => this.product.set(p));
      },
      error: e => this.toast.fromError(e)
    });
  }
}
