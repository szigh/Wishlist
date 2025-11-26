# Wishlist App

A full-stack wishlist application where users can create and manage their gift wishlists, and volunteer to buy gifts for others.

## Features

- **User Authentication**: Secure JWT-based authentication with token blacklisting
- **Personal Wishlists**: Users can create, edit, and delete gifts on their own wishlist
- **Browse Wishlists**: View other users' wishlists to see what they want
- **Gift Claims**: Volunteer to buy gifts from others' wishlists (claims are visible only to you)
- **Secure Authorization**: Users can only modify their own resources

## Tech Stack

### Backend

- **ASP.NET Core 10.0** - Web API
- **Entity Framework Core** - ORM with SQLite database
- **JWT Authentication** - Microsoft.AspNetCore.Authentication.JwtBearer
- **AutoMapper** - DTO mapping
- **BCrypt.Net** - Password hashing

### Frontend

- **React 19** - UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool and dev server
- **React Router DOM** - Client-side routing

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Node.js (v18+)

### Backend Setup

1. Navigate to the backend directory:

```powershell
cd WishlistWeb
```

2. Add your JWT secret key to user secrets:

```powershell
dotnet user-secrets set "Jwt:Key" "your-secret-key-here"
```

3. Run the backend:

```powershell
dotnet run
```

The API will be available at `http://localhost:5287`

### Frontend Setup

1. Navigate to the frontend directory:

```powershell
cd wishlist-frontend
```

2. Install dependencies:

```powershell
npm install
```

3. Start the development server:

```powershell
npm run dev
```

The frontend will be available at `http://localhost:5173`

## API Endpoints

### Authentication

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and receive JWT token
- `POST /api/auth/logout` - Logout (handled on frontend only)

### Users

- `GET /api/users` - Get all users (admin only)
- `GET /api/users/gifts/{userId}` - Get a user's wishlist with gifts

### Gifts

- `GET /api/gifts` - Get all gifts
- `POST /api/gifts` - Add a gift to your wishlist
- `PUT /api/gifts/{id}` - Update your own gift
- `DELETE /api/gifts/{id}` - Delete your own gift

### Volunteers (Claims)

- `GET /api/volunteers` - Get your claimed gifts
- `POST /api/volunteers` - Claim a gift to buy
- `DELETE /api/volunteers/{id}` - Unclaim a gift

## Project Structure

```
Wishlist/
├── WishlistWeb/              # Backend API
│   ├── Controllers/          # API controllers
│   ├── Services/             # Business logic
│   └── Program.cs            # Application configuration
├── WishlistModels/           # Database entities and DbContext
├── WishlistContracts/        # DTOs for API requests/responses
└── wishlist-frontend/        # React frontend
    ├── src/
    │   ├── components/       # Reusable UI components
    │   ├── contexts/         # React contexts (Auth)
    │   ├── pages/            # Page components
    │   ├── services/         # API client
    │   └── types/            # TypeScript type definitions
    └── public/
```

## Security Features

- Password hashing with BCrypt
- JWT tokens with 15-minute expiration
- Token blacklisting on logout
- Role-based authorization (admin)
- User ownership validation (users can only modify their own resources)
- Auto-population of user IDs from JWT claims

## License

This project is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License (CC BY-NC 4.0).

This means you can view, download, and modify the code for non-commercial purposes with attribution. Commercial use is not permitted.

See the LICENSE.txt file for full details.
