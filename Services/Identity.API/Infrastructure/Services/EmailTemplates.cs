namespace Identity.API.Infrastructure.Services;

/// <summary>
/// Shared HTML email layout for CraveKart Identity emails.
/// </summary>
public static class EmailTemplates
{
    private const string Brand   = "CraveKart";
    private const string Primary = "#2d6a4f";
    private const string Accent  = "#e9c46a";
    private const string BgPage  = "#f0f4f1";

    public static string PasswordResetOtp(string otp) => Wrap(
        headerEmoji: "🔑",
        headerBg: $"linear-gradient(135deg,#1b4332,{Primary})",
        title: "Password Reset OTP",
        subtitle: $"Your {Brand} account",
        body: $"""
            <p style="{BodyText}">You requested a password reset for your {Brand} account. Use the OTP below to continue.</p>
            <div style="text-align:center;margin:28px 0;">
              <div style="display:inline-block;background:#f0f7f2;border:2px dashed {Primary};border-radius:12px;padding:20px 40px;">
                <div style="font-size:11px;color:#5a8a6a;text-transform:uppercase;letter-spacing:2px;margin-bottom:8px;">Your OTP</div>
                <div style="font-size:36px;font-weight:900;letter-spacing:10px;color:{Primary};font-family:monospace;">{otp}</div>
              </div>
            </div>
            <p style="text-align:center;font-size:13px;color:#888;margin:0 0 16px;">This OTP expires in <strong>15 minutes</strong>. Do not share it with anyone.</p>
            <div style="background:#fff8e1;border-left:4px solid {Accent};border-radius:0 10px 10px 0;padding:14px 18px;font-size:13px;color:#92400e;">
              ⚠️ If you did not request a password reset, please ignore this email. Your account is safe.
            </div>
        """);

    private const string BodyText = "font-size:15px;color:#374151;line-height:1.7;margin:0 0 16px;";

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
              <table width="600" cellpadding="0" cellspacing="0" style="max-width:600px;width:100%;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">
                <tr>
                  <td style="background:{headerBg};padding:40px 40px 32px;text-align:center;">
                    <div style="font-size:48px;margin-bottom:12px;">{headerEmoji}</div>
                    <h1 style="margin:0 0 6px;color:#ffffff;font-size:24px;font-weight:800;">{title}</h1>
                    <p style="margin:0;color:rgba(255,255,255,0.85);font-size:14px;">{subtitle}</p>
                  </td>
                </tr>
                <tr>
                  <td style="background:{Accent};padding:10px 40px;text-align:center;">
                    <span style="font-size:13px;font-weight:700;color:{Primary};letter-spacing:1px;text-transform:uppercase;">🌿 {Brand}</span>
                  </td>
                </tr>
                <tr>
                  <td style="padding:36px 40px 28px;">{body}</td>
                </tr>
                <tr>
                  <td style="padding:0 40px;"><hr style="border:none;border-top:1px solid #e5e7eb;margin:0;"/></td>
                </tr>
                <tr>
                  <td style="padding:24px 40px;text-align:center;">
                    <p style="margin:0 0 6px;font-size:13px;color:#6b7280;">You're receiving this because you have an account on <strong style="color:{Primary};">{Brand}</strong>.</p>
                    <p style="margin:0;font-size:12px;color:#9ca3af;">© {DateTime.UtcNow.Year} {Brand}. All rights reserved.</p>
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
