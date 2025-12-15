# Wishlist App

A full-stack wishlist application where users can create and manage their gift wishlists, and volunteer to buy gifts for others.

## Features

- **User Authentication**: Secure JWT-based authentication (logout is handled on the frontend)
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
- **Nginx** - Production web server

### Infrastructure

- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **SQLite** - Database with persistent Docker volumes

## Getting Started

### Prerequisites

- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** - That's it! Docker includes everything needed to run the application.

### Quick Start (Recommended)

1. **Configure environment variables:**

```powershell
copy .env.example .env
```

Edit `.env` and set a secure JWT key (minimum 32 characters):
```
JWT_KEY=your-super-secret-jwt-key-that-is-at-least-32-characters-long
```

Add your Automapper key, or get a free (non-commercial) or paid license https://automapper.io/#pricing
```
AUTOMAPPER_KEY=
```

2. **Start the application:**

```powershell
docker-compose up --build
```

3. **Access the application:**
   - Frontend: http://localhost:3000
   - Backend API: http://localhost:5000
   - API Health Check: http://localhost:5000/api/health

4. **Stop the application:**

```powershell
docker-compose down
```

### Manual Setup (Without Docker)

If you prefer to run without Docker:

#### Prerequisites
- .NET 10.0 SDK
- Node.js (v20+)

#### Backend Setup

1. Navigate to the backend directory:
```powershell
cd WishlistWeb
```

2. Add your JWT secret key and automapper key to user secrets:
```powershell
dotnet user-secrets set "Jwt:Key" "your-secret-key-here"
dotnet user-secrets set "AUTOMAPPER_KEY" "your-automapper-key-here"
```

3. Run the backend:
```powershell
dotnet run
```

The API will be available at `https://localhost:7059`

#### Frontend Setup

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
│   ├── Dockerfile            # Backend container definition
│   └── Program.cs            # Application configuration
├── WishlistModels/           # Database entities and DbContext
├── WishlistContracts/        # DTOs for API requests/responses
├── wishlist-frontend/        # React frontend
│   ├── src/
│   │   ├── components/       # Reusable UI components
│   │   ├── contexts/         # React contexts (Auth)
│   │   ├── pages/            # Page components
│   │   ├── services/         # API client
│   │   └── types/            # TypeScript type definitions
│   ├── public/
│   ├── Dockerfile            # Frontend container definition
│   └── nginx.conf            # Nginx configuration
├── docker-compose.yml        # Development orchestration
├── docker-compose.prod.yml   # Production orchestration
└── .env.example              # Environment variables template
```

## Security Features

- Password hashing with BCrypt
- JWT tokens with configurable expiration (default: 60 minutes in development)  
- Logout is handled on the frontend (tokens expire automatically after the configured expiration time)  
- Role-based authorization
- User ownership validation (users can only modify their own resources)
- Auto-population of user IDs from JWT claims
- CORS configuration for secure cross-origin requests
- Nginx security headers in production

## Docker Features

- **Full Containerization** - Run anywhere with Docker installed
- **Persistent Database** - SQLite data stored in `.docker-data/` folder (easy to access and backup)
- **Auto Migrations** - Database schema updates automatically on startup
- **Environment Variables** - Secure configuration management
- **Flexible Storage** - Use default location or specify custom path in `.env`
- **Production Ready** - Optimized builds with Nginx
- **Easy Deployment** - Single command to start entire stack

## Common Commands

```powershell
# Start application
docker-compose up --build

# Start in background
docker-compose up -d

# Stop application (preserves database)
docker-compose down

# View logs
docker-compose logs -f

# Restart services
docker-compose restart

# Rebuild after code changes (preserves database)
docker-compose up --build

# Backup database (Docker volume - default)
docker cp wishlist-api:/data/wishlist.db ./backup-wishlist.db

docker cp wishlist-api:/data/wishlist.db ./backup.db # Or use this if database path was customized or containers are running

# Or if using custom host path (when DATABASE_PATH is set in .env)
Copy-Item .\backup-wishlist.db C:\your\custom\path\wishlist.db

# Reset database (⚠️ deletes all data - backup first if needed!)
docker-compose down

# If using Docker volume (default):
docker volume rm <project>_wishlist-data   # Replace <project> with your project directory name (e.g., 'wishlist_wishlist-data')

# If using host path (custom DATABASE_PATH):
Remove-Item -Recurse -Force C:\your\custom\path
```

## Database Configuration

**Default (Recommended):** The database is stored in a Docker volume named `wishlist-data`. This is the recommended approach as it:
- Is managed by Docker
- Persists across container restarts
- Provides better performance
- Is portable across environments

**Custom Host Path (Optional):** If you need direct access to the database file from your host machine, edit `.env` and set:
```dotenv
DATABASE_PATH=C:/your/custom/path
```

The database will then be accessible at `C:\your\custom\path\wishlist.db` on your host filesystem.

**To access the database in the default Docker volume:**
```powershell
# Copy database out of Docker volume
docker cp wishlist-api:/data/wishlist.db ./local-copy.db

# Or start an interactive shell in the container
docker exec -it wishlist-api sh
# Then navigate to /data/wishlist.db
```

## License

This project is licensed under the Creative Commons Attribution-NonCommercial 4.0 International License (CC BY-NC 4.0).

This means you can view, download, and modify the code for non-commercial purposes with attribution. Commercial use is not permitted.

See the LICENSE.txt file for full details.
