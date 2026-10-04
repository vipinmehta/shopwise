import { Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { CartService } from '../../core/cart.service';
import { ToastService } from '../../core/toast.service';
import { Address } from '../../core/models';

const blank = (): Partial<Address> => ({ fullName: '', phone: '', line1: '', line2: '', city: '', state: '', pincode: '', country: 'India', isDefault: false });

@Component({
  selector: 'app-checkout',
  imports: [CurrencyPipe, FormsModule, RouterLink],
  template: `
    <h1>Checkout</h1>
    @if (cart.cart().items.length === 0) {
      <div class="card">Your cart is empty. <a routerLink="/">Continue shopping</a></div>
    } @else {
      <div class="layout">
        <div class="stack">
          <section class="card stack">
            <div class="row spread"><h2>1. Delivery address</h2><button type="button" class="btn-sm" (click)="startAdd()">+ Add address</button></div>

            @for (a of addresses(); track a.id) {
              <label class="addr row">
                <input type="radio" name="addr" [checked]="selectedId() === a.id" (change)="selectedId.set(a.id)" />
                <span class="grow">
                  <b>{{ a.fullName }}</b> · {{ a.phone }}
                  @if (a.isDefault) { <span class="badge">Default</span> }<br />
                  <span class="muted">{{ a.line1 }}{{ a.line2 ? ', ' + a.line2 : '' }}, {{ a.city }}, {{ a.state }} {{ a.pincode }}, {{ a.country }}</span>
                </span>
                <button type="button" class="btn-link" (click)="startEdit(a)">Edit</button>
                <button type="button" class="btn-link" (click)="remove(a)">Delete</button>
              </label>
            } @empty {
              @if (!editing()) { <p class="muted">No saved addresses yet. Add one to continue.</p> }
            }

            @if (editing()) {
              <form class="stack addr-form" (ngSubmit)="saveAddress()">
                <h3>{{ form.id ? 'Edit address' : 'New address' }}</h3>
                <div class="grid2">
                  <div class="field"><label>Full name</label><input name="fullName" [(ngModel)]="form.fullName" required /></div>
                  <div class="field"><label>Phone</label><input name="phone" [(ngModel)]="form.phone" required /></div>
                </div>
                <div class="field"><label>Address line 1</label><input name="line1" [(ngModel)]="form.line1" required /></div>
                <div class="field"><label>Address line 2</label><input name="line2" [(ngModel)]="form.line2" /></div>
                <div class="grid2">
                  <div class="field"><label>City</label><input name="city" [(ngModel)]="form.city" required /></div>
                  <div class="field"><label>State</label><input name="state" [(ngModel)]="form.state" required /></div>
                  <div class="field"><label>Pincode</label><input name="pincode" [(ngModel)]="form.pincode" required /></div>
                  <div class="field"><label>Country</label><input name="country" [(ngModel)]="form.country" required /></div>
                </div>
                <label class="row"><input type="checkbox" name="isDefault" [(ngModel)]="form.isDefault" /> Make this my default address</label>
                <div class="row">
                  <button class="btn-primary" type="submit">Save address</button>
                  <button type="button" (click)="editing.set(false)">Cancel</button>
                </div>
              </form>
            }
          </section>

          <section class="card stack">
            <h2>2. Coupon</h2>
            <div class="row">
              <input class="grow" placeholder="Enter coupon code" [ngModel]="couponInput()" (ngModelChange)="couponInput.set($event)" />
              <button type="button" (click)="applyCoupon()">Apply</button>
              @if (appliedCode()) { <button type="button" class="btn-link" (click)="clearCoupon()">Remove</button> }
            </div>
            @if (couponMessage()) { <p class="small" [class.error]="!appliedCode()">{{ couponMessage() }}</p> }
          </section>
        </div>

        <aside class="card summary stack">
          <h3>Order summary</h3>
          @for (i of cart.cart().items; track i.productId + (i.size ?? '')) {
            <div class="row spread small"><span>{{ i.productName }}{{ i.size ? ' (' + i.size + ')' : '' }} × {{ i.quantity }}</span><span>{{ i.unitPrice * i.quantity | currency: 'INR' : 'symbol' : '1.0-0' }}</span></div>
          }
          <hr />
          <div class="row spread"><span>Subtotal</span><span>{{ cart.cart().subtotal | currency: 'INR' : 'symbol' : '1.0-0' }}</span></div>
          <div class="row spread"><span>Discount</span><span>− {{ discount() | currency: 'INR' : 'symbol' : '1.0-0' }}</span></div>
          <div class="row spread"><b>Total</b><b class="price">{{ total() | currency: 'INR' : 'symbol' : '1.0-0' }}</b></div>
          <p class="small muted">Payment is simulated in this demo — no real charge is made.</p>
          <button type="button" class="btn-primary" [disabled]="!selectedId() || busy()" (click)="pay()">Pay {{ total() | currency: 'INR' : 'symbol' : '1.0-0' }}</button>
        </aside>
      </div>
    }
  `,
  styles: `
    .layout { display: grid; grid-template-columns: 1fr 340px; gap: 20px; align-items: start; }
    .addr { gap: 12px; border: 1px solid var(--border); border-radius: 8px; padding: 12px; cursor: pointer; color: var(--text); font-size: 1rem; margin: 0; }
    .addr-form { border-top: 1px solid var(--border); padding-top: 12px; }
    hr { border: none; border-top: 1px solid var(--border); width: 100%; }
    @media (max-width: 800px) { .layout { grid-template-columns: 1fr; } }
  `
})
export class Checkout {
  private api = inject(Api);
  private router = inject(Router);
  private toast = inject(ToastService);
  cart = inject(CartService);

  addresses = signal<Address[]>([]);
  selectedId = signal<string | null>(null);
  editing = signal(false);
  form: Partial<Address> = blank();

  couponInput = signal('');
  appliedCode = signal<string | null>(null);
  couponMessage = signal('');
  discount = signal(0);
  busy = signal(false);

  total = computed(() => Math.max(0, this.cart.cart().subtotal - this.discount()));

  constructor() {
    this.loadAddresses();
  }

  private loadAddresses(selectId?: string) {
    this.api.addresses().subscribe(list => {
      this.addresses.set(list);
      const current = selectId ?? this.selectedId();
      if (current && list.some(a => a.id === current)) this.selectedId.set(current);
      else this.selectedId.set(list.find(a => a.isDefault)?.id ?? list[0]?.id ?? null);
      if (list.length === 0) this.startAdd();
    });
  }

  startAdd() { this.form = blank(); this.editing.set(true); }
  startEdit(a: Address) { this.form = { ...a }; this.editing.set(true); }

  saveAddress() {
    const obs = this.form.id ? this.api.updateAddress(this.form.id, this.form) : this.api.createAddress(this.form);
    obs.subscribe({
      next: saved => { this.editing.set(false); this.toast.ok('Address saved'); this.loadAddresses(saved.id); },
      error: e => this.toast.fromError(e)
    });
  }

  remove(a: Address) {
    this.api.deleteAddress(a.id).subscribe({ next: () => this.loadAddresses(), error: e => this.toast.fromError(e) });
  }

  applyCoupon() {
    const code = this.couponInput().trim();
    if (!code) return;
    this.api.validateCoupon(code, this.cart.cart().subtotal).subscribe({
      next: r => {
        if (r.valid) {
          this.appliedCode.set(code.toUpperCase());
          this.discount.set(r.discount);
          this.couponMessage.set(`Coupon ${code.toUpperCase()} applied`);
        } else {
          this.clearCoupon();
          this.couponMessage.set(r.message ?? 'Invalid coupon');
        }
      },
      error: e => this.toast.fromError(e)
    });
  }

  clearCoupon() { this.appliedCode.set(null); this.discount.set(0); this.couponMessage.set(''); }

  pay() {
    const addressId = this.selectedId();
    if (!addressId) return;
    this.busy.set(true);
    this.api.checkout(addressId, this.appliedCode()).subscribe({
      next: order => {
        this.busy.set(false);
        this.cart.refresh();
        this.toast.ok(`Order ${order.orderNumber} placed`);
        this.router.navigate(['/orders', order.id]);
      },
      error: e => { this.busy.set(false); this.toast.fromError(e, 'Checkout failed'); }
    });
  }
}
