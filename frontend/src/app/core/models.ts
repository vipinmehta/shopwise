export interface AuthResponse { token: string; userId: string; name: string; email: string; role: string; }

export interface Category { id: string; name: string; slug: string; image?: string | null; parentId?: string | null; isActive: boolean; }
export interface Brand { id: string; name: string; slug: string; logo?: string | null; isActive: boolean; }

export interface ProductVariant { size: string; color?: string | null; sku: string; stock: number; }
export interface SizeChartRow { size: string; measurements: Record<string, string>; }

export interface ProductListItem {
  id: string; name: string; slug: string; price: number; discountPrice?: number | null;
  thumbnail?: string | null; averageRating: number; reviewCount: number; inStock: boolean;
}

export interface ProductDetail {
  id: string; name: string; slug: string; description: string; categoryId: string; brandId: string;
  price: number; discountPrice?: number | null; images: string[]; videos: string[];
  variants: ProductVariant[]; sizeChart: SizeChartRow[]; tags: string[];
  averageRating: number; reviewCount: number; isActive: boolean;
}

export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number; }

export interface Review { id: string; userName: string; rating: number; comment?: string | null; createdAt: string; }

export interface CartItem { productId: string; productName: string; thumbnail?: string | null; size?: string | null; quantity: number; unitPrice: number; availableStock: number; }
export interface Cart { items: CartItem[]; savedForLater: CartItem[]; wishlistProductIds: string[]; subtotal: number; }

export interface Address {
  id: string; fullName: string; phone: string; line1: string; line2?: string | null;
  city: string; state: string; pincode: string; country: string; isDefault: boolean;
}

export interface Coupon {
  id: string; code: string; type: string; value: number; minOrderValue: number;
  expiresAt?: string | null; usageLimit?: number | null; usedCount: number; isActive: boolean;
}
export interface ValidateCouponResponse { valid: boolean; discount: number; message?: string | null; }

export interface OrderItem { productId: string; productName: string; image?: string | null; size?: string | null; quantity: number; unitPrice: number; }
export interface TrackingEvent { status: string; timestamp: string; note?: string | null; }
export interface Order {
  id: string; orderNumber: string; items: OrderItem[];
  shippingAddress: Omit<Address, 'id' | 'isDefault'>;
  couponCode?: string | null; subtotal: number; discount: number; total: number;
  paymentStatus: string; status: string; createdAt: string; trackingHistory: TrackingEvent[];
}

export interface AdminUser { id: string; name: string; email: string; phone?: string | null; role: string; isActive: boolean; createdAt: string; }
