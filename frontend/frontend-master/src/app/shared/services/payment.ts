import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Payment {
  leaseId?: number;
  amount?: number;
  paymentDate?: string;
  status?: string;
  tenantName?: string;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private apiBaseUrl = environment.apiBaseUrl;
  private baseUrl = environment.baseUrl;
  private updatePaymentStatusUrl = `${environment.baseUrl}/Tenant/UpdatePaymentStatus`;

  constructor(private http: HttpClient) {}

  getPaymentsByLeaseId(leaseId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(
      `${this.baseUrl}/Lease/${leaseId}/Payments`
    );
  }

  getPaymentById(paymentId: number): Observable<Payment> {
    return this.http.get<Payment>(`${this.baseUrl}/${paymentId}`);
  }

  updatePaymentStatus(leaseId: number): Observable<any> {
    console.log('Updating payment status for leaseId:', leaseId);
    return this.http.put(`${this.updatePaymentStatusUrl}/${leaseId}`, {});
  }

  getAllPayments(): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.baseUrl}/Payments`);
  }
}
