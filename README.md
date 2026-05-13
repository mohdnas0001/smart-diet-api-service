# Smart Diet API Service

A production-ready **ASP.NET Core 8** Web API backend for the Smart Diet nutritional analysis system. It integrates with a Python/FastAPI ML service to analyse food photographs and return detailed nutritional breakdowns.

---

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Local Development](#local-development)
  - [Docker Compose](#docker-compose)
- [Configuration](#configuration)
- [API Reference](#api-reference)
  - [Authentication](#authentication)
  - [User Profile](#user-profile)
  - [Food Analysis](#food-analysis)
- [Data Models](#data-models)
- [ML Service Integration](#ml-service-integration)
- [Database](#database)

---

## Architecture

```
Mobile App (React Native)
        │
        ▼
┌─────────────────────┐       ┌───────────────────────┐
│  Smart Diet API     │──────▶│  ML Service            │
│  (ASP.NET Core 8)   │       │  (Python / FastAPI)    │
│  Port 5000 / 8080   │       │  Port 8000             │
└─────────────────────┘       └───────────────────────┘
        │
        ▼
  SQLite Database
  (smartdiet.db)
```

---

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| Database | SQLite via Entity Framework Core 8 |
| Authentication | JWT Bearer (access + refresh tokens) |
| Password hashing | BCrypt.Net-Next |
| API documentation | Swagger / OpenAPI (Swashbuckle) |
| ML integration | `HttpClient` → FastAPI |
| Containerisation | Docker + Docker Compose |

---

## Project Structure

```
smart-diet-api-service/
├── .env.example               # Environment variable template
├── .gitignore
├── docker-compose.yml
├── README.md
└── SmartDietApi/
    ├── SmartDietApi.csproj
    ├── Program.cs
    ├── Dockerfile
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Controllers/
    │   ├── AuthController.cs
    │   ├── UserController.cs
    │   └── AnalysisController.cs
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── Migrations/
    ├── DTOs/
    │   ├── Auth/
    │   ├── User/
    │   └── Analysis/
    ├── Entities/
    │   ├── User.cs
    │   ├── RefreshToken.cs
    │   └── MealAnalysis.cs
    ├── Services/
    │   ├── AuthService.cs
    │   ├── UserService.cs
    │   ├── AnalysisService.cs
    │   └── MlClientService.cs
    ├── Middleware/
    │   └── ErrorHandlingMiddleware.cs
    └── wwwroot/
        └── uploads/          # Uploaded food images
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- (Optional) [Docker & Docker Compose](https://docs.docker.com/get-docker/)

### Local Development

```bash
# 1. Clone the repo
git clone https://github.com/mohdnas0001/smart-diet-api-service.git
cd smart-diet-api-service

# 2. Enter the API project
cd SmartDietApi

# 3. Run migrations (SQLite DB is created automatically on startup too)
dotnet ef database update

# 4. Start the API
dotnet run
```

The API will be available at **http://localhost:5000** and Swagger UI at **http://localhost:5000/swagger**.

### Docker Compose

```bash
# Copy and edit the env file
cp .env.example .env
# Edit .env: set JWT_SECRET to a long random string

docker compose up --build
```

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger

---

## Configuration

All settings can be provided via `appsettings.json`, `appsettings.Development.json`, or environment variables (using double-underscore `__` for nested keys).

| Key | Env Variable | Default | Description |
|---|---|---|---|
| `Jwt:Secret` | `Jwt__Secret` | *(required)* | JWT signing secret (≥ 32 chars) |
| `Jwt:Issuer` | `Jwt__Issuer` | `SmartDietApi` | JWT issuer claim |
| `Jwt:Audience` | `Jwt__Audience` | `SmartDietApp` | JWT audience claim |
| `MlService:Url` | `MlService__Url` | `http://localhost:8000` | ML service base URL |
| `Database:Path` | `Database__Path` | `smartdiet.db` | SQLite file path |

---

## API Reference

All endpoints return JSON. Authenticated endpoints require an `Authorization: Bearer <accessToken>` header.

### Authentication

#### `POST /api/auth/register`

Register a new user.

**Request body:**
```json
{
  "name": "Jane Doe",
  "email": "jane@example.com",
  "password": "secret123",
  "profile": {
    "dateOfBirth": "1990-05-15",
    "gender": "female",
    "height": 165,
    "weight": 60,
    "activityLevel": "moderate",
    "dietaryPreferences": ["vegetarian"],
    "allergies": ["peanuts"],
    "calorieTarget": 1800,
    "macroTargets": { "carbs": 225, "protein": 90, "fat": 60 }
  }
}
```

**Response `200`:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "base64...",
  "user": { ... }
}
```

---

#### `POST /api/auth/login`

**Request body:**
```json
{ "email": "jane@example.com", "password": "secret123" }
```

**Response `200`:** same `AuthResponse` shape as register.

---

#### `POST /api/auth/logout` *(auth required)*

Revokes all active refresh tokens.

**Response `204`** – No content.

---

#### `POST /api/auth/refresh`

Exchange a refresh token for a new access token.

**Request body:**
```json
{ "refreshToken": "base64..." }
```

**Response `200`:**
```json
{ "accessToken": "eyJ..." }
```

---

#### `POST /api/auth/forgot-password`

Initiates a password reset (stub – sends `204` in current implementation).

**Request body:** `{ "email": "jane@example.com" }`

**Response `204`** – No content.

---

### User Profile

#### `GET /api/user/profile` *(auth required)*

**Response `200`:** `User` object (see [Data Models](#data-models)).

---

#### `PUT /api/user/profile` *(auth required)*

Partial update – only include fields you want to change.

**Request body (all fields optional):**
```json
{
  "name": "Jane Smith",
  "weight": 58,
  "calorieTarget": 1700
}
```

**Response `200`:** Updated `User` object.

---

### Food Analysis

#### `POST /api/analysis/upload` *(auth required)*

Upload a food image for nutritional analysis.

**Request:** `multipart/form-data` with field `image` (JPEG / PNG / WebP / GIF).

**Response `200`:** `AnalysisResult` (see [Data Models](#data-models)).

---

#### `GET /api/analysis/{id}` *(auth required)*

Retrieve a previous analysis by its ID.

**Response `200`:** `AnalysisResult`.  
**Response `404`:** Analysis not found.

---

#### `GET /api/analysis/history` *(auth required)*

Paginated meal history.

**Query params:**

| Param | Default | Description |
|---|---|---|
| `page` | `1` | Page number (1-based) |
| `limit` | `20` | Records per page (max 100) |

**Response `200`:** Array of `MealRecord` objects.

---

## Data Models

### User

```json
{
  "id": "uuid",
  "name": "Jane Doe",
  "email": "jane@example.com",
  "avatar": null,
  "dateOfBirth": "1990-05-15",
  "gender": "female",
  "height": 165,
  "weight": 60,
  "activityLevel": "moderate",
  "dietaryPreferences": ["vegetarian"],
  "allergies": ["peanuts"],
  "calorieTarget": 1800,
  "macroTargets": { "carbs": 225, "protein": 90, "fat": 60 }
}
```

### AnalysisResult

```json
{
  "id": "uuid",
  "imageUrl": "/uploads/abc123.jpg",
  "createdAt": "2024-01-15T12:30:00Z",
  "mealType": "lunch",
  "foodItems": [
    {
      "name": "jollof rice",
      "confidence": 0.92,
      "portionGrams": 320.5,
      "calories": 538.4,
      "nutrients": { "calories": 538.4, "carbohydrates": 87.4, "..." : "..." }
    }
  ],
  "totalCalories": 538.4,
  "macronutrients": { "carbs": 87.4, "protein": 12.2, "fat": 16.6, "fiber": 2.1 },
  "micronutrients": [
    { "name": "Iron", "value": 2.5, "unit": "mg", "rdaPercentage": 13.9 }
  ]
}
```

### MealRecord (history)

```json
{
  "id": "uuid",
  "imageUrl": "/uploads/abc123.jpg",
  "createdAt": "2024-01-15T12:30:00Z",
  "mealType": "lunch",
  "foodItems": ["jollof rice"],
  "totalCalories": 538.4
}
```

---

## ML Service Integration

When the ML service is reachable the API calls:

```
POST {ML_SERVICE_URL}/api/predict
Content-Type: multipart/form-data
field: file = <image bytes>
```

If the ML service is **unavailable** (connection refused, timeout, non-2xx response), the API returns a realistic fallback/mock response so the demo works offline.

Meal type is inferred from the UTC time of the request:

| Hour (UTC) | Meal type |
|---|---|
| 06 – 10 | breakfast |
| 11 – 15 | lunch |
| 16 – 19 | dinner |
| all other | snack |

---

## Database

SQLite is used for portability. The file is created automatically on first run via EF Core migrations.

To manage migrations manually:

```bash
cd SmartDietApi

# Create a new migration
dotnet ef migrations add <MigrationName>

# Apply pending migrations
dotnet ef database update

# Roll back the last migration
dotnet ef migrations remove
```
