import { CurrencyPipe, DatePipe, isPlatformBrowser } from '@angular/common';
import { Component, DestroyRef, inject, OnInit, PLATFORM_ID, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Transaction } from './models/transaction';
import { TransactionService } from './services/transaction.service';

@Component({
  selector: 'app-root',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App implements OnInit {
  private readonly service = inject(TransactionService);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly localTimezone = signal('');
  protected readonly currentYear = new Date().getFullYear();
  protected readonly transactions = signal<Transaction[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal('');

  ngOnInit() {
    // Prerendering the app must not require a running backend.
    if (isPlatformBrowser(this.platformId)) {
      this.localTimezone.set(Intl.DateTimeFormat().resolvedOptions().timeZone);
      this.refresh();
    }
  }

  protected refresh() {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set('');

    this.service.getTransactions().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (transactions) => {
        this.transactions.set(transactions);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load transactions. Check that the backend is running and try again.');
        this.loading.set(false);
      },
    });
  }
}
