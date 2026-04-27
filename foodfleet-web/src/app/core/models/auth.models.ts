export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  mobileNumber: string;
  role: string;
}

export interface AuthResponse {
  id: string;
  fullName: string;
  email: string;
  role: string;
  accessToken: string;
  refreshToken: string;
}

export interface RefreshTokenRequest {
  email: string;
  refreshToken: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  otp: string;
  newPassword: string;
}
