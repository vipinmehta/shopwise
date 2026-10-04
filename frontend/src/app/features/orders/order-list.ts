import { Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Api } from '../../core/api.service';
import { Order } from '../../core/models';
import { statusClass } from './status';

@Component({
  selector: 'app-order-list',
  imports: [CurrencyPipe, DatePipe, RouterLink],
  template: `
    <h1>Your orders</h1>
    @if (loading()) { <p class="muted">Loading…</p> }
    @else if (orders().length === 0) { <div class="card">You haven't placed any orders yet. <a routerLink="/">Start shopping</a></div> }
    @else {
      <div class="stack">
        @for (o of orders(); track o.id) {
          <a class="card row spread link" [routerLink]="['/orders', o.id]">
            <div>
              <b>{{ o.orderNumber }}</b>
              <div class="muted small">{{ o.createdAt | date: 'medium' }} · {{ o.items.length }} item(s)</div>
            </div>
            <span class="badge" [class]="'badge ' + cls(o.status)">{{ o.status }}</span>
            <span class="price">{{ o.total | currency: 'INR' : 'symbol' : '1.0-0' }}</span>
          </a>
        }
      </div>
    }
  `,
  styles: `.link { color: var(--text); } .link:hover { text-decoration: none; border-color: var(--primary); }`
})
export class OrderList {
  private api = inject(Api);
  orders = signal<Order[]>([]);
  loading = signal(true);
  cls = statusClass;

  constructor() {
    this.api.orders().subscribe({ next: o => { this.orders.set(o); this.loading.set(false); }, error: () => this.loading.set(false) });
  }
}
