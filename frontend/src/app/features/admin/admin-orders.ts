import { Component, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Order, PagedResult } from '../../core/models';
import { ORDER_STATUSES, statusClass } from '../orders/status';

@Component({
  selector: 'app-admin-orders',
  imports: [CurrencyPipe, DatePipe, FormsModule],
  template: `
    <div class="row spread">
      <h2>Orders</h2>
      <div class="row">
        <label for="f" style="margin:0">Status</label>
        <select id="f" style="width:auto" [ngModel]="filter()" (ngModelChange)="filter.set($event); page.set(1); load()">
          <option value="">All</option>
          @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
        </select>
      </div>
    </div>

    <table>
      <thead><tr><th>Order</th><th>Customer / ship to</th><th>Items</th><th>Total</th><th>Payment</th><th>Status</th><th>Update</th></tr></thead>
      <tbody>
        @for (o of result()?.items ?? []; track o.id) {
          <tr>
            <td><b>{{ o.orderNumber }}</b><div class="muted small">{{ o.createdAt | date: 'short' }}</div></td>
            <td>{{ o.shippingAddress.fullName }}<div class="muted small">{{ o.shippingAddress.city }}</div></td>
            <td>{{ o.items.length }}</td>
            <td>{{ o.total | currency: 'INR' : 'symbol' : '1.0-0' }}</td>
            <td><span class="badge" [class.ok]="o.paymentStatus === 'Paid'">{{ o.paymentStatus }}</span></td>
            <td><span class="badge" [class]="'badge ' + cls(o.status)">{{ o.status }}</span></td>
            <td>
              <select style="width:auto" [ngModel]="o.status" (ngModelChange)="update(o, $event)">
                @for (s of statuses; track s) { <option [value]="s">{{ s }}</option> }
              </select>
            </td>
          </tr>
        } @empty { <tr><td colspan="7" class="muted">No orders.</td></tr> }
      </tbody>
    </table>

    @if ((result()?.totalPages ?? 0) > 1) {
      <div class="row" style="justify-content:center;margin-top:16px">
        <button type="button" [disabled]="page() <= 1" (click)="page.set(page() - 1); load()">‹ Prev</button>
        <span>Page {{ page() }} of {{ result()!.totalPages }}</span>
        <button type="button" [disabled]="page() >= result()!.totalPages" (click)="page.set(page() + 1); load()">Next ›</button>
      </div>
    }
  `
})
export class AdminOrders {
  private api = inject(Api);
  private toast = inject(ToastService);

  statuses = ORDER_STATUSES;
  cls = statusClass;
  result = signal<PagedResult<Order> | null>(null);
  filter = signal('');
  page = signal(1);

  constructor() { this.load(); }

  load() {
    this.api.adminOrders(this.page(), this.filter() || undefined).subscribe(r => this.result.set(r));
  }

  update(o: Order, status: string) {
    if (status === o.status) return;
    this.api.adminUpdateOrderStatus(o.id, status).subscribe({
      next: () => { this.toast.ok(`Order ${o.orderNumber} → ${status}`); this.load(); },
      error: e => { this.toast.fromError(e); this.load(); }
    });
  }
}
