import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { Brand, Category, PagedResult, ProductListItem } from '../../core/models';
import { ProductCard } from '../../shared/product-card';

@Component({
  selector: 'app-product-list',
  imports: [FormsModule, ProductCard],
  template: `
    <div class="layout">
      <aside class="card filters stack">
        <h3>Filters</h3>
        <div class="field">
          <label for="cat">Category</label>
          <select id="cat" [ngModel]="categoryId()" (ngModelChange)="categoryId.set($event); page.set(1)">
            <option value="">All categories</option>
            @for (c of categoryOptions(); track c.id) { <option [value]="c.id">{{ c.label }}</option> }
          </select>
        </div>
        <div class="field">
          <label for="brand">Brand</label>
          <select id="brand" [ngModel]="brandId()" (ngModelChange)="brandId.set($event); page.set(1)">
            <option value="">All brands</option>
            @for (b of brands(); track b.id) { <option [value]="b.id">{{ b.name }}</option> }
          </select>
        </div>
        <div class="field">
          <label>Price range</label>
          <div class="row">
            <input type="number" min="0" placeholder="Min" [ngModel]="minPrice()" (ngModelChange)="minPrice.set($event)" />
            <input type="number" min="0" placeholder="Max" [ngModel]="maxPrice()" (ngModelChange)="maxPrice.set($event)" />
          </div>
        </div>
        <button type="button" (click)="page.set(1); load()">Apply price</button>
        <button type="button" class="btn-link" (click)="reset()">Clear all filters</button>
      </aside>

      <section>
        <div class="row spread bar">
          <div>
            <h1>{{ search() ? 'Results for "' + search() + '"' : (categoryName() || 'All products') }}</h1>
            <span class="muted small">{{ result()?.totalCount ?? 0 }} items</span>
          </div>
          <div>
            <label for="sort">Sort by</label>
            <select id="sort" [ngModel]="sort()" (ngModelChange)="sort.set($event); page.set(1)">
              <option value="newest">Newest</option>
              <option value="price_asc">Price: low to high</option>
              <option value="price_desc">Price: high to low</option>
              <option value="rating">Top rated</option>
            </select>
          </div>
        </div>

        @if (loading()) { <p class="muted">Loading…</p> }
        @else if (!result()?.items?.length) { <div class="card muted">No products match your filters.</div> }
        @else {
          <div class="grid">
            @for (p of result()!.items; track p.id) { <app-product-card [p]="p" /> }
          </div>
          @if (result()!.totalPages > 1) {
            <div class="row pager">
              <button type="button" [disabled]="page() <= 1" (click)="page.set(page() - 1)">‹ Prev</button>
              <span>Page {{ page() }} of {{ result()!.totalPages }}</span>
              <button type="button" [disabled]="page() >= result()!.totalPages" (click)="page.set(page() + 1)">Next ›</button>
            </div>
          }
        }
      </section>
    </div>
  `,
  styles: `
    .layout { display: grid; grid-template-columns: 240px 1fr; gap: 20px; align-items: start; }
    .filters { position: sticky; top: 76px; }
    .bar { margin-bottom: 16px; }
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 16px; }
    .pager { justify-content: center; margin-top: 24px; }
    @media (max-width: 800px) { .layout { grid-template-columns: 1fr; } .filters { position: static; } }
  `
})
export class ProductList {
  private api = inject(Api);
  private route = inject(ActivatedRoute);
  private queryParams = toSignal(this.route.queryParamMap);

  categories = signal<Category[]>([]);
  brands = signal<Brand[]>([]);
  categoryOptions = computed(() => {
    const all = this.categories();
    const options: { id: string; label: string }[] = [];
    for (const parent of all.filter(c => !c.parentId)) {
      options.push({ id: parent.id, label: parent.name });
      for (const child of all.filter(c => c.parentId === parent.id)) {
        options.push({ id: child.id, label: '— ' + child.name });
      }
    }
    return options;
  });
  categoryName = computed(() => this.categories().find(c => c.id === this.categoryId())?.name ?? '');
  result = signal<PagedResult<ProductListItem> | null>(null);
  loading = signal(false);

  search = signal('');
  categoryId = signal('');
  brandId = signal('');
  minPrice = signal<number | null>(null);
  maxPrice = signal<number | null>(null);
  sort = signal('newest');
  page = signal(1);

  constructor() {
    this.api.categories().subscribe(c => this.categories.set(c.filter(x => x.isActive)));
    this.api.brands().subscribe(b => this.brands.set(b.filter(x => x.isActive)));

    effect(() => {
      const params = this.queryParams();
      this.search.set(params?.get('search') ?? '');
      this.categoryId.set(params?.get('category') ?? '');
      this.brandId.set(params?.get('brand') ?? '');
      this.page.set(1);
    });

    effect(() => {
      this.search(); this.categoryId(); this.brandId(); this.sort(); this.page();
      untracked(() => this.load());
    });
  }

  load() {
    this.loading.set(true);
    this.api.products({
      search: this.search() || undefined,
      categoryId: this.categoryId() || undefined,
      brandId: this.brandId() || undefined,
      minPrice: this.minPrice() ?? undefined,
      maxPrice: this.maxPrice() ?? undefined,
      sort: this.sort(),
      page: this.page(),
      pageSize: 12
    }).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  reset() {
    this.categoryId.set('');
    this.brandId.set('');
    this.minPrice.set(null);
    this.maxPrice.set(null);
    this.sort.set('newest');
    this.page.set(1);
  }
}
