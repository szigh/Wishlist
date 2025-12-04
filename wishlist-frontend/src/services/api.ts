import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  User,
  UserWithWishlist,
  Gift,
  GiftCreate,
  GiftUpdate,
  Volunteer,
  VolunteerCreate,
} from "../types";
import { logger } from "../utils/logger";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "https://localhost:7059/api";

class ApiClient {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem("token");
    return {
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  private async handleResponse<T>(
    response: Response,
    startTime: number
  ): Promise<T> {
    const duration = Date.now() - startTime;
    const url = response.url.replace(API_BASE_URL, "");

    logger.api(
      response.status < 400 ? "SUCCESS" : "ERROR",
      url,
      response.status,
      duration
    );

    if (!response.ok) {
      if (response.status === 401) {
        logger.warn("Authentication failed - redirecting to login");
        // Token expired or invalid - clear auth
        localStorage.removeItem("token");
        localStorage.removeItem("user");
        window.location.href = "/login";
      }
      const error = await response.text();
      logger.error(`API Error: ${url}`, error);
      throw new Error(error || `HTTP error! status: ${response.status}`);
    }

    if (response.status === 204) {
      return {} as T;
    }

    return response.json();
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: this.getAuthHeaders(),
      ...options,
    });
    return this.handleResponse<T>(response, startTime);
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    return this.request<LoginResponse>("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(credentials),
    });
  }

  async register(credentials: RegisterRequest): Promise<LoginResponse> {
    return this.request<LoginResponse>("/auth/register", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(credentials),
    });
  }

  async logout(): Promise<void> {
    return this.request<void>("/auth/logout", {
      method: "POST",
    });
  }

  // User endpoints
  async getUsers(): Promise<User[]> {
    return this.request<User[]>("/users");
  }

  async getUser(id: number): Promise<User> {
    return this.request<User>(`/users/${id}`);
  }

  async getUserWishlist(id: number): Promise<UserWithWishlist> {
    return this.request<UserWithWishlist>(`/users/${id}/wishlist`);
  }

  // Gift endpoints
  async getGifts(): Promise<Gift[]> {
    return this.request<Gift[]>("/gift");
  }

  async getGift(id: number): Promise<Gift> {
    return this.request<Gift>(`/gift/${id}`);
  }

  async createGift(gift: GiftCreate): Promise<Gift> {
    return this.request<Gift>("/gift", {
      method: "POST",
      body: JSON.stringify(gift),
    });
  }

  async updateGift(id: number, gift: GiftUpdate): Promise<void> {
    return this.request<void>(`/gift/${id}`, {
      method: "PUT",
      body: JSON.stringify(gift),
    });
  }

  async deleteGift(id: number): Promise<void> {
    return this.request<void>(`/gift/${id}`, {
      method: "DELETE",
    });
  }

  // Volunteer (Claim) endpoints
  async getVolunteers(): Promise<Volunteer[]> {
    return this.request<Volunteer[]>("/volunteers");
  }

  async claimGift(claim: VolunteerCreate): Promise<Volunteer> {
    return this.request<Volunteer>("/volunteers", {
      method: "POST",
      body: JSON.stringify(claim),
    });
  }

  async unclaimGift(id: number): Promise<void> {
    return this.request<void>(`/volunteers/${id}`, {
      method: "DELETE",
    });
  }
}

export const apiClient = new ApiClient();
