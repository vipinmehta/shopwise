import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { ToastService } from '../../core/toast.service';
import { AdminUser, PagedResult } from '../../core/models';

@Component({
  selector: 'app-admin-users',
  imports: [DatePipe, FormsModule],
  template: `
    <div class="row spread">
      <h2>Users</h2>
      <form class="row" (ngSubmit)="page.set(1); load()">
        <input name="s" placeholder="Search name or email" [ngModel]="search()" (ngModelChange)="search.set($event)" style="width:240px" />
        <button type="submit">Search</button>
      </form>
    </div>
    <table>
      <thead><tr><th>Name</th><th>Email</th><th>Phone</th><th>Role</th><th>Joined</th><th>Status</th><th></th></tr></thead>
      <tbody>
        @for (u of result()?.items ?? []; track u.id) {
          <tr>
            <td>{{ u.name }}</td>
            <td>{{ u.email }}</td>
            <td>{{ u.phone || '—' }}</td>
            <td><span class="badge">{{ u.role }}</span></td>
            <td class="muted small">{{ u.createdAt | date: 'mediumDate' }}</td>
            <td><span class="badge" [class.ok]="u.isActive" [class.bad]="!u.isActive">{{ u.isActive ? 'Active' : 'Disabled' }}</span></td>
            <td class="right">
              @if (u.id !== auth.session()?.userId) {
                <button type="button" class="btn-sm" [class.btn-danger]="u.isActive" (click)="toggle(u)">{{ u.isActive ? 'Disable' : 'Enable' }}</button>
              }
            </td>
          </tr>
        } @empty { <tr><td colspan="7" class="muted">No users found.</td></tr> }
      </tbody>
    </table>
    @if ((result()?.totalPages ?? 0) > 1) {
      <div class="row" style="justify-content:center;margin-top:16px">
        <button type="button" [disabled]="page() <= 1" (click)="page.set(page() - 1); load()">‹ Prev</button>
        <span>Page {{ page() }} of {{ result()!.totalPages }}</span>
        <button type="button" [disabled]="page() >= result()!.totalPages" (click)="page.set(page() + 1); load()">Next ›</button>
      </div>
    }
  `
})
export class AdminUsers {
  private api = inject(Api);
  private toast = inject(ToastService);
  auth = inject(AuthService);

  result = signal<PagedResult<AdminUser> | null>(null);
  search = signal('');
  page = signal(1);

  constructor() { this.load(); }

  load() { this.api.adminUsers(this.search(), this.page()).subscribe(r => this.result.set(r)); }

  toggle(u: AdminUser) {
    this.api.setUserActive(u.id, !u.isActive).subscribe({
      next: () => { this.toast.ok(`${u.name} ${u.isActive ? 'disabled' : 'enabled'}`); this.load(); },
      error: e => this.toast.fromError(e)
    });
  }
}
