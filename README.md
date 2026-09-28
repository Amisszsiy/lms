# Library Management System (LMS)

## Setup

Default user: 
username: admin
password: p@ssw0rd
Seed automatically when run in mode Development environment.

### Option 1 – Run locally

Requires .NET 8 SDK, Node 20+ and Angular CLI.

# 1. Database (from repo root, comment services (api/client) out)
docker compose up -d sqlserver

# 2. API -> https://localhost:7016/swagger
cd lms-api/LibraryManagementSystem
dotnet run --project WebAPI --launch-profile https
or
debug in vs studio

# 3. Client -> https://localhost:4200
cd lms-client
npm install
ng s --ssl

### Option 2 – Docker Compose

1. In docker-compose.yml, uncomment the lms-api and lms-client services.
2. Run docker compose up -d --build
3. Open the client at http://localhost:4000

## Architecture

Angular (SSR) ──HTTPS, JWT cookie -> ASP.NET Core API -> EF Core -> SQL Server

The API uses Clean Code Architecture:

- Domain – entities, value objects (Isbn, EmailAddress, Address), Result/Error. No framework dependencies. (using result pattern over exception in this layer)
- Application – feature services, DTOs, validators, mapping and repository interfaces.
- Infrastructure – EF Core DbContext, configurations, migrations, repositories and the seeder.
- WebAPI – controllers, JWT auth, CORS, Serilog and exception middleware.

The client has feature pages, guards (auth, noAuth, admin), typed HTTP services and an interceptor that sends credentials with every request.

## Tech stack
Mostly choices are made according to assignment requirement but there are below benefit.
- .NET 8, ASP.NET Core: LTS release, fast, built-in DI and auth.
- EF Core + SQL Server: Code-first migrations, rowversion and filtered unique indexes for concurrency
- JWT, Identity PasswordHasher: Stateless role-based auth and proven password hashing
- FluentValidation, AutoMapper: Free, robust and community proved third party libs.
- Serilog, Swagger: Structured logs and rich API docs
- Angular 21 + Material + SSR: Full-featured framework, premade useful ui components.

## Assumptions and design decisions

- Availability and location are tracked per copy.
- Copy lifecycle: Available → Borrowed → IssueReturn → Available. A copy can also be Damaged, Lost or Withdrawn.
- Returns take two steps. The user requests the return, then an admin processes it when the book is handed in.
- Loans last 14 days. Loan status (Borrowed/Overdue/ReturnRequested/Returned) is calculated when queried, not stored.
- Deletes are soft. Deleting a book or copy withdraws it, to preserve borrowing history.
- Self-registered users get the NormalUser role. Deactivated users cannot log in or borrow.
- Result pattern. Business failures return error codes instead of throwing exceptions.
- JWT in an HttpOnly Secure cookie so JavaScript cannot read it (XSS mitigation). The server enforces roles.
- No double borrowing. A rowversion on BookCopy and a unique index on open loans prevent two loans of the same copy.
- Automatic audit fields. Created/Modified By/Date are set in SaveChangesAsync.
- Search, sorting and pagination happen on the server.
- Secrets live in config files for local convenience. Move them to secret vault or a secret store for production.

## AI usages
- Claude Code was the only tool used to help build this project.
- Every architectural components and tech stack were decided by me and ai will list up pros and cons for futhur consideration.
- Code base patterns were written first then Claude was used to repeat similar pattern. For example, Book entity had been created first and then others is written by Claude and then get reviewed.