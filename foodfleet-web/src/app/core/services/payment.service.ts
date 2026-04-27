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
  amount: number;
  status: string;
  paymentMethod: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private base = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  process(req: ProcessPaymentRequest) {
    return this.http.post<PaymentDto>(`${this.base}/process`, req);
  }
}
