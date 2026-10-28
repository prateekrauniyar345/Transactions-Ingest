import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App API connection', () => {
  let http: HttpTestingController;
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());

  it.each(['2026-10-27T08:30:00Z', '2026-01-15T08:30:00Z'])(
    'displays a revoked transaction in local time: %s', (timestamp) => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Loading transactions');
    const request = http.expectOne('/api/transactions');
    expect(request.request.method).toBe('GET');
    request.flush([{
      id: 2, sourceSystem: 'Store Uptown', transactionId: 100002,
      cardNumber: '***2222', locationCode: 'UPTOWN', productName: 'USB Cable',
      amount: 14.99, transactionTime: timestamp, status: 'Revoked',
    }]);
    fixture.detectChanges();
    const row = fixture.nativeElement.querySelector('tbody tr');
    expect(row.textContent).toContain('USB Cable');
    expect(row.textContent).toContain('Revoked');
    const local = new Date(timestamp);
    const pad = (value: number) => String(value).padStart(2, '0');
    const expected = `${local.getFullYear()}-${pad(local.getMonth() + 1)}-${pad(local.getDate())} ${pad(local.getHours())}:${pad(local.getMinutes())}:${pad(local.getSeconds())}`;
    expect(row.textContent).toContain(expected);
    expect(fixture.nativeElement.querySelector('caption').textContent)
      .toContain(Intl.DateTimeFormat().resolvedOptions().timeZone);
    expect(fixture.nativeElement.querySelectorAll('tbody tr').length).toBe(1);
  });

  it('shows an empty state for an empty response', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/transactions').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No transactions yet');
  });

  it('shows an error and allows retrying', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/transactions').flush({}, { status: 500, statusText: 'Error' });
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]').textContent)
      .toContain('Could not load transactions');
    fixture.nativeElement.querySelector('.refresh-button').click();
    http.expectOne('/api/transactions').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });
  it('switches to Admin and returns to Transactions without refetching', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    http.expectOne('/api/transactions').flush([]);
    fixture.detectChanges();
    const tabs = fixture.nativeElement.querySelectorAll('.nav-inner button');
    tabs[1].click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Admin');
    expect(fixture.nativeElement.textContent).toContain('Administrative tools will be available');
    expect(tabs[1].getAttribute('aria-current')).toBe('page');
    tabs[0].click();
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1').textContent).toBe('Transactions');
    expect(fixture.nativeElement.textContent).toContain('No transactions yet');
    http.expectNone('/api/transactions');
  });
});
