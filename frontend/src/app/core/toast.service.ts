import { Injectable, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

export interface Toast { id: number; text: string; kind: 'ok' | 'error'; }

@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<Toast[]>([]);
  private nextId = 1;

  ok(text: string) { this.push(text, 'ok'); }
  error(text: string) { this.push(text, 'error'); }

  fromError(err: unknown, fallback = 'Something went wrong') {
    const e = err as HttpErrorResponse;
    this.error(e?.error?.message ?? fallback);
  }

  private push(text: string, kind: Toast['kind']) {
    const id = this.nextId++;
    this.toasts.update(t => [...t, { id, text, kind }]);
    setTimeout(() => this.toasts.update(t => t.filter(x => x.id !== id)), 3500);
  }
}
