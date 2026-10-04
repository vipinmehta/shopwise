import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Coupon } from '../../core/models';

interface Form { id?: string; code: string; type: string; value: number; minOrderValue: number; expiresAt: string; usageLimit: number | null; isActive: boolean; }
const blank = (): Form => ({ code: '', type: 'Percentage', value: 10, minOrderValue: 0, expiresAt: '', usageLimit: null, isActive: true });

@Component({
  selector: 'app-admin-coupons',
  imports: [DatePipe, FormsModule],
  template: `
    <h2>Promotions &amp; coupons</h2>
    <form class="card stack" (ngSubmit)="save()">
      <h3>{{ f.id ? 'Edit coupon' : 'New coupon' }}</h3>
      <div class="grid2">
        <div class="field"><label>Code</label><input name="code" [(ngModel)]="f.code" required style="text-transform:uppercase" /></div>
        <div class="field"><label>Type</label>
          <select name="type" [(ngModel)]="f.type"><option value="Percentage">Percentage (%)</option><option value="Flat">Flat amount (₹)</option></select></div>
        <div class="field"><label>Value</label><input name="value" type="number" min="0" [(ngModel)]="f.value" required /></div>
        <div class="field"><label>Minimum order value (₹)</label><input name="min" type="number" min="0" [(ngModel)]="f.minOrderValue" /></div>
        <div class="field"><label>Expires on (optional)</label><input name="exp" type="date" [(ngModel)]="f.expiresAt" /></div>
        <div class="field"><label>Usage limit (optional)</label><input name="lim" type="number" min="1" [(ngModel)]="f.usageLimit" /></div>
      </div>
      <label class="row"><input type="checkbox" name="act" [(ngModel)]="f.isActive" /> Active</label>
      <div class="row"><button class="btn-primary" type="submit">{{ f.id ? 'Update' : 'Create' }} coupon</button>@if (f.id) { <button type="button" (click)="f = blank()">Cancel</button> }</div>
    </form>

    <table style="margin-top:16px">
      <thead><tr><th>Code</th><th>Discount</th><th>Min order</th><th>Expires</th><th>Used</th><th>Status</th><th></th></tr></thead>
      <tbody>
        @for (c of coupons(); track c.id) {
          <tr>
            <td><b>{{ c.code }}</b></td>
            <td>{{ c.type === 'Percentage' ? c.value + '%' : '₹' + c.value }}</td>
            <td>₹{{ c.minOrderValue }}</td>
            <td>{{ c.expiresAt ? (c.expiresAt | date: 'mediumDate') : 'Never' }}</td>
            <td>{{ c.usedCount }}{{ c.usageLimit ? ' / ' + c.usageLimit : '' }}</td>
            <td><span class="badge" [class.ok]="c.isActive" [class.bad]="!c.isActive">{{ c.isActive ? 'Active' : 'Inactive' }}</span></td>
            <td class="right"><button type="button" class="btn-sm" (click)="edit(c)">Edit</button> <button type="button" class="btn-sm btn-danger" (click)="remove(c)">Delete</button></td>
          </tr>
        } @empty { <tr><td colspan="7" class="muted">No coupons yet.</td></tr> }
      </tbody>
    </table>
  `
})
export class AdminCoupons {
  private api = inject(Api);
  private toast = inject(ToastService);

  coupons = signal<Coupon[]>([]);
  f: Form = blank();
  blank = blank;

  constructor() { this.load(); }

  load() { this.api.coupons().subscribe(c => this.coupons.set(c)); }

  edit(c: Coupon) {
    this.f = {
      id: c.id, code: c.code, type: c.type, value: c.value, minOrderValue: c.minOrderValue,
      expiresAt: c.expiresAt ? c.expiresAt.substring(0, 10) : '', usageLimit: c.usageLimit ?? null, isActive: c.isActive
    };
  }

  save() {
    const body = {
      code: this.f.code, type: this.f.type, value: Number(this.f.value), minOrderValue: Number(this.f.minOrderValue) || 0,
      expiresAt: this.f.expiresAt ? new Date(this.f.expiresAt + 'T23:59:59Z').toISOString() : null,
      usageLimit: this.f.usageLimit ? Number(this.f.usageLimit) : null, isActive: this.f.isActive
    };
    const obs = this.f.id ? this.api.updateCoupon(this.f.id, body) : this.api.createCoupon(body);
    obs.subscribe({ next: () => { this.toast.ok('Coupon saved'); this.f = blank(); this.load(); }, error: e => this.toast.fromError(e) });
  }

  remove(c: Coupon) {
    if (!confirm(`Delete coupon ${c.code}?`)) return;
    this.api.deleteCoupon(c.id).subscribe({ next: () => this.load(), error: e => this.toast.fromError(e) });
  }
}
