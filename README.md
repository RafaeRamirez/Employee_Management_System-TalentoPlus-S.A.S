# TalentoPlus Employee Management System

Modern ASP.NET Core 8 solution for TalentoPlus S.A.S. with:
- Admin web (MVC + Identity) in Spanish for HR to CRUD employees, import Excel, generate PDF CV (download-only), dashboard and AI-assisted queries.
- Public/secure REST API with JWT for employees to self-register, login, read profile, and download their own PDF.
- PostgreSQL persistence, Docker Compose for full stack, environment-driven configuration, and automated tests (unit + integration).

## Architecture
- Clean layering: Domain (entities/enums), Application (DTOs/contracts/interfaces), Infrastructure (EF Core + Identity + repos/servicios), Web (MVC) and Api (REST).
- Shared `AppDbContext` using PostgreSQL + ASP.NET Core Identity with seeded admin user.
- Services: `EmployeeService` (CRUD/import/pdf/metrics/AI), `AuthService` (register/login + email), `DepartmentService`, `JwtTokenService`, `EmailService`, `AiProvider`.

## Running with Docker
1) Ensure Docker and Docker Compose are installed.
2) From repo root, run:
```bash
docker compose up --build
```
3) Apps:
   - Web admin: http://localhost:5000
   - REST API swagger: http://localhost:5001/swagger
   - PostgreSQL: host `localhost`, port `5433` (maps to container 5432), database `talentoplus`

### Environment variables
Override via `.env` or CLI:
- `ConnectionStrings__DefaultConnection` (e.g. `Host=postgres;Port=5432;Database=talentoplus;Username=postgres;Password=postgres`)
- `JWT_SECRET` / `Jwt__Secret` (required for auth)
- `Jwt__Issuer`, `Jwt__Audience`
- `ADMIN_EMAIL`, `ADMIN_PASSWORD` (seeded Identity admin for web)
- SMTP: `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASS`, `SMTP_FROM`
- AI: `AI__ApiUrl`, `AI__ApiKey` (optional; falls back to heuristics if empty)
 - Docker DB host port: `POSTGRES_HOST_PORT` (defaults to 5433 on host)
Create your env file: `cp .env.example .env` and replace secrets (especially `Jwt__Secret`, DB password, SMTP).

## Credentials
- Default admin (web): email `admin@talentoplus.com`, password `Admin123!` (override via env).
- Employee login (API): uses `document` + `password` chosen at self-registration.

## Excel import
- Upload from web (`/Import`). First row must contain at least 14 columns in the order:
  `Documento, Nombres, Apellidos, FechaNacimiento, Direccion, Telefono, Email, Cargo, Salario, FechaIngreso, Estado, NivelEducativo, PerfilProfesional, Departamento`
- Current model ignores `FechaNacimiento` (not stored); the rest are mapped and upserted. Departments are created on the fly if they do not exist.

## API overview
- Public:
  - `GET /api/departments`
  - `POST /api/auth/register` (self-register + welcome email) body: employee fields + `password`
  - `POST /api/auth/login` body: `{ document, password }` -> `{ token }`
- Secured (JWT Bearer):
  - `GET /api/me` -> employee profile from token
  - `GET /api/me/pdf` -> own CV PDF

## Tests
- Run tests (once packages restored): `dotnet test TalentoPlus.sln`
- Includes 2 unit tests (domain) + 2 integration tests (EmployeeService with EF InMemory).

## Local development (optional)
```bash
dotnet restore  # requires NuGet access
dotnet ef database update -p TalentoPlus.Infrastructure -s TalentoPlus.Api
dotnet run --project TalentoPlus.Web   # admin UI
dotnet run --project TalentoPlus.Api   # REST API
```

## Notes
- SMTP sends real emails; configure valid credentials before running `POST /api/auth/register`.
- AI section calls configurable endpoint; if unset, heuristic parsing is used to avoid hallucinations.
- QuestPDF license is set to Community at startup (no key required). CVs download directly with a timestamped filename to avoid cache.
- If EF tools are unavailable, you can apply `schema.sql` directly: `psql -h localhost -p 5432 -U postgres -d talentoplus -f schema.sql` (adjust port if using Docker host 5433).

## Setup quick steps
1) Clone: `git clone https://github.com/RafaeRamirez/Employee_Management_System-TalentoPlus-S.A.S.git`
2) Environment: `cp .env.example .env` and set secrets (JWT, DB, SMTP).
3) Optional local run: `dotnet restore && dotnet ef database update -p TalentoPlus.Infrastructure -s TalentoPlus.Api`
4) Run with Docker: `docker compose up --build`
5) Admin web: http://localhost:5000 (credentials in `.env` or `ADMIN_EMAIL/ADMIN_PASSWORD`)

## Requested info
- Repository link: https://github.com/RafaeRamirez/Employee_Management_System-TalentoPlus-S.A.S.git
- Clan: caiman
- Path: C#
- Name: Rafael Augusto Ramirez Bolaño
- Email: rafar1129@gmail.com
