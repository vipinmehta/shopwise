import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Brand, Category } from '../../core/models';

const slugify = (s: string) => s.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');

@Component({
  selector: 'app-admin-catalog',
  imports: [FormsModule],
  template: `
    <div class="cols">
      <section class="stack">
        <h2>Categories</h2>
        <form class="card stack" (ngSubmit)="saveCategory()">
          <div class="grid2">
            <div class="field"><label>Name</label><input name="cn" [(ngModel)]="cat.name" (ngModelChange)="cat.slug = cat.id ? cat.slug : slug($event)" required /></div>
            <div class="field"><label>Slug</label><input name="cs" [(ngModel)]="cat.slug" required /></div>
          </div>
          <div class="field"><label>Parent category</label>
            <select name="cp" [(ngModel)]="cat.parentId"><option [ngValue]="null">None (top level)</option>
              @for (c of categories(); track c.id) { @if (c.id !== cat.id) { <option [ngValue]="c.id">{{ c.name }}</option> } }
            </select></div>
          <label class="row"><input type="checkbox" name="ca" [(ngModel)]="cat.isActive" /> Active</label>
          <div class="row"><button class="btn-primary" type="submit">{{ cat.id ? 'Update' : 'Add' }} category</button>@if (cat.id) { <button type="button" (click)="resetCat()">Cancel</button> }</div>
        </form>
        <table>
          <thead><tr><th>Name</th><th>Parent</th><th>Active</th><th></th></tr></thead>
          <tbody>
            @for (c of categories(); track c.id) {
              <tr><td>{{ c.name }}</td><td class="muted">{{ parentName(c) }}</td><td>{{ c.isActive ? 'Yes' : 'No' }}</td>
                <td class="right"><button type="button" class="btn-sm" (click)="cat = { ...c }">Edit</button> <button type="button" class="btn-sm btn-danger" (click)="delCategory(c)">Delete</button></td></tr>
            } @empty { <tr><td colspan="4" class="muted">None yet.</td></tr> }
          </tbody>
        </table>
      </section>

      <section class="stack">
        <h2>Brands</h2>
        <form class="card stack" (ngSubmit)="saveBrand()">
          <div class="grid2">
            <div class="field"><label>Name</label><input name="bn" [(ngModel)]="brand.name" (ngModelChange)="brand.slug = brand.id ? brand.slug : slug($event)" required /></div>
            <div class="field"><label>Slug</label><input name="bs" [(ngModel)]="brand.slug" required /></div>
          </div>
          <div class="field"><label>Logo URL</label><input name="bl" [(ngModel)]="brand.logo" /></div>
          <label class="row"><input type="checkbox" name="ba" [(ngModel)]="brand.isActive" /> Active</label>
          <div class="row"><button class="btn-primary" type="submit">{{ brand.id ? 'Update' : 'Add' }} brand</button>@if (brand.id) { <button type="button" (click)="resetBrand()">Cancel</button> }</div>
        </form>
        <table>
          <thead><tr><th>Name</th><th>Active</th><th></th></tr></thead>
          <tbody>
            @for (b of brands(); track b.id) {
              <tr><td>{{ b.name }}</td><td>{{ b.isActive ? 'Yes' : 'No' }}</td>
                <td class="right"><button type="button" class="btn-sm" (click)="brand = { ...b }">Edit</button> <button type="button" class="btn-sm btn-danger" (click)="delBrand(b)">Delete</button></td></tr>
            } @empty { <tr><td colspan="3" class="muted">None yet.</td></tr> }
          </tbody>
        </table>
      </section>
    </div>
  `,
  styles: `.cols { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; align-items: start; } @media (max-width: 900px) { .cols { grid-template-columns: 1fr; } }`
})
export class AdminCatalog {
  private api = inject(Api);
  private toast = inject(ToastService);

  categories = signal<Category[]>([]);
  brands = signal<Brand[]>([]);
  cat: Partial<Category> = { name: '', slug: '', parentId: null, isActive: true };
  brand: Partial<Brand> = { name: '', slug: '', logo: '', isActive: true };
  slug = slugify;

  constructor() { this.load(); }

  load() {
    this.api.categories().subscribe(c => this.categories.set(c));
    this.api.brands().subscribe(b => this.brands.set(b));
  }

  parentName(c: Category) { return this.categories().find(x => x.id === c.parentId)?.name ?? '—'; }

  resetCat() { this.cat = { name: '', slug: '', parentId: null, isActive: true }; }
  resetBrand() { this.brand = { name: '', slug: '', logo: '', isActive: true }; }

  saveCategory() {
    const obs = this.cat.id ? this.api.updateCategory(this.cat.id, this.cat) : this.api.createCategory(this.cat);
    obs.subscribe({ next: () => { this.toast.ok('Category saved'); this.resetCat(); this.load(); }, error: e => this.toast.fromError(e) });
  }

  saveBrand() {
    const obs = this.brand.id ? this.api.updateBrand(this.brand.id, this.brand) : this.api.createBrand(this.brand);
    obs.subscribe({ next: () => { this.toast.ok('Brand saved'); this.resetBrand(); this.load(); }, error: e => this.toast.fromError(e) });
  }

  delCategory(c: Category) {
    if (!confirm(`Delete category "${c.name}"?`)) return;
    this.api.deleteCategory(c.id).subscribe({ next: () => this.load(), error: e => this.toast.fromError(e) });
  }

  delBrand(b: Brand) {
    if (!confirm(`Delete brand "${b.name}"?`)) return;
    this.api.deleteBrand(b.id).subscribe({ next: () => this.load(), error: e => this.toast.fromError(e) });
  }
}
