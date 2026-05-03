// 'namespace' — declares the logical scope for this class; prevents name collisions across assemblies
namespace Notification.API.Infrastructure.Services;

/// <summary>
/// Shared HTML email layout for all CraveKart transactional emails.
/// Brand colours: primary green #2d6a4f, accent gold #e9c46a
/// </summary>
// 'public' — access modifier: this class is visible to any other namespace or assembly
// 'static' — the class belongs to the type itself; no instance is ever created
//   Static classes are idiomatic for utility/helper types that only contain stateless methods or constants
public static class EmailTemplates
{
    // 'private' — accessible only within this class; hides implementation details from callers
    // 'const' — a compile-time constant; the value is baked into the IL at every usage site
    // 'string' — a reference type representing an immutable sequence of Unicode characters
    private const string Brand   = "CraveKart";
    private const string Primary = "#2d6a4f";
    private const string Accent  = "#e9c46a";
    private const string BgPage  = "#f0f4f1";
    private const string BgCard  = "#ffffff";

    // ── Public builders ──────────────────────────────────────────────────────

    /// <summary>Welcome email sent after registration.</summary>
    // 'public static' — callable without an instance; part of the class's public API
    // 'string' return type — the method produces an HTML string
    // Expression-bodied member (=>) — concise single-expression method body; equivalent to { return Wrap(...); }
    public static string Welcome(string fullName) => Wrap(
        headerEmoji: "🎉",
        // String interpolation ($"...") — embeds expressions inside a string literal at runtime
        headerBg: $"linear-gradient(135deg,{Primary},{Accent})",
        title: $"Welcome to {Brand}, {FirstName(fullName)}!",
        subtitle: "Your food adventure starts now",
        // Raw string literal (""" ... """) — multi-line string without escape sequences; introduced in C# 11
        body: $"""
            <p style="{BodyText}">Hi <strong>{fullName}</strong>,</p>
            <p style="{BodyText}">We're thrilled to have you on board! {Brand} connects you with the best restaurants in your city — from biryani to burgers, it's all just a tap away.</p>
            {InfoBox("🍽️", "Browse hundreds of restaurants, track your order live, and enjoy fast delivery right to your door.")}
            <p style="{BodyText}">Ready to explore?</p>
            {Cta("Browse Restaurants", "http://localhost:4200/restaurants")}
        """);

    /// <summary>Sign-in security alert.</summary>
    public static string LoginAlert(string fullName, string loggedInAt) => Wrap(
        headerEmoji: "🔐",
        headerBg: $"linear-gradient(135deg,#1b4332,{Primary})",
        title: "New Sign-In Detected",
        subtitle: $"Your {Brand} account was accessed",
        body: $"""
            <p style="{BodyText}">Hi <strong>{fullName}</strong>,</p>
            <p style="{BodyText}">A new sign-in was detected on your {Brand} account.</p>
            {InfoBox("🕐", $"<strong>Time:</strong> {loggedInAt} IST")}
            <p style="{BodyText}">If this was you, no action is needed. If you don't recognise this activity, please reset your password immediately.</p>
            {Cta("Reset My Password", "http://localhost:4200/auth/forgot-password", danger: true)}
        """);

    /// <summary>Password reset OTP.</summary>
    public static string PasswordResetOtp(string otp) => Wrap(
        headerEmoji: "🔑",
        headerBg: $"linear-gradient(135deg,#1b4332,{Primary})",
        title: "Password Reset OTP",
        subtitle: $"Your {Brand} account",
        body: $"""
            <p style="{BodyText}">You requested a password reset. Use the OTP below to continue.</p>
            <div style="text-align:center;margin:28px 0;">
              <div style="display:inline-block;background:#f0f7f2;border:2px dashed {Primary};border-radius:12px;padding:20px 40px;">
                <div style="font-size:11px;color:#5a8a6a;text-transform:uppercase;letter-spacing:2px;margin-bottom:8px;">Your OTP</div>
                <div style="font-size:36px;font-weight:900;letter-spacing:10px;color:{Primary};font-family:monospace;">{otp}</div>
              </div>
            </div>
            <p style="text-align:center;font-size:13px;color:#888;margin:0;">This OTP expires in <strong>15 minutes</strong>. Do not share it with anyone.</p>
        """);

    /// <summary>Order status change notification.</summary>
    public static string OrderStatusChanged(string orderId, string newStatus) => Wrap(
        headerEmoji: StatusEmoji(newStatus),
        headerBg: StatusGradient(newStatus),
        title: $"Order {StatusLabel(newStatus)}",
        // Range operator [..Math.Min(8, orderId.Length)] — slices the first 8 characters (or fewer) of the orderId string
        // .ToUpper() — converts the slice to uppercase for display
        subtitle: $"Order #{orderId[..Math.Min(8, orderId.Length)].ToUpper()}",
        body: $"""
            <p style="{BodyText}">Your order status has been updated.</p>
            {InfoBox(StatusEmoji(newStatus), $"<strong>New Status:</strong> {StatusLabel(newStatus)}<br/><span style='color:#888;font-size:13px;'>{StatusDescription(newStatus)}</span>")}
            <p style="{BodyText}">You can track your order in real-time on the app.</p>
            {Cta("Track My Order", "http://localhost:4200/orders")}
        """);

    /// <summary>Order cancelled notification.</summary>
    public static string OrderCancelled(string orderId, string cancelledAt, string reason) => Wrap(
        headerEmoji: "❌",
        headerBg: "linear-gradient(135deg,#7f1d1d,#dc2626)",
        title: "Order Cancelled",
        subtitle: $"Order #{orderId[..Math.Min(8, orderId.Length)].ToUpper()}",
        body: $"""
            <p style="{BodyText}">We're sorry to let you know that your order has been cancelled.</p>
            {InfoBox("📋", $"<strong>Cancelled on:</strong> {cancelledAt}<br/><strong>Reason:</strong> {reason}")}
            <p style="{BodyText}">If you have any questions, please contact our support team. We'd love to help you place a new order!</p>
            {Cta("Order Again", "http://localhost:4200/restaurants")}
        """);

    /// <summary>Delivery completed notification.</summary>
    public static string DeliveryCompleted(string orderId, string completedAt) => Wrap(
        headerEmoji: "🎊",
        headerBg: $"linear-gradient(135deg,{Primary},#52b788)",
        title: "Your Order Has Arrived!",
        subtitle: $"Order #{orderId[..Math.Min(8, orderId.Length)].ToUpper()}",
        body: $"""
            <p style="{BodyText}">Great news — your order has been delivered!</p>
            {InfoBox("🕐", $"<strong>Delivered at:</strong> {completedAt}")}
            <p style="{BodyText}">We hope you enjoy your meal! If you loved it, leave a review to help others discover great food.</p>
            {Cta("Rate Your Order", "http://localhost:4200/orders")}
        """);

    /// <summary>Payment confirmed via UPI notification.</summary>
    // 'decimal' — 128-bit high-precision numeric type; used for monetary amounts to avoid floating-point rounding errors
    public static string PaymentConfirmed(string orderId, decimal amount, string paymentMethod, string confirmedAt) => Wrap(
        headerEmoji: "✅",
        headerBg: "linear-gradient(135deg,#4f46e5,#7c3aed)",
        title: "Payment Confirmed!",
        subtitle: $"Order #{orderId[..Math.Min(8, orderId.Length)].ToUpper()}",
        body: $"""
            <p style="{BodyText}">Your payment has been successfully received. Your order is now confirmed!</p>
            {InfoBox("📲", $"<strong>Amount Paid:</strong> ₹{amount:F2}<br/><strong>Method:</strong> {(paymentMethod == "UpiNow" ? "UPI" : paymentMethod)}<br/><strong>Confirmed At:</strong> {confirmedAt} IST")}
            <p style="{BodyText}">The restaurant has been notified and will start preparing your food shortly.</p>
            {Cta("Track Your Order", "http://localhost:4200/orders")}
        """);

    /// <summary>Payment failed notification.</summary>
    public static string PaymentFailed(string orderId, string reason) => Wrap(
        headerEmoji: "⚠️",
        headerBg: "linear-gradient(135deg,#92400e,#d97706)",
        title: "Payment Failed",
        subtitle: $"Order #{orderId[..Math.Min(8, orderId.Length)].ToUpper()}",
        body: $"""
            <p style="{BodyText}">Unfortunately, your payment could not be processed.</p>
            {InfoBox("💳", $"<strong>Reason:</strong> {reason}")}
            <p style="{BodyText}">Please try again with a different payment method or contact your bank.</p>
            {Cta("Retry Payment", "http://localhost:4200/orders")}
        """);

    /// <summary>Payment refunded notification.</summary>
    public static string PaymentRefunded(string orderId, decimal amount, string refundedAt) => Wrap(
        headerEmoji: "💸",
        headerBg: "linear-gradient(135deg,#1e40af,#3b82f6)",
        title: "Refund Processed",
        subtitle: $"Order #{orderId[..Math.Min(8, orderId.Length)].ToUpper()}",
        body: $"""
            <p style="{BodyText}">Your refund has been successfully processed.</p>
            {InfoBox("💰", $"<strong>Refund Amount:</strong> ₹{amount:F2}<br/><strong>Processed At:</strong> {refundedAt} IST")}
            <p style="{BodyText}">The amount will be credited to your original payment method within 5–7 business days.</p>
            {Cta("View Orders", "http://localhost:4200/orders")}
        """);

    /// <summary>Restaurant approved notification.</summary>
    public static string RestaurantApproved(string restaurantName, string approvedAt) => Wrap(
        headerEmoji: "✅",
        headerBg: $"linear-gradient(135deg,{Primary},#52b788)",
        title: "Restaurant Approved!",
        subtitle: restaurantName,
        body: $"""
            <p style="{BodyText}">Congratulations! Your restaurant has been reviewed and approved.</p>
            {InfoBox("🏪", $"<strong>{restaurantName}</strong> is now live on {Brand}.<br/><span style='color:#888;font-size:13px;'>Approved on {approvedAt}</span>")}
            <p style="{BodyText}">Customers can now discover and order from your restaurant. Make sure your menu is up to date!</p>
            {Cta("Go to Dashboard", "http://localhost:4200/owner/dashboard")}
        """);

    /// <summary>Restaurant rejected or suspended notification.</summary>
    // 'bool isSuspended' — a boolean parameter; true means suspended, false means rejected
    //   bool is a value type that holds exactly two states: true or false
    public static string RestaurantRejected(string restaurantName, string reason, bool isSuspended) => Wrap(
        // Ternary operator (? :) — inline if/else; selects one of two values based on the bool condition
        headerEmoji: isSuspended ? "⏸️" : "❌",
        headerBg: isSuspended ? "linear-gradient(135deg,#92400e,#d97706)" : "linear-gradient(135deg,#7f1d1d,#dc2626)",
        title: isSuspended ? "Restaurant Suspended" : "Application Not Approved",
        subtitle: restaurantName,
        body: $"""
            <p style="{BodyText}">We regret to inform you about the status of your restaurant on {Brand}.</p>
            {InfoBox(isSuspended ? "⏸️" : "📋", $"<strong>Restaurant:</strong> {restaurantName}<br/><strong>{(isSuspended ? "Suspension reason" : "Reason")}:</strong> {reason.Replace("Suspended: ", "")}")}
            <p style="{BodyText}">{(isSuspended ? "Please contact our support team if you believe this is a mistake." : "You may reapply after addressing the concerns mentioned above.")}</p>
            {Cta("Contact Support", "mailto:support@cravekart.com", danger: true)}
        """);

    // ── Private helpers ───────────────────────────────────────────────────────

    // 'private const string' — compile-time CSS string constant; shared across all template methods
    private const string BodyText = "font-size:15px;color:#374151;line-height:1.7;margin:0 0 16px;";

    // 'private static' — helper method; no instance needed, not part of the public API
    // Expression-bodied (=>) — single-expression body; splits the full name on spaces and returns the first token
    private static string FirstName(string fullName) =>
        fullName.Split(' ')[0];

    // Raw string literal (""" ... """) — multi-line string; avoids escaping HTML quotes and braces
    private static string InfoBox(string icon, string content) => $"""
        <div style="background:#f0f7f2;border-left:4px solid {Primary};border-radius:0 10px 10px 0;padding:16px 20px;margin:20px 0;display:flex;gap:12px;align-items:flex-start;">
          <span style="font-size:20px;flex-shrink:0;">{icon}</span>
          <div style="font-size:14px;color:#374151;line-height:1.6;">{content}</div>
        </div>
        """;

    // 'bool danger = false' — optional parameter with a default value; callers omit it for the normal (green) CTA
    private static string Cta(string label, string url, bool danger = false) => $"""
        <div style="text-align:center;margin:28px 0 8px;">
          <a href="{url}" style="display:inline-block;padding:14px 36px;background:{(danger ? "#dc2626" : Primary)};color:#ffffff;font-size:15px;font-weight:700;text-decoration:none;border-radius:10px;letter-spacing:0.3px;">{label}</a>
        </div>
        """;

    // Switch expression — a concise pattern-matching construct introduced in C# 8;
    //   each arm (pattern => result) is evaluated in order; '_' is the discard/default arm
    private static string StatusEmoji(string status) => status.ToLower() switch
    {
        "confirmed" => "✅",
        "preparing" => "👨‍🍳",
        "ready"     => "📦",
        "pickedup"  => "🛵",
        "delivered" => "🏠",
        "cancelled" => "❌",
        // '_' — discard pattern (default case); matches any value not matched above
        _           => "📋"
    };

    // Switch expression — maps status strings to human-readable labels
    private static string StatusLabel(string status) => status.ToLower() switch
    {
        "confirmed" => "Confirmed",
        "preparing" => "Being Prepared",
        "ready"     => "Ready for Pickup",
        "pickedup"  => "Picked Up",
        "delivered" => "Delivered",
        "cancelled" => "Cancelled",
        _           => status
    };

    // Switch expression — maps status strings to descriptive sentences shown in the email body
    private static string StatusDescription(string status) => status.ToLower() switch
    {
        "confirmed" => "The restaurant has accepted your order.",
        "preparing" => "The kitchen is preparing your food.",
        "ready"     => "Your order is packed and waiting for a delivery agent.",
        "pickedup"  => "A delivery agent has picked up your order and is on the way.",
        "delivered" => "Your order has been delivered. Enjoy!",
        "cancelled" => "Your order has been cancelled.",
        _           => ""
    };

    // Switch expression — maps status strings to CSS gradient strings for the email header background
    private static string StatusGradient(string status) => status.ToLower() switch
    {
        "confirmed" => $"linear-gradient(135deg,{Primary},#52b788)",
        "preparing" => "linear-gradient(135deg,#92400e,#d97706)",
        "ready"     => "linear-gradient(135deg,#1e40af,#3b82f6)",
        "pickedup"  => $"linear-gradient(135deg,#52796f,{Primary})",
        "delivered" => $"linear-gradient(135deg,{Primary},#52b788)",
        "cancelled" => "linear-gradient(135deg,#7f1d1d,#dc2626)",
        _           => $"linear-gradient(135deg,{Primary},{Accent})"
    };

    /// <summary>Wraps content in the shared CraveKart email shell.</summary>
    // 'private static' — internal helper; not exposed to callers of EmailTemplates
    // All parameters are 'string' — the method assembles them into a complete HTML document
    private static string Wrap(string headerEmoji, string headerBg, string title, string subtitle, string body) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>{title}</title>
        </head>
        <body style="margin:0;padding:0;background:{BgPage};font-family:'Segoe UI',Helvetica,Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:{BgPage};padding:40px 16px;">
            <tr><td align="center">
              <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:{BgCard};border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">

                <!-- Header -->
                <tr>
                  <td style="background:{headerBg};padding:40px 40px 32px;text-align:center;">
                    <div style="font-size:48px;margin-bottom:12px;">{headerEmoji}</div>
                    <h1 style="margin:0 0 6px;color:#ffffff;font-size:24px;font-weight:800;letter-spacing:-0.3px;">{title}</h1>
                    <p style="margin:0;color:rgba(255,255,255,0.85);font-size:14px;">{subtitle}</p>
                  </td>
                </tr>

                <!-- Brand strip -->
                <tr>
                  <td style="background:{Accent};padding:10px 40px;text-align:center;">
                    <span style="font-size:13px;font-weight:700;color:{Primary};letter-spacing:1px;text-transform:uppercase;">🌿 {Brand}</span>
                  </td>
                </tr>

                <!-- Body -->
                <tr>
                  <td style="padding:36px 40px 28px;">
                    {body}
                  </td>
                </tr>

                <!-- Divider -->
                <tr>
                  <td style="padding:0 40px;">
                    <hr style="border:none;border-top:1px solid #e5e7eb;margin:0;"/>
                  </td>
                </tr>

                <!-- Footer -->
                <tr>
                  <td style="padding:24px 40px;text-align:center;">
                    <p style="margin:0 0 6px;font-size:13px;color:#6b7280;">You're receiving this email because you have an account on <strong style="color:{Primary};">{Brand}</strong>.</p>
                    <p style="margin:0;font-size:12px;color:#9ca3af;">© {IstClock.Now.Year} {Brand}. All rights reserved.</p>
                  </td>
                </tr>

              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
