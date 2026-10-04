import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Api } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { ProductDetail } from '../../core/models';

@Component({
  selector: 'app-admin-inventory',
  imports: [FormsModule],
  template: `
    <h2>Inventory</h2>
    <p class="muted small">Adjust stock per size. Use a positive number to add stock and a negative number to remove it.</p>
    <table>
      <thead><tr><th>Product</th><th>Size</th><th>SKU</th><th>Stock</th><th>Adjust by</th><th></th></tr></thead>
      <tbody>
        @for (p of products(); track p.id) {
          @for (v of p.variants; track v.sku; let first = $first) {
            <tr>
              <td>@if (first) { <b>{{ p.name }}</b> }</td>
              <td>{{ v.size }}</td>
              <td class="muted small">{{ v.sku }}</td>
              <td><span class="badge" [class.bad]="v.stock === 0" [class.warn]="v.stock > 0 && v.stock <= 5" [class.ok]="v.stock > 5">{{ v.stock }}</span></td>
              <td style="width:120px"><input type="number" [ngModel]="delta[p.id + v.size] ?? 0" (ngModelChange)="delta[p.id + v.size] = $event" /></td>
              <td><button type="button" class="btn-sm" (click)="apply(p, v.size)">Apply</button></td>
            </tr>
          }
        } @empty { <tr><td colspan="6" class="muted">No products.</td></tr> }
      </tbody>
    </table>
  `
})
export class AdminInventory {
  private api = inject(Api);
  private toast = inject(ToastService);

  products = signal<ProductDetail[]>([]);
  delta: Record<string, number> = {};

  constructor() { this.load(); }

  load() {
    this.api.products({ pageSize: 100, sort: 'newest' }).subscribe(r => {
      if (r.items.length === 0) { this.products.set([]); return; }
      forkJoin(r.items.map(i => this.api.product(i.id))).subscribe(list => this.products.set(list));
    });
  }

  apply(p: ProductDetail, size: string) {
    const d = Number(this.delta[p.id + size] ?? 0);
    if (!d) return;
    this.api.adjustStock(p.id, size, d).subscribe({
      next: () => { this.toast.ok('Stock updated'); this.delta[p.id + size] = 0; this.load(); },
      error: e => this.toast.fromError(e)
    });
  }
}
