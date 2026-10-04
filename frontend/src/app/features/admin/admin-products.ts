import { Component, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { ToastService } from '../../core/toast.service';
import { Brand, Category, ProductDetail, ProductListItem, ProductVariant } from '../../core/models';

interface Form {
  id?: string; name: string; slug: string; description: string; categoryId: string; brandId: string;
  price: number; discountPrice: number | null; images: string; videos: string; tags: string;
  sizeChart: string; variants: ProductVariant[]; isActive: boolean;
}

const blank = (): Form => ({
  name: '', slug: '', description: '', categoryId: '', brandId: '', price: 0, discountPrice: null,
  images: '', videos: '', tags: '', sizeChart: '', variants: [{ size: '', color: null, sku: '', stock: 0 }], isActive: true
});

const lines = (s: string) => s.split('\n').map(x => x.trim()).filter(Boolean);

@Component({
  selector: 'app-admin-products',
  imports: [CurrencyPipe, FormsModule],
  template: `
    <div class="row spread"><h2>Products</h2><button class="btn-primary" type="button" (click)="startNew()">+ New product</button></div>

    @if (form(); as f) {
      <form class="card stack" (ngSubmit)="save()">
        <h3>{{ f.id ? 'Edit product' : 'New product' }}</h3>
        <div class="grid2">
          <div class="field"><label>Name</label><input name="name" [(ngModel)]="f.name" (ngModelChange)="autoSlug(f)" required /></div>
          <div class="field"><label>Slug</label><input name="slug" [(ngModel)]="f.slug" required /></div>
          <div class="field"><label>Category</label>
            <select name="cat" [(ngModel)]="f.categoryId" required><option value="">Select…</option>@for (c of categories(); track c.id) { <option [value]="c.id">{{ c.name }}</option> }</select></div>
          <div class="field"><label>Brand</label>
            <select name="brand" [(ngModel)]="f.brandId" required><option value="">Select…</option>@for (b of brands(); track b.id) { <option [value]="b.id">{{ b.name }}</option> }</select></div>
          <div class="field"><label>Price (INR)</label><input name="price" type="number" min="0" [(ngModel)]="f.price" required /></div>
          <div class="field"><label>Discount price (optional)</label><input name="dp" type="number" min="0" [(ngModel)]="f.discountPrice" /></div>
        </div>
        <div class="field"><label>Description</label><textarea name="desc" rows="3" [(ngModel)]="f.description"></textarea></div>
        <div class="grid2">
          <div class="field"><label>Image URLs (one per line)</label><textarea name="images" rows="3" [(ngModel)]="f.images"></textarea></div>
          <div class="field"><label>Video URLs (one per line)</label><textarea name="videos" rows="3" [(ngModel)]="f.videos"></textarea></div>
        </div>
        <div class="field"><label>Tags (comma separated)</label><input name="tags" [(ngModel)]="f.tags" /></div>
        <div class="field"><label>Size chart (one size per line, e.g. <code>M: Chest=38 in, Length=28 in</code>)</label><textarea name="chart" rows="3" [(ngModel)]="f.sizeChart"></textarea></div>

        <div>
          <label>Variants &amp; stock</label>
          @for (v of f.variants; track $index) {
            <div class="row var">
              <input [name]="'vs' + $index" placeholder="Size" [(ngModel)]="v.size" required />
              <input [name]="'vc' + $index" placeholder="Color" [(ngModel)]="v.color" />
              <input [name]="'vk' + $index" placeholder="SKU" [(ngModel)]="v.sku" required />
              <input [name]="'vt' + $index" type="number" min="0" placeholder="Stock" [(ngModel)]="v.stock" />
              <button type="button" class="btn-sm btn-danger" (click)="removeVariant(f, $index)">✕</button>
            </div>
          }
          <button type="button" class="btn-sm" (click)="f.variants.push({ size: '', color: null, sku: '', stock: 0 })">+ Add variant</button>
        </div>

        <label class="row"><input type="checkbox" name="active" [(ngModel)]="f.isActive" /> Active (visible in store)</label>
        <div class="row"><button class="btn-primary" type="submit">Save product</button><button type="button" (click)="form.set(null)">Cancel</button></div>
      </form>
    }

    <table class="list">
      <thead><tr><th>Name</th><th>Price</th><th>Stock</th><th>Rating</th><th></th></tr></thead>
      <tbody>
        @for (p of products(); track p.id) {
          <tr>
            <td>{{ p.name }}</td>
            <td>{{ (p.discountPrice ?? p.price) | currency: 'INR' : 'symbol' : '1.0-0' }}</td>
            <td><span class="badge" [class.ok]="p.inStock" [class.bad]="!p.inStock">{{ p.inStock ? 'In stock' : 'Out' }}</span></td>
            <td><span class="muted small">★ {{ p.averageRating }} ({{ p.reviewCount }})</span></td>
            <td class="right"><button type="button" class="btn-sm" (click)="edit(p.id)">Edit</button> <button type="button" class="btn-sm btn-danger" (click)="remove(p)">Delete</button></td>
          </tr>
        } @empty { <tr><td colspan="5" class="muted">No products.</td></tr> }
      </tbody>
    </table>
  `,
  styles: `.list { margin-top: 16px; } .var { margin-bottom: 8px; } .var input { width: auto; flex: 1; min-width: 90px; } code { background: #eef0f4; padding: 0 4px; border-radius: 4px; }`
})
export class AdminProducts {
  private api = inject(Api);
  private toast = inject(ToastService);

  products = signal<ProductListItem[]>([]);
  categories = signal<Category[]>([]);
  brands = signal<Brand[]>([]);
  form = signal<Form | null>(null);

  constructor() {
    this.api.categories().subscribe(c => this.categories.set(c));
    this.api.brands().subscribe(b => this.brands.set(b));
    this.load();
  }

  load() {
    this.api.products({ pageSize: 100, sort: 'newest' }).subscribe(r => this.products.set(r.items));
  }

  startNew() { this.form.set(blank()); }

  autoSlug(f: Form) {
    if (!f.id) f.slug = f.name.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
  }

  removeVariant(f: Form, i: number) { f.variants.splice(i, 1); }

  edit(id: string) {
    this.api.product(id).subscribe(p => this.form.set(this.toForm(p)));
  }

  private toForm(p: ProductDetail): Form {
    return {
      id: p.id, name: p.name, slug: p.slug, description: p.description, categoryId: p.categoryId, brandId: p.brandId,
      price: p.price, discountPrice: p.discountPrice ?? null, images: p.images.join('\n'), videos: p.videos.join('\n'),
      tags: p.tags.join(', '),
      sizeChart: p.sizeChart.map(r => `${r.size}: ${Object.entries(r.measurements).map(([k, v]) => `${k}=${v}`).join(', ')}`).join('\n'),
      variants: p.variants.map(v => ({ ...v })), isActive: p.isActive
    };
  }

  private parseChart(text: string) {
    return lines(text).map(line => {
      const [size, rest = ''] = line.split(':');
      const measurements: Record<string, string> = {};
      rest.split(',').map(x => x.trim()).filter(Boolean).forEach(pair => {
        const [k, ...v] = pair.split('=');
        measurements[k.trim()] = v.join('=').trim();
      });
      return { size: size.trim(), measurements };
    });
  }

  save() {
    const f = this.form();
    if (!f) return;
    const body = {
      name: f.name, slug: f.slug, description: f.description, categoryId: f.categoryId, brandId: f.brandId,
      price: Number(f.price), discountPrice: f.discountPrice ? Number(f.discountPrice) : null,
      images: lines(f.images), videos: lines(f.videos),
      variants: f.variants.map(v => ({ size: v.size, color: v.color || null, sku: v.sku, stock: Number(v.stock) || 0 })),
      sizeChart: this.parseChart(f.sizeChart),
      tags: f.tags.split(',').map(t => t.trim()).filter(Boolean),
      isActive: f.isActive
    };
    const obs = f.id ? this.api.updateProduct(f.id, body) : this.api.createProduct(body);
    obs.subscribe({
      next: () => { this.toast.ok('Product saved'); this.form.set(null); this.load(); },
      error: e => this.toast.fromError(e)
    });
  }

  remove(p: ProductListItem) {
    if (!confirm(`Delete "${p.name}"?`)) return;
    this.api.deleteProduct(p.id).subscribe({ next: () => { this.toast.ok('Deleted'); this.load(); }, error: e => this.toast.fromError(e) });
  }
}
