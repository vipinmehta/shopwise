import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { API_URL } from './config';
import { AuthResponse } from './models';

const STORAGE_KEY = 'ecom_auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  readonly session = signal<AuthResponse | null>(this.load());
  readonly isLoggedIn = computed(() => this.session() !== null);
  readonly isAdmin = computed(() => this.session()?.role === 'Admin');
  readonly token = computed(() => this.session()?.token ?? null);

  private load(): AuthResponse | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as AuthResponse) : null;
    } catch {
      return null;
    }
  }

  private store(res: AuthResponse) {
    this.session.set(res);
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(res)); } catch { /* ignore */ }
  }

  register(name: string, email: string, password: string, phone?: string) {
    return this.http.post<AuthResponse>(`${API_URL}/auth/register`, { name, email, password, phone }).pipe(tap(r => this.store(r)));
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${API_URL}/auth/login`, { email, password }).pipe(tap(r => this.store(r)));
  }

  requestOtp(phone: string) {
    return this.http.post(`${API_URL}/auth/otp/request`, { phone });
  }

  verifyOtp(phone: string, code: string, name?: string) {
    return this.http.post<AuthResponse>(`${API_URL}/auth/otp/verify`, { phone, code, name }).pipe(tap(r => this.store(r)));
  }

  googleLogin(idToken: string) {
    return this.http.post<AuthResponse>(`${API_URL}/auth/google`, { idToken }).pipe(tap(r => this.store(r)));
  }

  logout() {
    this.session.set(null);
    try { localStorage.removeItem(STORAGE_KEY); } catch { /* ignore */ }
    this.router.navigateByUrl('/');
  }
}
