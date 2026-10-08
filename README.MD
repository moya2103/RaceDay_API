# RaceDay — Event Management System API

![RaceDay CI](https://github.com/moya2103/RaceDay_API/actions/workflows/ci.yml/badge.svg)

A full-stack RESTful API for the RaceDay event management platform, built with ASP.NET Core 8 Web API, Entity Framework Core, and SQL Server. Designed for the South African road running, walking, and cycling community.

**Module:** PROG6212 — Programming 2B  
**Part 2:** RESTful API with authentication, role-based access, Swagger, unit tests, and GitHub Actions CI/CD.

---

## System Roles

**Organiser**
- Create, edit, and delete events
- Manage age and distance categories per event
- View all enrolments for their events
- Capture participant results (finish times and positions)

**Participant**
- Register and log in
- Browse upcoming events
- Enter events by selecting a category
- View their own enrolments
- Track personal race results

---

## Prerequisites

- **Visual Studio 2022** (17.8 or later)
- **.NET 8.0 SDK**
- **SQL Server LocalDB** (installed with VS) or **SQL Server Express**

---

## Setup Instructions

1. Clone the repository:
   ```
   git clone https://github.com/<your-username>/RaceDay_API.git
   ```

2. Open `RaceDay.API.sln` in Visual Studio 2022.

3. Set `RaceDay.API` as the Startup Project.

4. Press **F5**.

The API automatically:
- Applies EF Core migrations to create the database schema
- Seeds two default test accounts (see below)

5. Navigate to:
   ```
   https://localhost:{port}/swagger
   ```
   (Or just the root URL — it redirects to Swagger automatically.)

---

## Default Test Accounts

| Role | Email | Password |
|------|-------|----------|
| Organiser | `organiser@raceday.co.za` | `Pass123!` |
| Participant | `participant@raceday.co.za` | `Pass123!` |

---

## Testing the API in Swagger

1. Run the API (F5)
2. Navigate to `/swagger`
3. Use `POST /api/auth/login` to log in as Organiser
4. The session cookie is stored automatically
5. Try `POST /api/events` → returns **201 Created**
6. Log out with `POST /api/auth/logout`
7. Log in as Participant
8. Try `POST /api/events` again → returns **403 Forbidden** (role enforced)

---

## Running the Tests

In Visual Studio: **Test → Run All Tests** — 36 tests across 8 test files.

Or via CLI:
```
dotnet test
```

---

## CI/CD

GitHub Actions runs on every push and pull request:
- Restores NuGet packages
- Builds the solution in Release configuration
- Runs all unit tests

View the live build status: [Actions tab](https://github.com/moya2103/RaceDay_API/actions)

### Green Build Screenshot

![CI Green Build](docs/ci-green-build.png)

---

## API Endpoints

Full endpoint documentation is available via Swagger at `/swagger` when the API is running.

| Resource | Endpoints |
|----------|-----------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/me` |
| Profile | `GET /api/profile`, `PUT /api/profile`, `PUT /api/profile/password` |
| Events | `GET /api/events`, `GET /api/events/{id}`, `POST /api/events`, `PUT /api/events/{id}`, `DELETE /api/events/{id}` |
| Categories | `GET /api/events/{eventId}/categories`, `POST /api/events/{eventId}/categories`, `PUT`, `DELETE` |
| Enrolments | `POST /api/events/{eventId}/enrol`, `GET /api/events/{eventId}/enrolments`, `GET /api/enrolments/my`, `DELETE /api/enrolments/{id}`, `PUT /api/enrolments/{id}/status` |
| Results | `POST /api/enrolments/{id}/result`, `GET /api/enrolments/{id}/result`, `GET /api/results/my` |
| Health | `GET /health` |

---

## Project Structure

```
RaceDay_API/
├── .github/workflows/ci.yml
├── .gitignore
├── README.md
├── RaceDay.API.sln
├── RaceDay.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Helpers/
│   ├── Migrations/
│   ├── Models/
│   ├── Properties/
│   ├── Services/
│   ├── appsettings.json
│   ├── Program.cs
│   └── RaceDay.API.csproj
└── RaceDay.API.Tests/
    ├── AuthTests.cs
    ├── CategoriesTests.cs
    ├── EnrolmentsTests.cs
    ├── EventsTests.cs
    ├── ProfileTests.cs
    ├── RaceDayFactory.cs
    ├── ResultsTests.cs
    ├── RoleAccessTests.cs
    ├── SessionFlowTests.cs
    └── RaceDay.API.Tests.csproj
```

---

## Video Presentation

Unlisted YouTube walkthrough: *(link to be added)*

---

## Notes on the Database

The API uses **Entity Framework Core Code-First** with migrations. The database schema matches the Part 1 ERD and SQL script.
