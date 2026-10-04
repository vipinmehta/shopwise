import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { API_URL } from './config';
import {
  Address, AdminUser, Brand, Cart, Category, Coupon, Order, PagedResult,
  ProductDetail, ProductListItem, Review, ValidateCouponResponse
} from './models';

export interface ProductQuery {
  search?: string; categoryId?: string; brandId?: string; minPrice?: number; maxPrice?: number;
  sort?: string; page?: number; pageSize?: number;
}

function toParams(obj: Record<string, unknown>): HttpParams {
  let p = new HttpParams();
  for (const [k, v] of Object.entries(obj)) {
    if (v !== undefined && v !== null && v !== '') p = p.set(k, String(v));
  }
  return p;
}

@Injectable({ providedIn: 'root' })
export class Api {
  private http = inject(HttpClient);

  // catalog
  categories() { return this.http.get<Category[]>(`${API_URL}/categories`); }
  createCategory(b: Partial<Category>) { return this.http.post<Category>(`${API_URL}/categories`, b); }
  updateCategory(id: string, b: Partial<Category>) { return this.http.put<Category>(`${API_URL}/categories/${id}`, b); }
  deleteCategory(id: string) { return this.http.delete(`${API_URL}/categories/${id}`); }

  brands() { return this.http.get<Brand[]>(`${API_URL}/brands`); }
  createBrand(b: Partial<Brand>) { return this.http.post<Brand>(`${API_URL}/brands`, b); }
  updateBrand(id: string, b: Partial<Brand>) { return this.http.put<Brand>(`${API_URL}/brands/${id}`, b); }
  deleteBrand(id: string) { return this.http.delete(`${API_URL}/brands/${id}`); }

  products(q: ProductQuery) { return this.http.get<PagedResult<ProductListItem>>(`${API_URL}/products`, { params: toParams({ ...q }) }); }
  product(id: string) { return this.http.get<ProductDetail>(`${API_URL}/products/${id}`); }
  createProduct(b: unknown) { return this.http.post<ProductDetail>(`${API_URL}/products`, b); }
  updateProduct(id: string, b: unknown) { return this.http.put<ProductDetail>(`${API_URL}/products/${id}`, b); }
  deleteProduct(id: string) { return this.http.delete(`${API_URL}/products/${id}`); }
  adjustStock(id: string, size: string, delta: number) { return this.http.post(`${API_URL}/products/${id}/stock`, { size, delta }); }

  reviews(productId: string) { return this.http.get<Review[]>(`${API_URL}/products/${productId}/reviews`); }
  addReview(productId: string, rating: number, comment: string) {
    return this.http.post<Review>(`${API_URL}/products/${productId}/reviews`, { rating, comment });
  }

  // cart
  cart() { return this.http.get<Cart>(`${API_URL}/cart`); }
  addToCart(productId: string, size: string | null, quantity: number) { return this.http.post<Cart>(`${API_URL}/cart/items`, { productId, size, quantity }); }
  updateCartItem(productId: string, size: string | null | undefined, quantity: number) { return this.http.put<Cart>(`${API_URL}/cart/items`, { productId, size, quantity }); }
  removeCartItem(productId: string, size?: string | null) { return this.http.delete<Cart>(`${API_URL}/cart/items`, { params: toParams({ productId, size }) }); }
  saveForLater(productId: string, size?: string | null) { return this.http.post<Cart>(`${API_URL}/cart/save-for-later`, null, { params: toParams({ productId, size }) }); }
  moveToCart(productId: string, size?: string | null) { return this.http.post<Cart>(`${API_URL}/cart/move-to-cart`, null, { params: toParams({ productId, size }) }); }
  toggleWishlist(productId: string) { return this.http.post<Cart>(`${API_URL}/cart/wishlist/${productId}`, null); }

  // addresses
  addresses() { return this.http.get<Address[]>(`${API_URL}/addresses`); }
  createAddress(a: Partial<Address>) { return this.http.post<Address>(`${API_URL}/addresses`, a); }
  updateAddress(id: string, a: Partial<Address>) { return this.http.put<Address>(`${API_URL}/addresses/${id}`, a); }
  deleteAddress(id: string) { return this.http.delete(`${API_URL}/addresses/${id}`); }

  // coupons
  coupons() { return this.http.get<Coupon[]>(`${API_URL}/coupons`); }
  createCoupon(c: unknown) { return this.http.post<Coupon>(`${API_URL}/coupons`, c); }
  updateCoupon(id: string, c: unknown) { return this.http.put<Coupon>(`${API_URL}/coupons/${id}`, c); }
  deleteCoupon(id: string) { return this.http.delete(`${API_URL}/coupons/${id}`); }
  validateCoupon(code: string, orderValue: number) { return this.http.post<ValidateCouponResponse>(`${API_URL}/coupons/validate`, { code, orderValue }); }

  // checkout + orders
  checkout(addressId: string, couponCode: string | null) { return this.http.post<Order>(`${API_URL}/checkout`, { addressId, couponCode }); }
  orders() { return this.http.get<Order[]>(`${API_URL}/orders`); }
  order(id: string) { return this.http.get<Order>(`${API_URL}/orders/${id}`); }
  cancelOrder(id: string, reason: string) { return this.http.post<Order>(`${API_URL}/orders/${id}/cancel`, { reason }); }
  returnOrder(id: string, reason: string) { return this.http.post<Order>(`${API_URL}/orders/${id}/return`, { reason }); }
  adminOrders(page: number, status?: string) { return this.http.get<PagedResult<Order>>(`${API_URL}/orders/admin/all`, { params: toParams({ page, pageSize: 20, status }) }); }
  adminUpdateOrderStatus(id: string, status: string, note?: string) { return this.http.put<Order>(`${API_URL}/orders/${id}/status`, { status, note }); }

  // admin users
  adminUsers(search: string, page: number) { return this.http.get<PagedResult<AdminUser>>(`${API_URL}/admin/users`, { params: toParams({ search, page, pageSize: 20 }) }); }
  setUserActive(id: string, isActive: boolean) {
    return this.http.put(`${API_URL}/admin/users/${id}/active`, isActive, { headers: { 'Content-Type': 'application/json' } });
  }
}
