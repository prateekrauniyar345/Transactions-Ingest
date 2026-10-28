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

  it('loads and displays a revoked backend transaction', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Loading transactions');
    const request = http.expectOne('/api/transactions');
    expect(request.request.method).toBe('GET');
    request.flush([{
      id: 2, sourceSystem: 'Store Uptown', transactionId: 100002,
      cardNumber: '***2222', locationCode: 'UPTOWN', productName: 'USB Cable',
      amount: 14.99, transactionTime: '2026-10-27T08:30:00Z', status: 'Revoked',
    }]);
    fixture.detectChanges();
    const row = fixture.nativeElement.querySelector('tbody tr');
    expect(row.textContent).toContain('USB Cable');
    expect(row.textContent).toContain('Revoked');
    expect(row.textContent).toContain('2026-10-27 08:30:00');
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
    fixture.nativeElement.querySelector('button').click();
    http.expectOne('/api/transactions').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });
});
