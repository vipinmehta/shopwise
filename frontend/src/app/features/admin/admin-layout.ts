import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-layout',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <h1>Admin</h1>
    <nav class="row tabs">
      <a routerLink="products" routerLinkActive="on">Products</a>
      <a routerLink="catalog" routerLinkActive="on">Categories &amp; Brands</a>
      <a routerLink="inventory" routerLinkActive="on">Inventory</a>
      <a routerLink="orders" routerLinkActive="on">Orders</a>
      <a routerLink="users" routerLinkActive="on">Users</a>
      <a routerLink="coupons" routerLinkActive="on">Promotions &amp; Coupons</a>
    </nav>
    <router-outlet />
  `,
  styles: `
    .tabs { border-bottom: 1px solid var(--border); margin-bottom: 20px; gap: 4px; }
    .tabs a { padding: 8px 14px; color: var(--text); border-bottom: 3px solid transparent; }
    .tabs a:hover { text-decoration: none; background: #eef0f4; }
    .tabs a.on { border-bottom-color: var(--primary); color: var(--primary); font-weight: 600; }
  `
})
export class AdminLayout {}
