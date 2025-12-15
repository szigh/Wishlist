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

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

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
    const url = response.url.replace(API_BASE_URL, '');
    
    logger.api(response.status < 400 ? 'SUCCESS' : 'ERROR', url, response.status, duration);
    
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

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });

    return this.handleResponse(response, startTime);
  }

  async register(credentials: RegisterRequest): Promise<LoginResponse> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });

    return this.handleResponse(response, startTime);
  }

  async logout(): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      headers: this.getAuthHeaders()
    });

    return this.handleResponse(response, startTime);
  }

  // User endpoints
  async getUsers(): Promise<User[]> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/users`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<User[]>(response, startTime);
  }

  async getUser(id: number): Promise<User> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<User>(response, startTime);
  }

  async getUserWishlist(id: number): Promise<UserWithWishlist> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/users/${id}/wishlist`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<UserWithWishlist>(response, startTime);
  }

  // Gift endpoints
  async getGifts(): Promise<Gift[]> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<Gift[]>(response, startTime);
  }

  async getGift(id: number): Promise<Gift> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift/${id}`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<Gift>(response, startTime);
  }

  async createGift(gift: GiftCreate): Promise<Gift> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(gift)
    });

    return this.handleResponse(response, startTime);
  }

  async updateGift(id: number, gift: GiftUpdate): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(gift)
    });

    return this.handleResponse(response, startTime);
  }

  async deleteGift(id: number): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    });

    return this.handleResponse(response, startTime);
  }

  // Volunteer (Claim) endpoints
  async getVolunteers(): Promise<Volunteer[]> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/volunteers`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<Volunteer[]>(response, startTime);
  }

  async claimGift(claim: VolunteerCreate): Promise<Volunteer> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/volunteers`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(claim)
    });
    return this.handleResponse<Volunteer>(response, startTime);
  }

  async unclaimGift(id: number): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/volunteers/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    });

    return this.handleResponse(response, startTime);
  }
}

export const apiClient = new ApiClient();
