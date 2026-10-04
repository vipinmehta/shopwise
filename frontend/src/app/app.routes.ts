import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/guards';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/catalog/product-list').then(m => m.ProductList), title: 'Shopwise' },
  { path: 'product/:id', loadComponent: () => import('./features/catalog/product-detail').then(m => m.ProductDetail), title: 'Product' },
  { path: 'login', loadComponent: () => import('./features/auth/login').then(m => m.Login), title: 'Sign in' },
  { path: 'register', loadComponent: () => import('./features/auth/register').then(m => m.Register), title: 'Create account' },
  { path: 'cart', canActivate: [authGuard], loadComponent: () => import('./features/cart/cart').then(m => m.Cart), title: 'Cart' },
  { path: 'wishlist', canActivate: [authGuard], loadComponent: () => import('./features/cart/wishlist').then(m => m.Wishlist), title: 'Wishlist' },
  { path: 'checkout', canActivate: [authGuard], loadComponent: () => import('./features/checkout/checkout').then(m => m.Checkout), title: 'Checkout' },
  { path: 'orders', canActivate: [authGuard], loadComponent: () => import('./features/orders/order-list').then(m => m.OrderList), title: 'Orders' },
  { path: 'orders/:id', canActivate: [authGuard], loadComponent: () => import('./features/orders/order-detail').then(m => m.OrderDetail), title: 'Order' },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin-layout').then(m => m.AdminLayout),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'products' },
      { path: 'products', loadComponent: () => import('./features/admin/admin-products').then(m => m.AdminProducts), title: 'Admin · Products' },
      { path: 'catalog', loadComponent: () => import('./features/admin/admin-catalog').then(m => m.AdminCatalog), title: 'Admin · Catalog' },
      { path: 'inventory', loadComponent: () => import('./features/admin/admin-inventory').then(m => m.AdminInventory), title: 'Admin · Inventory' },
      { path: 'orders', loadComponent: () => import('./features/admin/admin-orders').then(m => m.AdminOrders), title: 'Admin · Orders' },
      { path: 'users', loadComponent: () => import('./features/admin/admin-users').then(m => m.AdminUsers), title: 'Admin · Users' },
      { path: 'coupons', loadComponent: () => import('./features/admin/admin-coupons').then(m => m.AdminCoupons), title: 'Admin · Coupons' }
    ]
  },
  { path: '**', redirectTo: '' }
];
