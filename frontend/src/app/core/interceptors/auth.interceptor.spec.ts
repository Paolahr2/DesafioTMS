import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { PLATFORM_ID } from '@angular/core';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

describe('AuthInterceptor', () => {
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;

  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([AuthInterceptor])),
        provideHttpClientTesting(),
        AuthService,
        { provide: PLATFORM_ID, useValue: 'browser' }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should add Authorization header when token exists and request goes to API', () => {
    const mockToken = 'test-token-123';
    localStorage.setItem('auth_token', mockToken);

    httpClient.get(`${environment.apiUrl}/test`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/test`);
    expect(req.request.headers.has('Authorization')).toBe(true);
    expect(req.request.headers.get('Authorization')).toBe(`Bearer ${mockToken}`);
    req.flush({});
  });

  it('should not add Authorization header when token does not exist', () => {
    localStorage.removeItem('auth_token');

    httpClient.get(`${environment.apiUrl}/test`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/test`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should not add Authorization header for external URLs', () => {
    const mockToken = 'test-token-123';
    localStorage.setItem('auth_token', mockToken);
    const externalUrl = 'https://external-api.com/data';

    httpClient.get(externalUrl).subscribe();

    const req = httpMock.expectOne(externalUrl);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should add Authorization header to multiple API requests', () => {
    const mockToken = 'test-token-456';
    localStorage.setItem('auth_token', mockToken);

    httpClient.get(`${environment.apiUrl}/endpoint1`).subscribe();
    httpClient.post(`${environment.apiUrl}/endpoint2`, {}).subscribe();
    httpClient.put(`${environment.apiUrl}/endpoint3`, {}).subscribe();

    const req1 = httpMock.expectOne(`${environment.apiUrl}/endpoint1`);
    const req2 = httpMock.expectOne(`${environment.apiUrl}/endpoint2`);
    const req3 = httpMock.expectOne(`${environment.apiUrl}/endpoint3`);

    expect(req1.request.headers.get('Authorization')).toBe(`Bearer ${mockToken}`);
    expect(req2.request.headers.get('Authorization')).toBe(`Bearer ${mockToken}`);
    expect(req3.request.headers.get('Authorization')).toBe(`Bearer ${mockToken}`);

    req1.flush({});
    req2.flush({});
    req3.flush({});
  });

  it('should handle requests without token', () => {
    localStorage.removeItem('auth_token');

    httpClient.get(`${environment.apiUrl}/test`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/test`);
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should properly clone and modify the request', () => {
    const mockToken = 'clone-test-token';
    localStorage.setItem('auth_token', mockToken);

    const customHeaders = { 'Custom-Header': 'custom-value' };
    httpClient.get(`${environment.apiUrl}/test`, { headers: customHeaders }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/test`);
    
    // Should have both Authorization and custom headers
    expect(req.request.headers.get('Authorization')).toBe(`Bearer ${mockToken}`);
    expect(req.request.headers.get('Custom-Header')).toBe('custom-value');
    req.flush({});
  });
});
