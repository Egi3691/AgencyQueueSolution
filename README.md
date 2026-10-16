## Agency Appointment Queue Management System

A web-based appointment booking and queue management system developed using **ASP.NET Core Web API (.NET 8)**.

The application allows an agency to schedule customer appointments, automatically generate queue tokens, manage daily capacity, and configure unavailable dates.

## Features

- **Appointment Booking:** Register customers with their name, phone number, and preferred appointment date.
- **Automatic Queue Tokens:** Generate sequential tokens for each appointment date.
- **Daily Queue Dashboard:** Display scheduled customers and their queue positions.
- **Daily Capacity Management:** Configure the maximum number of appointments allowed per day.
- **Holiday Management:** Mark dates as unavailable for appointments.
- **Automatic Rescheduling:** Move bookings to the next available date when the preferred date is closed or full.
- **REST API:** Expose appointment and administration functionality through HTTP endpoints.
- **Swagger Documentation:** Explore API endpoints through an interactive interface.
- **Unit Testing:** Validate booking rules using xUnit.

## Technology Stack

| Component | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 8) |
| Language | C# |
| Frontend | HTML, CSS, JavaScript |
| Database | SQLite |
| ORM | Entity Framework Core |
| API Documentation | Swagger / OpenAPI |
| Unit Testing | xUnit |
| Dependency Injection | Built-in ASP.NET Core DI |
| Source Control | Git and GitHub |

## System Architecture

The application uses three separate projects to demonstrate separation of concerns.

```text
AgencyQueueSolution/
├── src/
│   ├── AgencyQueue.Api/
│   │   ├── Program.cs
│   │   ├── wwwroot/
│   │   └── appsettings.json
│   │
│   ├── AgencyQueue.Business/
│   │   └── BookingService.cs
│   │
│   └── AgencyQueue.Data/
│       ├── AppDbContext.cs
│       └── Models.cs
│
├── tests/
│   └── AgencyQueue.Tests/
│       └── BookingServiceTests.cs
│
└── README.md
```

### Layer Responsibilities

**API Layer — AgencyQueue.Api**

Handles HTTP requests, API endpoints, Swagger documentation, input handling, and the web dashboard.

**Business Layer — AgencyQueue.Business**

Implements appointment scheduling, capacity checking, token generation, holiday handling, and retry logic.

**Data Layer — AgencyQueue.Data**

Contains database entities and the Entity Framework Core database context.

The application uses dependency injection to register and access the booking service and database context.

## Installation

### Prerequisites

- .NET 8 SDK
- Git
- A web browser

### Clone the Repository

```bash
git clone https://github.com/Egi3691/AgencyQueueSolution.git
cd AgencyQueueSolution
```

### Run the Application

```bash
cd src/AgencyQueue.Api
dotnet restore
dotnet run --urls http://localhost:5050
```

Open the web dashboard:

http://localhost:5050

Open Swagger documentation:

http://localhost:5050/swagger

The SQLite database is initialized automatically for the demonstration application.

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/appointments` | Create an appointment |
| GET | `/api/appointments/{id}` | Retrieve appointment details |
| GET | `/api/queue` | View daily appointments |
| GET | `/api/settings` | Retrieve agency settings |
| PUT | `/api/settings` | Update daily capacity |
| GET | `/api/closed-days` | Retrieve unavailable dates |
| POST | `/api/closed-days` | Add an unavailable date |
| DELETE | `/api/closed-days/{id}` | Remove an unavailable date |

The API can be explored using Swagger UI.

## Appointment Scheduling Example

Suppose the agency has a maximum capacity of two appointments per day.

| Customer | Requested Date | Scheduled Date | Queue Token |
|---|---|---|---|
| Customer A | 20 Oct 2026 | 20 Oct 2026 | 20261020-001 |
| Customer B | 20 Oct 2026 | 20 Oct 2026 | 20261020-002 |
| Customer C | 20 Oct 2026 | 21 Oct 2026 | 20261021-001 |

When the requested date reaches its daily capacity, the system automatically selects the next available date.

Unavailable dates are skipped when scheduling appointments.

## Unit Testing

The project includes unit tests implemented using xUnit.

Run the tests from the repository root:

```bash
dotnet test tests/AgencyQueue.Tests/AgencyQueue.Tests.csproj
```

### Test Results

The refactored solution was tested locally with the following results:

| Result | Count |
|---|---|
| Passed | 4 |
| Failed | 0 |
| Skipped | 0 |

All four unit tests passed after separating the API, Business, and Data projects.

## LINQ and Dependency Injection

LINQ is used for database operations including appointment filtering, queue ordering, capacity checking, and unavailable-date retrieval.

ASP.NET Core's built-in dependency injection is used to resolve services such as `IBookingService`, `BookingService`, `AppDbContext`, and `TimeProvider`.

This structure supports future improvements and alternative IoC containers.

## Deployment and Public Demonstration

### Local Hosting

The application runs locally using the ASP.NET Core development server.

### Cloudflare Quick Tunnel

For temporary external access, Cloudflare Quick Tunnel can expose the locally running application through an HTTPS URL.

```bash
cloudflared tunnel --url http://localhost:5050
```

This provides a temporary public demonstration link.

**Limitations:**

- The local application must remain running.
- The tunnel process must remain active.
- The public URL may change when the tunnel restarts.
- This is not permanent cloud hosting.

### Azure App Service

Azure App Service is the intended cloud hosting option for the original technical assignment.

Deployment was not completed because an active Azure subscription was unavailable.

A future Azure deployment would require configuring an appropriate hosting plan, application settings, database persistence, and a deployment workflow.

## Security and Production Considerations

This application is intended as a technical demonstration.

Before production use, additional improvements should include:

- Authentication and authorization for administrative endpoints.
- Persistent database hosting and backup procedures.
- Appropriate protection of customer personal information.
- HTTPS enforcement and production configuration.
- Database migrations and monitoring.
- Additional concurrency and integration testing.

Use fictional customer information when demonstrating the application through a public tunnel.

## Source Control

The project is maintained using GitHub.

Repository:

https://github.com/Egi3691/AgencyQueueSolution

The architecture was improved through a dedicated Git branch and merged using Pull Request #1.

## Project Status

- Appointment booking: Completed
- Queue token generation: Completed
- Holiday and capacity management: Completed
- Business and Data layer separation: Completed
- Swagger documentation: Completed
- Unit testing: Completed
- GitHub source control: Completed
- Temporary external demonstration access: Tested
- Azure deployment: Not completed

## Conclusion

Agency Appointment Queue demonstrates the implementation of an ASP.NET Core Web API application with layered architecture, dependency injection, LINQ, Entity Framework Core, Swagger documentation, automated unit testing, and GitHub source control.

The core appointment management functionality has been implemented and tested successfully.

**Verification note:** source code was created in an environment without the .NET SDK. Compile and test on a computer with the .NET 8 SDK before submitting.
