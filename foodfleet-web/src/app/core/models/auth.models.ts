/** Login request payload sent to the Identity API. */
export interface LoginRequest {
  /** The user's registered email address. */
  email: string;
  /** The user's plain-text password. */
  password: string;
}

/** Registration request payload for creating a new user account. */
export interface RegisterRequest {
  /** The full name of the user. */
  fullName: string;
  /** The email address for the new account. */
  email: string;
  /** The plain-text password to be hashed server-side. */
  password: string;
  /** The user's mobile phone number. */
  mobileNumber: string;
  /** The role to assign (e.g., "Customer", "RestaurantOwner", "DeliveryAgent"). */
  role: string;
}

/** Response returned after a successful login or registration. */
export interface AuthResponse {
  /** The unique identifier of the authenticated user. */
  id: string;
  /** The full name of the authenticated user. */
  fullName: string;
  /** The email address of the authenticated user. */
  email: string;
  /** The role of the authenticated user (e.g., "Customer", "Admin"). */
  role: string;
  /** The short-lived JWT access token for API authorization. */
  accessToken: string;
  /** The long-lived refresh token for obtaining new access tokens. */
  refreshToken: string;
}

/** Request payload for refreshing an expired access token. */
export interface RefreshTokenRequest {
  /** The user's email address. */
  email: string;
  /** The current refresh token to exchange. */
  refreshToken: string;
}

/** Request payload for initiating a password reset via OTP. */
export interface ForgotPasswordRequest {
  /** The email address of the account to reset. */
  email: string;
}

/** Request payload for completing a password reset using an OTP. */
export interface ResetPasswordRequest {
  /** The email address of the account. */
  email: string;
  /** The one-time password received via email. */
  otp: string;
  /** The new plain-text password to set. */
  newPassword: string;
}
