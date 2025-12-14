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
const API_ENDPOINT = `${API_BASE_URL}/api`;

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

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const startTime = Date.now();
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: this.getAuthHeaders(),
        ...options,
      });
      return this.handleResponse<T>(response, startTime);
    } catch (error) {
      // Handle network errors (backend not running, no internet, etc.)
      // Check for TypeError with message containing 'fetch' for cross-browser compatibility
      if (error instanceof TypeError && typeof error.message === 'string' && error.message.toLowerCase().includes('fetch')) {
        logger.error('Network Error', 'Unable to connect to the server. Please check if the backend is running.');
        throw new Error('Unable to connect to the server. Please make sure the backend is running and try again.');
      }
      // Re-throw other errors
      throw error;
    }
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });
  }

  async register(credentials: RegisterRequest): Promise<LoginResponse> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });
  }

  async logout(): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      headers: this.getAuthHeaders()
    });
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
  }

  async updateGift(id: number, gift: GiftUpdate): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(gift)
    });
  }

  async deleteGift(id: number): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/gift/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    });
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
  }

  async unclaimGift(id: number): Promise<void> {
    const startTime = Date.now();
    const response = await fetch(`${API_BASE_URL}/volunteers/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    });
  }
}

export const apiClient = new ApiClient();
