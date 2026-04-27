using System.Text;
using FoodFleet.Shared.Events.Orders;
using MassTransit;
using Notification.API.Application.Interfaces;

namespace Notification.API.Infrastructure.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly IEmailService _emailService;

    public OrderPlacedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg = context.Message;
        var body = BuildEmailBody(msg);

        await _emailService.SendAsync(
            msg.CustomerEmail,
            $"✅ Order #{msg.OrderId.ToString()[..8].ToUpper()} Placed Successfully!",
            body);
    }

    private static string BuildEmailBody(OrderPlacedEvent msg)
    {
        var sb = new StringBuilder();

        // Items rows
        var itemsHtml = new StringBuilder();
        foreach (var item in msg.Items)
        {
            var lineTotal = item.UnitPrice * item.Quantity;
            itemsHtml.Append($"""
                <tr>
                  <td style="padding:10px 8px;border-bottom:1px solid #f0f0f0;">{item.MenuItemName}{(string.IsNullOrWhiteSpace(item.Customizations) ? "" : $"<br/><small style='color:#888;'>{item.Customizations}</small>")}</td>
                  <td style="padding:10px 8px;border-bottom:1px solid #f0f0f0;text-align:center;">{item.Quantity}</td>
                  <td style="padding:10px 8px;border-bottom:1px solid #f0f0f0;text-align:right;">₹{item.UnitPrice:F2}</td>
                  <td style="padding:10px 8px;border-bottom:1px solid #f0f0f0;text-align:right;">₹{lineTotal:F2}</td>
                </tr>
                """);
        }

        var paymentIcon = msg.PaymentMethod == "CashOnDelivery" ? "💵" : "💳";
        var paymentLabel = msg.PaymentMethod == "CashOnDelivery" ? "Cash on Delivery" : "Card";

        sb.Append($"""
            <!DOCTYPE html>
            <html lang="en">
            <head><meta charset="UTF-8"/><meta name="viewport" content="width=device-width,initial-scale=1"/></head>
            <body style="margin:0;padding:0;background:#f5f5f5;font-family:'Segoe UI',Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0" style="background:#f5f5f5;padding:32px 0;">
                <tr><td align="center">
                  <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 2px 12px rgba(0,0,0,0.08);">

                    <!-- Header -->
                    <tr>
                      <td style="background:linear-gradient(135deg,#1b4332,#2d6a4f);padding:32px 40px;text-align:center;">
                        <div style="font-size:40px;margin-bottom:8px;">🍽️</div>
                        <h1 style="margin:0;color:#ffffff;font-size:24px;font-weight:700;">Order Confirmed!</h1>
                        <p style="margin:8px 0 0;color:rgba(255,255,255,0.9);font-size:14px;">Your order has been placed successfully</p>
                      </td>
                    </tr>

                    <!-- Brand strip -->
                    <tr>
                      <td style="background:#e9c46a;padding:10px 40px;text-align:center;">
                        <span style="font-size:13px;font-weight:700;color:#2d6a4f;letter-spacing:1px;text-transform:uppercase;">🌿 CraveKart</span>
                      </td>
                    </tr>

                    <!-- Order ID & Date -->
                    <tr>
                      <td style="padding:24px 40px 0;">
                        <table width="100%" cellpadding="0" cellspacing="0">
                          <tr>
                            <td style="background:#f0f7f2;border:1px solid #c8e6d0;border-radius:8px;padding:16px 20px;">
                              <table width="100%">
                                <tr>
                                  <td>
                                    <div style="font-size:12px;color:#888;text-transform:uppercase;letter-spacing:0.5px;">Order ID</div>
                                    <div style="font-size:16px;font-weight:700;color:#2d6a4f;margin-top:2px;">#{msg.OrderId.ToString()[..8].ToUpper()}</div>
                                  </td>
                                  <td align="right">
                                    <div style="font-size:12px;color:#888;text-transform:uppercase;letter-spacing:0.5px;">Placed On</div>
                                    <div style="font-size:14px;font-weight:600;color:#333;margin-top:2px;">{msg.PlacedAt:dd MMM yyyy, hh:mm tt} UTC</div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>

                    <!-- Items Table -->
                    <tr>
                      <td style="padding:24px 40px 0;">
                        <h2 style="margin:0 0 12px;font-size:16px;color:#333;font-weight:700;">🛒 Items Ordered</h2>
                        <table width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #f0f0f0;border-radius:8px;overflow:hidden;">
                          <thead>
                            <tr style="background:#f8f8f8;">
                              <th style="padding:10px 8px;text-align:left;font-size:12px;color:#666;font-weight:600;text-transform:uppercase;">Item</th>
                              <th style="padding:10px 8px;text-align:center;font-size:12px;color:#666;font-weight:600;text-transform:uppercase;">Qty</th>
                              <th style="padding:10px 8px;text-align:right;font-size:12px;color:#666;font-weight:600;text-transform:uppercase;">Price</th>
                              <th style="padding:10px 8px;text-align:right;font-size:12px;color:#666;font-weight:600;text-transform:uppercase;">Total</th>
                            </tr>
                          </thead>
                          <tbody>
                            {itemsHtml}
                          </tbody>
                        </table>
                      </td>
                    </tr>

                    <!-- Price Breakdown -->
                    <tr>
                      <td style="padding:16px 40px 0;">
                        <table width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #f0f0f0;border-radius:8px;overflow:hidden;">
                          <tr>
                            <td style="padding:10px 16px;font-size:14px;color:#555;">Subtotal</td>
                            <td style="padding:10px 16px;font-size:14px;color:#333;text-align:right;">₹{msg.SubTotal:F2}</td>
                          </tr>
                          <tr style="background:#fafafa;">
                            <td style="padding:10px 16px;font-size:14px;color:#555;">Delivery Fee</td>
                            <td style="padding:10px 16px;font-size:14px;color:#333;text-align:right;">₹{msg.DeliveryFee:F2}</td>
                          </tr>
                          <tr>
                            <td style="padding:10px 16px;font-size:14px;color:#555;">Tax (5%)</td>
                            <td style="padding:10px 16px;font-size:14px;color:#333;text-align:right;">₹{msg.Tax:F2}</td>
                          </tr>
                          <tr style="background:#f0f7f2;border-top:2px solid #2d6a4f;">
                            <td style="padding:12px 16px;font-size:16px;font-weight:700;color:#333;">Total</td>
                            <td style="padding:12px 16px;font-size:16px;font-weight:700;color:#2d6a4f;text-align:right;">₹{msg.TotalAmount:F2}</td>
                          </tr>
                        </table>
                      </td>
                    </tr>

                    <!-- Delivery & Payment Info -->
                    <tr>
                      <td style="padding:16px 40px 0;">
                        <table width="100%" cellpadding="0" cellspacing="0">
                          <tr>
                            <td width="48%" style="background:#f8f8f8;border-radius:8px;padding:16px;vertical-align:top;">
                              <div style="font-size:12px;color:#888;text-transform:uppercase;letter-spacing:0.5px;margin-bottom:6px;">📍 Delivery Address</div>
                              <div style="font-size:14px;color:#333;line-height:1.5;">{msg.DeliveryAddress}</div>
                            </td>
                            <td width="4%"></td>
                            <td width="48%" style="background:#f8f8f8;border-radius:8px;padding:16px;vertical-align:top;">
                              <div style="font-size:12px;color:#888;text-transform:uppercase;letter-spacing:0.5px;margin-bottom:6px;">💳 Payment Method</div>
                              <div style="font-size:14px;color:#333;">{paymentIcon} {paymentLabel}</div>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                      <td style="padding:32px 40px;text-align:center;">
                        <p style="margin:0 0 8px;font-size:14px;color:#555;">Thank you for ordering with <strong style="color:#2d6a4f;">CraveKart</strong>! 🎉</p>
                        <p style="margin:0;font-size:13px;color:#999;">You can track your order in the app. We'll keep you updated on every step.</p>
                      </td>
                    </tr>

                    <!-- Bottom bar -->
                    <tr>
                      <td style="background:#f8f8f8;padding:16px 40px;text-align:center;border-top:1px solid #eee;">
                        <p style="margin:0;font-size:12px;color:#aaa;">© {DateTime.UtcNow.Year} CraveKart. All rights reserved.</p>
                      </td>
                    </tr>

                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """);

        return sb.ToString();
    }
}
