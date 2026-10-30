# RaceDay — Event Management System

![RaceDay CI](https://github.com/moya2103/RaceDay_API/actions/workflows/ci.yml/badge.svg)

A full-stack web-based event management platform for the South African road running, walking, and cycling community.

**Module:** PROG6212 — Programming 2B  
**Part 2:** RESTful API, Session Auth, xUnit Tests, CI/CD  
**Part 3:** MVC Web App, Azure Blob Storage, Docker

---

## System Roles

**Organiser**
- Create, edit, and delete events
- Upload event banner images (stored in Azure)
- Manage age and distance categories per event
- View all enrolments for their events
- Capture participant results (finish times and positions)

**Participant**
- Register and log in
- Browse upcoming events
- Enter events by selecting a category
- View their own enrolments
- Track personal race results
- Update their profile details

---

## Prerequisites

- **Visual Studio 2022** (17.8 or later)
- **.NET 8.0 SDK**
- **SQL Server LocalDB** (installed with VS)
- **Docker Desktop** (required for the MVC app)

---

##  How to Run the Application

This project consists of two parts: the **API** (Part 2) and the **MVC Front-End** (Part 3). 

### Step 1: Run the API
1. Open `RaceDay.API.sln` in Visual Studio 2022.
2. At the top toolbar, change the startup profile dropdown from `https` to **`http`**.
3. Press **F5**. The API will start, automatically apply EF Core migrations, and seed default accounts.

### Step 2: Run the MVC App via Docker
1. Open a terminal (PowerShell or Command Prompt) at the root folder of this solution.
2. Build the Docker image:
   ```bash
   docker build -t raceday-web -f RaceDay.Web/Dockerfile .
3.Run the container:
```bash
docker run -d -p 8080:8080 --name raceday-container raceday-web.

4. Open your browser and navigate to: http://localhost:8080

Default Test Accounts
Role	Email	Password
Organiser	organiser@raceday.co.za	Pass123!
Participant	participant@raceday.co.za	Pass123!
☁️ Azure Blob Storage Integration
Organisers can upload a banner image when creating an event.

The MVC app sends the image to the API via MultipartFormDataContent.

The API uses the Azure.Storage.Blobs SDK to upload the image to Azure Blob Storage.

The resulting public URL is saved in the SQL database and displayed on the MVC Home and Details pages.

Running the API Tests
In Visual Studio: Test → Run All Tests — 36 tests across 8 test files.

Or via CLI:

text
dotnet test
CI/CD
GitHub Actions runs on every push and pull request:

Restores NuGet packages

Builds the solution in Release configuration

Runs all API unit tests

View the live build status: Actions tab

Project Structure
text
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
│   ├── Services/       (Includes BlobService for Azure)
│   ├── appsettings.json
│   └── Program.cs
├── RaceDay.API.Tests/
│   ├── (8 test files)
│   └── RaceDay.API.Tests.csproj
└── RaceDay.Web/
    ├── Controllers/
    ├── Models/
    ├── Services/       (Includes ApiClient for HTTP)
    ├── Helpers/
    ├── Views/
    ├── wwwroot/
    ├── Dockerfile
    ├── appsettings.json
    └── Program.cs
Video Presentation
Unlisted YouTube walkthrough: [Your YouTube Link Here]

Notes on the Database
The API uses Entity Framework Core Code-First with migrations. The database schema matches the Part 1 ERD and SQL script.



















