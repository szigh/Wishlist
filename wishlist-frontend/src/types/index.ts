// Auth types
export interface LoginRequest {
  name: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: number;
  name: string;
  role: string;
}

export interface RegisterRequest {
  name: string;
  password: string;
}

// User types
export interface User {
  id: number;
  name: string;
  role: string;
}

export interface UserWithWishlist extends User {
  gifts: Gift[];
}

// Gift types
export interface Gift {
  id: number;
  title: string;
  description?: string;
  link?: string;
  category?: string;
  isTaken: boolean;
  userId: number;
}

export interface GiftCreate {
  title: string;
  description?: string;
  link?: string;
  category?: string;
}

export interface GiftUpdate {
  title: string;
  description?: string;
  link?: string;
  category?: string;
  isTaken: boolean;
}

// Volunteer (Claim) types
export interface Volunteer {
  id: number;
  giftId: number;
  volunteerUserId: number;
  gift?: Gift;
  volunteerUser?: User;
}

export interface VolunteerCreate {
  giftId: number;
  volunteerUserId: number;
}
