# MIS

MIS Collection Firm is the foundation for an internal collection management system. The repository is organized as a production-oriented full-stack application with a React/TypeScript frontend and a Clean Architecture ASP.NET Core backend.

## Architecture

```text
MIS/
  frontend/
    src/
      assets/                       # MIS logo and visual assets
      components/common/            # Reusable buttons, spinners, error blocks
      components/forms/             # Reusable form inputs
      components/layout/            # Auth page shell and page layouts
      config/                       # Frontend runtime configuration
      context/                      # Auth context
      features/auth/                # Auth feature components, services, types, validation
      pages/auth/                   # Login page
      pages/dashboard/              # Protected dashboard placeholder
      routes/                       # App routes and ProtectedRoute
      services/                     # Central Axios API client
      types/                        # Shared API types
      utils/                        # Storage helpers
  backend/
    MIS.API/                        # Controllers, middleware, CORS/JWT/API composition
    MIS.Application/                # DTOs, interfaces, auth use case
    MIS.Domain/                     # User, Role, UserRole entities and domain constants
    MIS.Infrastructure/             # EF Core, PostgreSQL, repositories, JWT/password services, seed data
    MIS.sln
    dotnet-tools.json               # Repo-local dotnet-ef tool
```

## Prerequisites

- .NET SDK 10
- Node.js 24+
- PostgreSQL

## Backend Setup

The backend does not store secrets in `appsettings.json`. Configure sensitive values with environment variables or user secrets.

PowerShell environment variable example:

```powershell
cd backend
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=mis_dev;Username=postgres;Password=YOUR_DEV_DB_PASSWORD"
$env:Jwt__SecretKey="replace-with-a-long-random-dev-secret-at-least-32-bytes"
$env:Seed__AdminPassword="choose-a-strong-development-password"
$env:Seed__HrPassword="choose-a-strong-development-hr-password"
dotnet restore
dotnet build
dotnet run --project MIS.API
```

User secrets example:

```powershell
cd backend/MIS.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=mis_dev;Username=postgres;Password=YOUR_DEV_DB_PASSWORD"
dotnet user-secrets set "Jwt:SecretKey" "replace-with-a-long-random-dev-secret-at-least-32-bytes"
dotnet user-secrets set "Seed:AdminPassword" "choose-a-strong-development-password"
dotnet run
```

Development seed user:

- Username: `admin`
- Department: `ADMIN`
- Password: supplied by `Seed:AdminPassword`

Development HR user (when `Seed:HrPassword` is configured):

- Username: `hr.user`
- Department: `HR`
- Password: supplied by `Seed:HrPassword`

The seed password is intentionally not checked into source control.

## Database

The initial migration has already been created at:

```text
backend/MIS.Infrastructure/Persistence/Migrations
```

To create a future migration:

```powershell
cd backend
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName --project MIS.Infrastructure --startup-project MIS.API --output-dir Persistence\Migrations
```

To apply migrations:

```powershell
cd backend
dotnet tool restore
dotnet tool run dotnet-ef database update --project MIS.Infrastructure --startup-project MIS.API
```

If you prefer a global EF tool, the equivalent command is:

```powershell
dotnet ef database update --project MIS.Infrastructure --startup-project MIS.API
```

## Frontend Setup

```powershell
cd frontend
copy .env.example .env
npm install
npm run dev
```

Default frontend API URL:

```text
VITE_API_URL=http://localhost:5000/api
```

Routes:

- `/login` - MIS branded sign-in page
- `/hr` and `/hr/dashboard` - protected HR dashboard
- `/hr/delegations` - تفويضات placeholder
- `/hr/absences` - غيابات الشركة placeholder
- `/hr/employee-documents` - أوراق الموظفين placeholder
- `/hr/master` - Master placeholder

## Authentication Flow

1. React posts credentials to `POST /api/auth/login`.
2. ASP.NET Core validates the user against PostgreSQL.
3. Password verification uses ASP.NET Core `PasswordHasher<T>`.
4. The API returns a JWT access token and safe user profile fields.
5. React stores the token in `sessionStorage` or `localStorage` depending on "Remember me".
6. Department is read from the database, included in the signed JWT, and used for post-login routing.
7. HR API endpoints enforce the `HrDepartment` policy server-side; frontend guards provide the matching navigation boundary.

## API Error Format

Errors use a consistent response shape:

```json
{
  "success": false,
  "message": "Invalid username or password.",
  "errors": []
}
```

Technical exception details are logged by the API and are not shown to users.

