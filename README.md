# Smart Diet API Service

Smart Diet API Service is a **NestJS** backend for the Smart Diet and Food Analyzer final-year project. It provides JWT authentication, user profile management, meal image analysis orchestration, PostgreSQL persistence, and Swagger API documentation for the mobile app and ML service.

## Tech Stack

- NestJS + Node.js + TypeScript
- PostgreSQL + TypeORM
- JWT authentication
- bcrypt password hashing
- NestJS `HttpModule` / Axios for ML service integration
- Swagger for API documentation
- `class-validator` + `class-transformer` for DTO validation
- multipart/form-data upload support
- `@nestjs/config` for environment configuration

## Project Structure

```text
src/
├── analysis/   # upload/history/detail endpoints and persistence
├── auth/       # register/login/logout/refresh and JWT strategy
├── common/     # guards, decorators, error filter
├── config/     # environment loading and validation
├── database/   # TypeORM PostgreSQL connection
├── ml/         # ML service HTTP integration and response normalization
├── users/      # user entity and profile management
└── main.ts     # bootstrap, global pipes, Swagger
```

## Core Features

- `POST /auth/register`
- `POST /auth/login`
- `POST /auth/logout`
- `POST /auth/refresh`
- `GET /users/profile`
- `PATCH /users/profile`
- `POST /analysis/upload`
- `GET /analysis/history`
- `GET /analysis/:id`
- Swagger docs at `/docs`
- Health check at `/health`

## Expected Integration Flow

1. User signs in from the mobile app.
2. Mobile app uploads a meal image to `POST /analysis/upload`.
3. Backend validates the JWT and accepts the multipart upload.
4. Backend forwards the image to the configured ML service.
5. ML service returns detected foods and nutrient estimates.
6. Backend normalizes the ML payload.
7. Backend stores the result in PostgreSQL.
8. Backend returns the formatted response to the mobile app.
9. User retrieves history later via `GET /analysis/history`.

## Environment Configuration

Copy the example file and update the values:

```bash
cp .env.example .env
```

### Required variables

| Variable | Description |
| --- | --- |
| `PORT` | API port |
| `DB_HOST` | PostgreSQL host |
| `DB_PORT` | PostgreSQL port |
| `DB_USERNAME` | PostgreSQL username |
| `DB_PASSWORD` | PostgreSQL password |
| `DB_DATABASE` | PostgreSQL database name |
| `DB_SYNCHRONIZE` | Set `true` for demo/dev schema sync |
| `JWT_SECRET` | Access token secret |
| `JWT_EXPIRES_IN` | Access token lifetime |
| `JWT_REFRESH_SECRET` | Refresh token secret |
| `JWT_REFRESH_EXPIRES_IN` | Refresh token lifetime |
| `BCRYPT_SALT_ROUNDS` | Password hashing cost |
| `ML_SERVICE_URL` | Base URL of the ML backend |
| `ML_ANALYSIS_ENDPOINT` | ML image analysis endpoint path |
| `ML_SERVICE_TIMEOUT_MS` | ML request timeout in milliseconds |

## Local Setup

### 1. Install dependencies

```bash
npm install
```

### 2. Start PostgreSQL (Docker Compose)

This repository already includes `docker-compose.yml` with PostgreSQL and pgAdmin.

```bash
docker compose up -d postgres
```

Optional (start pgAdmin too):

```bash
docker compose up -d
```

Useful checks:

```bash
docker compose ps
docker compose logs -f postgres
```

### 3. Configure environment

```bash
cp .env.example .env
```

Make sure these DB values remain aligned with `docker-compose.yml`:

```bash
DB_HOST=localhost
DB_PORT=5432
DB_USERNAME=postgres
DB_PASSWORD=postgres
DB_DATABASE=smart_diet
```

### 4. Run the API

```bash
npm run start:dev
```

Open:

- API: `http://localhost:3000`
- Swagger: `http://localhost:3000/docs`
- Health check: `http://localhost:3000/health`
- pgAdmin: `http://localhost:5050` (email: `admin@smartdiet.com`, password: `admin123`)

## API Overview

### Authentication

- `POST /auth/register` — create a user and return access/refresh tokens
- `POST /auth/login` — authenticate and return access/refresh tokens
- `POST /auth/refresh` — rotate tokens using a refresh token
- `POST /auth/logout` — clear the stored refresh token for the authenticated user

### User Profile

- `GET /users/profile` — get the authenticated user's profile
- `PATCH /users/profile` — update name, age, weight, height, gender, activity level, or dietary goal

### Food Analysis

- `POST /analysis/upload` — send `multipart/form-data` with an `image` field
- `GET /analysis/history` — list the authenticated user's analysis history
- `GET /analysis/:id` — fetch one analysis result by ID

## ML Service Integration Notes

The backend uses the `ml` module to call the external ML service with Axios via NestJS `HttpModule`.

- Uploads are forwarded as multipart form data using the `image` field.
- Timeouts are configurable with `ML_SERVICE_TIMEOUT_MS`.
- Upstream failures are converted into backend-friendly error responses.
- The response normalizer accepts common shapes such as `foods`, `detectedFoods`, `predictions`, `nutrition`, and `nutrients`.

Example ML response shapes the backend can normalize:

```json
{
  "foods": [
    {
      "name": "Jollof Rice",
      "confidence": 0.96,
      "calories": 420,
      "nutrients": {
        "protein": 9,
        "carbohydrates": 65,
        "fat": 11
      }
    }
  ],
  "totalCalories": 420
}
```

or

```json
{
  "predictions": ["banana", "apple"],
  "nutrition": {
    "calories": 180,
    "fiber": 7
  }
}
```

## Notes for Demonstration

- `DB_SYNCHRONIZE=true` is convenient for a project demo and quick setup.
- Passwords are hashed before storage.
- Protected routes require a Bearer token.
- Swagger can be used to demonstrate the full mobile-to-backend contract.

## Validation

```bash
npm run lint
npm run build
npm test -- --runInBand
npm run test:e2e
```
