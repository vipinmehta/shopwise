import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/auth.service';
import { ToastService } from '../../core/toast.service';

type Mode = 'email' | 'otp' | 'google';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="wrap card">
      <h1>Sign in</h1>
      <div class="tabs row">
        <button type="button" [class.btn-primary]="mode() === 'email'" (click)="mode.set('email')">Email</button>
        <button type="button" [class.btn-primary]="mode() === 'otp'" (click)="mode.set('otp')">Mobile OTP</button>
        <button type="button" [class.btn-primary]="mode() === 'google'" (click)="mode.set('google')">Google</button>
      </div>

      @switch (mode()) {
        @case ('email') {
          <form (ngSubmit)="loginEmail()" class="stack">
            <div class="field"><label for="email">Email</label><input id="email" name="email" type="email" [(ngModel)]="email" required /></div>
            <div class="field"><label for="password">Password</label><input id="password" name="password" type="password" [(ngModel)]="password" required /></div>
            <button class="btn-primary" type="submit" [disabled]="busy()">Sign in</button>
            <p class="small muted">No account? <a routerLink="/register">Create one</a></p>
          </form>
        }
        @case ('otp') {
          <form (ngSubmit)="otpSent() ? verifyOtp() : requestOtp()" class="stack">
            <div class="field"><label for="phone">Mobile number</label><input id="phone" name="phone" [(ngModel)]="phone" placeholder="e.g. 9876543210" required [disabled]="otpSent()" /></div>
            @if (otpSent()) {
              <div class="field"><label for="otp">Enter OTP</label><input id="otp" name="otp" [(ngModel)]="otp" maxlength="6" required /></div>
              <div class="field"><label for="oname">Your name (new users)</label><input id="oname" name="oname" [(ngModel)]="otpName" /></div>
              <p class="small muted">Dev mode: the OTP is printed in the API console log instead of being sent by SMS.</p>
            }
            <button class="btn-primary" type="submit" [disabled]="busy()">{{ otpSent() ? 'Verify & sign in' : 'Send OTP' }}</button>
          </form>
        }
        @case ('google') {
          <form (ngSubmit)="loginGoogle()" class="stack">
            <p class="small muted">Dev mode: Google sign-in is simulated. Enter the Google account details to sign in as; a real Google button replaces this once a Google client ID is configured.</p>
            <div class="field"><label for="gemail">Google email</label><input id="gemail" name="gemail" type="email" [(ngModel)]="gEmail" required /></div>
            <div class="field"><label for="gname">Name</label><input id="gname" name="gname" [(ngModel)]="gName" /></div>
            <button class="btn-primary" type="submit" [disabled]="busy()">Continue with Google</button>
          </form>
        }
      }
    </div>
  `,
  styles: `.wrap { max-width: 440px; margin: 0 auto; } .tabs { margin-bottom: 16px; }`
})
export class Login {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toast = inject(ToastService);

  mode = signal<Mode>('email');
  busy = signal(false);
  otpSent = signal(false);

  email = '';
  password = '';
  phone = '';
  otp = '';
  otpName = '';
  gEmail = '';
  gName = '';

  private done() {
    this.busy.set(false);
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    this.router.navigateByUrl(returnUrl || (this.auth.isAdmin() ? '/admin' : '/'));
  }

  private fail(e: unknown) {
    this.busy.set(false);
    this.toast.fromError(e);
  }

  loginEmail() {
    this.busy.set(true);
    this.auth.login(this.email, this.password).subscribe({ next: () => this.done(), error: e => this.fail(e) });
  }

  requestOtp() {
    this.busy.set(true);
    this.auth.requestOtp(this.phone).subscribe({
      next: () => { this.busy.set(false); this.otpSent.set(true); this.toast.ok('OTP sent'); },
      error: e => this.fail(e)
    });
  }

  verifyOtp() {
    this.busy.set(true);
    this.auth.verifyOtp(this.phone, this.otp, this.otpName || undefined).subscribe({ next: () => this.done(), error: e => this.fail(e) });
  }

  loginGoogle() {
    this.busy.set(true);
    const payload = btoa(JSON.stringify({ sub: 'dev-' + this.gEmail, email: this.gEmail, name: this.gName || this.gEmail }));
    this.auth.googleLogin(payload).subscribe({ next: () => this.done(), error: e => this.fail(e) });
  }
}
