import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

/** Request payload for processing a payment. */
export interface ProcessPaymentRequest {
  /** The unique identifier of the order being paid for. */
  orderId: string;
  /** The total amount to charge. */
  amount: number;
  /** The payment method used (e.g., "UpiNow", "CashOnDelivery"). */
  paymentMethod: string;
}

/** Represents a payment record returned from the Payment API. */
export interface PaymentDto {
  /** The unique identifier of the payment record. */
  id: string;
  /** The unique identifier of the associated order. */
  orderId: string;
  /** The unique identifier of the customer who made the payment. */
  customerId: string;
  /** The total amount charged. */
  amount: number;
  /** The currency code (e.g., "INR"). */
  currency: string;
  /** The current status of the payment (e.g., "Pending", "Confirmed", "Refunded"). */
  status: string;
  /** The payment method used. */
  paymentMethod: string;
  /** ISO timestamp when the payment record was created. */
  createdAt: string;
  /** ISO timestamp when the payment was last processed or updated. */
  processedAt: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private base = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  /**
   * Processes a payment for an order.
   * @param req - The payment request payload.
   * @returns An observable that emits the created {@link PaymentDto}.
   */
  process(req: ProcessPaymentRequest) {
    return this.http.post<PaymentDto>(`${this.base}/process`, req);
  }

  /**
   * Retrieves the payment record for a specific order.
   * @param orderId - The order ID.
   * @returns An observable that emits the {@link PaymentDto}.
   */
  getByOrder(orderId: string) {
    return this.http.get<PaymentDto>(`${this.base}/order/${orderId}`);
  }

  /**
   * Retrieves all payment records for a specific customer.
   * @param customerId - The customer's user ID.
   * @returns An observable that emits an array of {@link PaymentDto}.
   */
  getByCustomer(customerId: string) {
    return this.http.get<PaymentDto[]>(`${this.base}/customer/${customerId}`);
  }

  /**
   * Retrieves all payment records (admin use).
   * @returns An observable that emits an array of {@link PaymentDto}.
   */
  getAll() {
    return this.http.get<PaymentDto[]>(this.base);
  }

  /**
   * Issues a refund for the payment associated with an order.
   * @param orderId - The order ID whose payment to refund.
   * @returns An observable that emits the updated {@link PaymentDto}.
   */
  refund(orderId: string) {
    return this.http.post<PaymentDto>(`${this.base}/order/${orderId}/refund`, {});
  }
}
