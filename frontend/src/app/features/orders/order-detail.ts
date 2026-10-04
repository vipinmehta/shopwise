import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Order } from '../../core/models';
import { statusClass } from './status';

@Component({
  selector: 'app-order-detail',
  imports: [CurrencyPipe, DatePipe, RouterLink, FormsModule],
  template: `
    <a routerLink="/orders" class="small">← All orders</a>
    @if (order(); as o) {
      <div class="row spread head">
        <div>
          <h1>Order {{ o.orderNumber }}</h1>
          <span class="muted small">Placed {{ o.createdAt | date: 'medium' }}</span>
        </div>
        <span class="badge" [class]="'badge ' + cls(o.status)">{{ o.status }}</span>
      </div>

      <div class="layout">
        <div class="stack">
          <section class="card">
            <h2>Items</h2>
            @for (i of o.items; track i.productId + (i.size ?? '')) {
              <div class="row item">
                <div class="thumb">@if (i.image) { <img [src]="i.image" alt="" /> }</div>
                <div class="grow">
                  <a [routerLink]="['/product', i.productId]"><b>{{ i.productName }}</b></a>
                  <div class="muted small">{{ i.size ? 'Size ' + i.size + ' · ' : '' }}Qty {{ i.quantity }}</div>
                </div>
                <span class="price">{{ i.unitPrice * i.quantity | currency: 'INR' : 'symbol' : '1.0-0' }}</span>
              </div>
            }
          </section>

          <section class="card">
            <h2>Tracking</h2>
            <ol class="timeline">
              @for (t of timeline(); track t.timestamp) {
                <li>
                  <b>{{ t.status }}</b> <span class="muted small">{{ t.timestamp | date: 'medium' }}</span>
                  @if (t.note) { <div class="small muted">{{ t.note }}</div> }
                </li>
              }
            </ol>
          </section>
        </div>

        <aside class="stack">
          <section class="card stack">
            <h3>Payment</h3>
            <div class="row spread"><span>Subtotal</span><span>{{ o.subtotal | currency: 'INR' : 'symbol' : '1.0-0' }}</span></div>
            @if (o.discount > 0) { <div class="row spread"><span>Discount {{ o.couponCode ? '(' + o.couponCode + ')' : '' }}</span><span>− {{ o.discount | currency: 'INR' : 'symbol' : '1.0-0' }}</span></div> }
            <div class="row spread"><b>Total</b><b>{{ o.total | currency: 'INR' : 'symbol' : '1.0-0' }}</b></div>
            <span class="badge" [class.ok]="o.paymentStatus === 'Paid'">{{ o.paymentStatus }}</span>
          </section>

          <section class="card">
            <h3>Shipping to</h3>
            <div class="small">
              <b>{{ o.shippingAddress.fullName }}</b><br />
              {{ o.shippingAddress.line1 }}{{ o.shippingAddress.line2 ? ', ' + o.shippingAddress.line2 : '' }}<br />
              {{ o.shippingAddress.city }}, {{ o.shippingAddress.state }} {{ o.shippingAddress.pincode }}<br />
              {{ o.shippingAddress.country }} · {{ o.shippingAddress.phone }}
            </div>
          </section>

          @if (canCancel() || canReturn()) {
            <section class="card stack">
              <h3>{{ canCancel() ? 'Cancel order' : 'Return order' }}</h3>
              <textarea rows="2" placeholder="Reason (optional)" [ngModel]="reason()" (ngModelChange)="reason.set($event)"></textarea>
              @if (canCancel()) { <button type="button" class="btn-danger" (click)="cancel()">Cancel this order</button> }
              @if (canReturn()) { <button type="button" (click)="requestReturn()">Request return</button> }
            </section>
          }
        </aside>
      </div>
    } @else {
      <p class="muted">Loading…</p>
    }
  `,
  styles: `
    .head { margin: 8px 0 16px; }
    .layout { display: grid; grid-template-columns: 1fr 320px; gap: 20px; align-items: start; }
    .item { padding: 10px 0; border-top: 1px solid var(--border); gap: 14px; }
    .thumb { width: 56px; height: 72px; background: #eef0f4; border-radius: 6px; overflow: hidden; flex: none; }
    .thumb img { width: 100%; height: 100%; object-fit: cover; }
    .timeline { list-style: none; margin: 0; padding: 0 0 0 18px; border-left: 2px solid var(--border); }
    .timeline li { position: relative; padding: 0 0 14px 14px; }
    .timeline li::before { content: ''; position: absolute; left: -25px; top: 6px; width: 10px; height: 10px; border-radius: 50%; background: var(--primary); }
    @media (max-width: 800px) { .layout { grid-template-columns: 1fr; } }
  `
})
export class OrderDetail {
  id = input.required<string>();
  private api = inject(Api);
  private toast = inject(ToastService);

  order = signal<Order | null>(null);
  reason = signal('');
  cls = statusClass;

  timeline = computed(() => [...(this.order()?.trackingHistory ?? [])].reverse());
  canCancel = computed(() => ['Placed', 'Confirmed'].includes(this.order()?.status ?? ''));
  canReturn = computed(() => this.order()?.status === 'Delivered');

  constructor() {
    effect(() => {
      this.api.order(this.id()).subscribe({ next: o => this.order.set(o), error: e => this.toast.fromError(e, 'Order not found') });
    });
  }

  cancel() {
    this.api.cancelOrder(this.id(), this.reason()).subscribe({
      next: o => { this.order.set(o); this.toast.ok('Order cancelled'); },
      error: e => this.toast.fromError(e)
    });
  }

  requestReturn() {
    this.api.returnOrder(this.id(), this.reason()).subscribe({
      next: o => { this.order.set(o); this.toast.ok('Return requested'); },
      error: e => this.toast.fromError(e)
    });
  }
}
