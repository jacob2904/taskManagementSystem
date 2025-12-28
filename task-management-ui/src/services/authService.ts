import axios from "axios";
import { config } from "../config/env";

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  telephone: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface User {
  id: number;
  email: string;
  fullName: string;
  telephone: string;
  createdAt: string;
  lastLoginAt?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}

const TOKEN_KEY = "auth_token";
const USER_KEY = "user_data";
const EXPIRES_KEY = "token_expires";

class AuthService {
  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await axios.post<AuthResponse>(
      `${config.apiUrl}/auth/register`,
      data
    );
    this.setAuthData(response.data);
    return response.data;
  }

  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await axios.post<AuthResponse>(
      `${config.apiUrl}/auth/login`,
      data
    );
    this.setAuthData(response.data);
    return response.data;
  }

  async getCurrentUser(): Promise<User> {
    const response = await axios.get<User>(`${config.apiUrl}/auth/me`, {
      headers: this.getAuthHeaders(),
    });
    return response.data;
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(EXPIRES_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getUser(): User | null {
    const userData = localStorage.getItem(USER_KEY);
    if (!userData) return null;

    try {
      return JSON.parse(userData);
    } catch {
      return null;
    }
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    const expiresAt = localStorage.getItem(EXPIRES_KEY);

    if (!token || !expiresAt) {
      return false;
    }

    const now = new Date().getTime();
    const expiration = new Date(expiresAt).getTime();

    if (now >= expiration) {
      this.logout();
      return false;
    }

    return true;
  }

  getAuthHeaders(): Record<string, string> {
    const token = this.getToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
  }

  private setAuthData(authResponse: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, authResponse.token);
    localStorage.setItem(USER_KEY, JSON.stringify(authResponse.user));
    localStorage.setItem(EXPIRES_KEY, authResponse.expiresAt);
  }

  setupAxiosInterceptor(): void {
    axios.interceptors.request.use(
      (config) => {
        const token = this.getToken();
        if (token && this.isAuthenticated()) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    axios.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          this.logout();
          window.location.href = "/login";
        }
        return Promise.reject(error);
      }
    );
  }
}

export const authService = new AuthService();
