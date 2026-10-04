import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { ToastService } from '../../core/toast.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="wrap card">
      <h1>Create account</h1>
      <form (ngSubmit)="submit()" class="stack">
        <div class="field"><label for="name">Full name</label><input id="name" name="name" [(ngModel)]="name" required /></div>
        <div class="field"><label for="email">Email</label><input id="email" name="email" type="email" [(ngModel)]="email" required /></div>
        <div class="field"><label for="phone">Mobile (optional)</label><input id="phone" name="phone" [(ngModel)]="phone" /></div>
        <div class="field"><label for="password">Password (min 6 characters)</label><input id="password" name="password" type="password" minlength="6" [(ngModel)]="password" required /></div>
        <button class="btn-primary" type="submit" [disabled]="busy()">Create account</button>
        <p class="small muted">Already registered? <a routerLink="/login">Sign in</a></p>
      </form>
    </div>
  `,
  styles: `.wrap { max-width: 440px; margin: 0 auto; }`
})
export class Register {
  private auth = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  busy = signal(false);
  name = '';
  email = '';
  phone = '';
  password = '';

  submit() {
    if (this.password.length < 6) { this.toast.error('Password must be at least 6 characters'); return; }
    this.busy.set(true);
    this.auth.register(this.name, this.email, this.password, this.phone || undefined).subscribe({
      next: () => { this.busy.set(false); this.router.navigateByUrl('/'); },
      error: e => { this.busy.set(false); this.toast.fromError(e); }
    });
  }
}
