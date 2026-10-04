import { Component, computed, inject, signal } from '@angular/core';
import { Api } from './core/api.service';
import { Category } from './core/models';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from './core/auth.service';
import { CartService } from './core/cart.service';
import { ToastService } from './core/toast.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  auth = inject(AuthService);
  cart = inject(CartService);
  toast = inject(ToastService);
  private router = inject(Router);
  private api = inject(Api);

  search = signal('');
  private categories = signal<Category[]>([]);
  menu = computed(() => {
    const all = this.categories().filter(c => c.isActive);
    return all.filter(c => !c.parentId).map(parent => ({
      parent,
      children: all.filter(c => c.parentId === parent.id).map(c => ({ id: c.id, label: c.name.replace(/^(Men's|Women's) /, '') }))
    }));
  });

  constructor() {
    this.api.categories().subscribe(c => this.categories.set(c));
  }

  doSearch() {
    this.router.navigate(['/'], { queryParams: { search: this.search() || null } });
  }
}
