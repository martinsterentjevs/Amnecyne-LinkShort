# Amnecyne LinkShort

A personal link shortener under the Amnecyne domain. Supports anonymous public link creation with rate limiting, and authenticated accounts with custom slugs and link management.

Live at [shrtn.amnecyne.com](https://shrtn.amnecyne.com)

---

## Stack

**Backend** — .NET 10, ASP.NET Core, Entity Framework Core, SQLite, JWT authentication with refresh tokens

**Frontend** — React 19, TypeScript, Vite, Tailwind CSS, shadcn/ui

**Infra** — Docker, nginx, Cloudflare (SSL + proxy), Oracle VPS, GitHub Actions CI/CD

---

## Features

- Anonymous link shortening — rate limited to 5 per hour per IP
- Authenticated accounts — custom slugs, link management dashboard
- Link unraveling — resolve a short code without following the redirect
- JWT auth with refresh token rotation
- Auto-migration on startup

---

## Project Structure

```
├── backend/        # .NET 10 API
├── client/         # React frontend
├── tests/          # xUnit integration tests
├── infra/          # Dockerfile, docker-compose, nginx config
└── README.md
```

---

## Local Development

### Backend

Requires .NET 10 SDK.

```bash
cd backend
dotnet restore
dotnet run
```

Or via Docker:

```bash
docker compose -f infra/docker-compose.dev.yaml up --build
```

Backend runs on `http://localhost:5000`. OpenAPI available at `http://localhost:5000/openapi/v1.json` in Development mode.

### Frontend

Requires Node 22+.

```bash
cd client
npm install
cp .env.example .env.local   # edit as needed
npm run dev
```

Frontend runs on `http://localhost:5173`. The Vite dev proxy forwards `/api` and `/Auth` to the backend automatically.

### Environment variables

**Backend** — set via environment or `appsettings.json` (never commit real values):

| Variable | Description |
|---|---|
| `Jwt__Key` | JWT signing key, minimum 32 chars |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `ConnectionStrings__DefaultConnection` | SQLite connection string |

**Frontend** — set in `.env.local` for dev, `.env.production` for prod:

| Variable | Description |
|---|---|
| `VITE_API_URL` | API base URL (empty in dev, Vite proxy handles it) |
| `VITE_SHORT_BASE_URL` | Base URL for generated short links |

---

## Running Tests

```bash
dotnet test tests/Amnecyne.LinkShort.Tests/Amnecyne.LinkShort.Tests.csproj
```

Tests use an isolated per-test SQLite database via `WebApplicationFactory` and do not require the backend to be running.

---

## Deployment

Deployment is via GitHub Actions on push to `main`. The workflow SSHs into the Oracle VPS, pulls the latest commit, and rebuilds the Docker stack.

### First-time VPS setup

```bash
# Clone the repo
cd /srv
git clone https://github.com/YOUR_USERNAME/Amnecyne-LinkShort.git amnecyne-linkshort
cd amnecyne-linkshort

# Create the prod env file
cp infra/.env.example infra/.env
nano infra/.env   # fill in real values

# Start the stack
docker compose -f infra/docker-compose.prod.yaml up --build -d
```

Cloudflare origin certificates are expected at `/etc/ssl/cloudflare/` on the host and are mounted into the nginx container at runtime.

### GitHub Actions secrets required

| Secret | Description |
|---|---|
| `ORACLE_VPS_SSH_HOST` | VPS IP address |
| `ORACLE_VPS_SSH_USERNAME` | SSH username |
| `ORACLE_VPS_SSH_KEY` | Private SSH key |

---

## API Endpoints

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/Auth/Register` | None | Register a new account |
| `POST` | `/Auth/Login` | None | Login, returns tokens |
| `POST` | `/Auth/refresh` | None | Refresh access token |
| `POST` | `/Auth/logout` | Required | Logout, revokes refresh token |
| `POST` | `/api/Link/create-random` | None | Create anonymous short link |
| `POST` | `/api/Link/create` | Required | Create authenticated short link |
| `GET` | `/api/Link/user-links` | Required | List authenticated user's links |
| `GET` | `/api/Link/resolve/{code}` | None | Resolve short code without redirecting |
| `DELETE` | `/api/Link/delete/{code}` | Required | Delete a link (owner only) |
| `GET` | `/{code}` | None | Redirect to destination |
| `GET` | `/health` | None | Health check |