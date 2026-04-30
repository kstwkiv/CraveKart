import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export interface ProcessPaymentRequest {
  orderId: string;
  amount: number;
  paymentMethod: string;
}

export interface PaymentDto {
  id: string;
  orderId: string;
  customerId: string;
  amount: number;
  currency: string;
  status: string;
  paymentMethod: string;
  createdAt: string;
  processedAt: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private base = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  process(req: ProcessPaymentRequest) {
    return this.http.post<PaymentDto>(`${this.base}/process`, req);
  }

  getByOrder(orderId: string) {
    return this.http.get<PaymentDto>(`${this.base}/order/${orderId}`);
  }

  getByCustomer(customerId: string) {
    return this.http.get<PaymentDto[]>(`${this.base}/customer/${customerId}`);
  }

  getAll() {
    return this.http.get<PaymentDto[]>(this.base);
  }

  refund(orderId: string) {
    return this.http.post<PaymentDto>(`${this.base}/order/${orderId}/refund`, {});
  }
}
