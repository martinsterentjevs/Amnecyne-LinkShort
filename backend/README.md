# Amnecyne LinkShort Backend

Backend API for the Amnecyne link shortener project.

---

## Purpose

This project provides the server-side functionality for a link shortener application, including user authentication and
token management used by the client application.

---

## Tech Stack

- ASP.NET Core 10
- Entity Framework Core 10
- SQLite
- Built-in OpenAPI support
- Docker

---

## API Features

- User registration
- User login
- JWT access and refresh token flow
- Token rotation and revocation/logout

---

## Run the Backend

1. Clone the repository:
   ```bash
   git clone https://github.com/martinsterentjevs/Amnecyne-LinkShort.git
   cd Amnecyne-LinkShort/backend
   ```
2. Configure `appsettings.json` (or environment variables) for JWT settings.
3. Run locally:
   ```bash
   dotnet run
   ```

Optional Docker run:

```bash
docker build -t amnecyne-linkshort-backend .
docker run -p 8080:8080 amnecyne-linkshort-backend
```

---

## OpenAPI

In Development environment, the OpenAPI document is exposed by the app at:

- `/openapi/v1.json`

---

## Notes

- EF Core migrations are applied automatically at startup.
- For production, use a strong JWT key (at least 32 characters) and configure issuer/audience values for your
  environment.

---

## Author

Mārtiņš Terentjevs  
GitHub: [@martinsterentjevs](https://github.com/martinsterentjevs)
