import { Component, inject } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/cart.service';

@Component({
  selector: 'app-cart',
  imports: [CurrencyPipe, RouterLink],
  template: `
    <h1>Your cart</h1>
    @if (svc.cart().items.length === 0 && svc.cart().savedForLater.length === 0) {
      <div class="card">Your cart is empty. <a routerLink="/">Continue shopping</a></div>
    }

    @if (svc.cart().items.length) {
      <div class="layout">
        <div class="stack">
          @for (i of svc.cart().items; track i.productId + (i.size ?? '')) {
            <div class="card item row">
              <div class="thumb">@if (i.thumbnail) { <img [src]="i.thumbnail" alt="" /> }</div>
              <div class="grow">
                <a [routerLink]="['/product', i.productId]"><b>{{ i.productName }}</b></a>
                @if (i.size) { <div class="muted small">Size: {{ i.size }}</div> }
                <div class="price">{{ i.unitPrice | currency: 'INR' : 'symbol' : '1.0-0' }}</div>
                @if (i.quantity > i.availableStock) { <div class="error small">Only {{ i.availableStock }} available</div> }
                <div class="row small">
                  <button type="button" class="btn-link" (click)="svc.saveForLater(i.productId, i.size)">Save for later</button>
                  <button type="button" class="btn-link" (click)="svc.remove(i.productId, i.size)">Remove</button>
                </div>
              </div>
              <div class="row">
                <button type="button" class="btn-sm" (click)="svc.setQuantity(i.productId, i.size, i.quantity - 1)">−</button>
                <b>{{ i.quantity }}</b>
                <button type="button" class="btn-sm" (click)="svc.setQuantity(i.productId, i.size, i.quantity + 1)" [disabled]="i.quantity >= i.availableStock">+</button>
              </div>
              <div class="price right line">{{ i.unitPrice * i.quantity | currency: 'INR' : 'symbol' : '1.0-0' }}</div>
            </div>
          }
        </div>
        <aside class="card summary stack">
          <h3>Order summary</h3>
          <div class="row spread"><span>Subtotal</span><span class="price">{{ svc.cart().subtotal | currency: 'INR' : 'symbol' : '1.0-0' }}</span></div>
          <p class="muted small">Coupons and shipping are applied at checkout.</p>
          <a routerLink="/checkout" class="btn btn-primary">Proceed to checkout</a>
        </aside>
      </div>
    }

    @if (svc.cart().savedForLater.length) {
      <h2 class="later">Saved for later</h2>
      <div class="stack">
        @for (i of svc.cart().savedForLater; track i.productId + (i.size ?? '')) {
          <div class="card item row">
            <div class="thumb">@if (i.thumbnail) { <img [src]="i.thumbnail" alt="" /> }</div>
            <div class="grow">
              <a [routerLink]="['/product', i.productId]"><b>{{ i.productName }}</b></a>
              @if (i.size) { <div class="muted small">Size: {{ i.size }}</div> }
              <div class="price">{{ i.unitPrice | currency: 'INR' : 'symbol' : '1.0-0' }}</div>
            </div>
            <button type="button" (click)="svc.moveToCart(i.productId, i.size)">Move to cart</button>
          </div>
        }
      </div>
    }
  `,
  styles: `
    .layout { display: grid; grid-template-columns: 1fr 320px; gap: 20px; align-items: start; }
    .item { gap: 16px; }
    .thumb { width: 80px; height: 104px; background: #eef0f4; border-radius: 8px; overflow: hidden; flex: none; }
    .thumb img { width: 100%; height: 100%; object-fit: cover; }
    .line { min-width: 90px; }
    .later { margin-top: 32px; }
    @media (max-width: 800px) { .layout { grid-template-columns: 1fr; } }
  `
})
export class Cart {
  svc = inject(CartService);
}
