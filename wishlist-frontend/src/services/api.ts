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
  VolunteerCreate 
} from '../types';

const API_BASE_URL = 'http://localhost:5287/api';

class ApiClient {
  private getAuthHeaders(): HeadersInit {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      ...(token && { 'Authorization': `Bearer ${token}` })
    };
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      if (response.status === 401) {
        // Token expired or invalid - clear auth
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
      const error = await response.text();
      throw new Error(error || `HTTP error! status: ${response.status}`);
    }
    
    if (response.status === 204) {
      return {} as T;
    }
    
    return response.json();
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });
    return this.handleResponse<LoginResponse>(response);
  }

  async register(credentials: RegisterRequest): Promise<LoginResponse> {
    const response = await fetch(`${API_BASE_URL}/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });
    return this.handleResponse<LoginResponse>(response);
  }

  async logout(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<void>(response);
  }

  // User endpoints
  async getUsers(): Promise<User[]> {
    const response = await fetch(`${API_BASE_URL}/users`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<User[]>(response);
  }

  async getUser(id: number): Promise<User> {
    const response = await fetch(`${API_BASE_URL}/users/${id}`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<User>(response);
  }

  async getUserWishlist(id: number): Promise<UserWithWishlist> {
    const response = await fetch(`${API_BASE_URL}/users/${id}/wishlist`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<UserWithWishlist>(response);
  }

  // Gift endpoints
  async getGifts(): Promise<Gift[]> {
    const response = await fetch(`${API_BASE_URL}/gifts`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<Gift[]>(response);
  }

  async getGift(id: number): Promise<Gift> {
    const response = await fetch(`${API_BASE_URL}/gifts/${id}`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<Gift>(response);
  }

  async createGift(gift: GiftCreate): Promise<Gift> {
    const response = await fetch(`${API_BASE_URL}/gifts`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(gift)
    });
    return this.handleResponse<Gift>(response);
  }

  async updateGift(id: number, gift: GiftUpdate): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/gifts/${id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(gift)
    });
    return this.handleResponse<void>(response);
  }

  async deleteGift(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/gifts/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<void>(response);
  }

  // Volunteer (Claim) endpoints
  async getVolunteers(): Promise<Volunteer[]> {
    const response = await fetch(`${API_BASE_URL}/volunteers`, {
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<Volunteer[]>(response);
  }

  async claimGift(claim: VolunteerCreate): Promise<Volunteer> {
    const response = await fetch(`${API_BASE_URL}/volunteers`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify(claim)
    });
    return this.handleResponse<Volunteer>(response);
  }

  async unclaimGift(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/volunteers/${id}`, {
      method: 'DELETE',
      headers: this.getAuthHeaders()
    });
    return this.handleResponse<void>(response);
  }
}

export const apiClient = new ApiClient();
