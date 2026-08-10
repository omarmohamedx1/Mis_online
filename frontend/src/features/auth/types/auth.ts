export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginFormValues extends LoginRequest {
  rememberMe: boolean;
}

export interface AuthenticatedUser {
  id: string;
  username: string;
  fullName: string;
  department: string;
  role: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  user: AuthenticatedUser;
}
