# API-Auth-Demo

Lightweight ASP.Net Core 8 server that handles authentication of users
---
## Tech Stack:
- ASP.Net Core 8
- SQLite database (via EF Core)
- Swagger (for API documentation UI)
- Docker (included Dockerfile for easy containerization)

---
## API-Enabled features:
- User registration  
- User login  
- JWT access and refresh token rotation  
- Token revocation and logout  
--- 
## Build Guide
1. Clone the repository:
    ```bash
    git clone https://github.com/<your-username>/api-auth-demo.git
    cd api-auth-demo
    ```
2. Edit `appsettings.json`.
3. Run locally:
    ```bash
    dotnet run
    ```
   Or build and run with Docker:
    ```bash
    docker build -t api-auth-demo .
    docker run -p 8080:8080 api-auth-demo
    ```

---
## Deployment quirks:
EF Core runs automigrations therefore /Migrations folder is excluded from the project 
For production replace the ``appsettings.json`` JWT section with proper key size of at least 32 and change the issue and audience for your needs/projects

--
## Author: 
Mārtiņš Terentjevs 
GitHub: @martinsterentjevs 
Junior Software Developer
