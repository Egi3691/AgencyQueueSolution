# Agency Appointment Queue — ASP.NET Core 8 Demo

A runnable source-code demo for the agency queue assignment, with business logic separated from the HTTP layer, EF Core + SQLite, Swagger, basic HTML UI, dependency injection, LINQ, and xUnit tests.

## Requirements
- .NET 8 SDK (https://dotnet.microsoft.com/download/dotnet/8.0)

## Run locally
```bash
cd src/AgencyQueue.Api
dotnet restore
dotnet run --urls http://localhost:5050
```
Open http://localhost:5050 for the booking dashboard and http://localhost:5050/swagger for the API explorer. The `queue.db` file is automatically created on first run.

## Run tests
From the repository root:
```bash
dotnet test tests/AgencyQueue.Tests/AgencyQueue.Tests.csproj
```

## API endpoints
| Verb | Route | Purpose |
| --- | --- | --- |
| POST | `/api/appointments` | Book and generate a unique token |
| GET | `/api/appointments/{id}` | Get a booking |
| GET | `/api/queue?date=2026-10-16` | Day's ordered queue |
| GET | `/api/settings` | Show daily capacity |
| PUT | `/api/settings` | Update daily capacity |
| GET | `/api/closed-days` | List agency holidays |
| POST | `/api/closed-days` | Register holiday |
| DELETE | `/api/closed-days/{id}` | Remove holiday |

## Request examples
Book:
```bash
curl -X POST http://localhost:5050/api/appointments \
  -H 'Content-Type: application/json' \
  -d '{"customerName":"John Doe","phone":"08123456789","preferredDate":"2026-10-20"}'
```
Set capacity to two:
```bash
curl -X PUT http://localhost:5050/api/settings \
  -H 'Content-Type: application/json' -d '{"dailyCapacity":2}'
```
Close an agency day:
```bash
curl -X POST http://localhost:5050/api/closed-days \
  -H 'Content-Type: application/json' \
  -d '{"date":"2026-10-21","reason":"Public holiday"}'
```

## Booking rules
- Each appointment has one queue number per date, beginning at 1.
- Token format: `yyyyMMdd-NNN` (the numeric suffix grows beyond three digits if necessary).
- When preferred date is full or marked closed, the system searches forward up to 366 days.
- Past-date validation uses UTC in this demonstration. For an agency in Indonesia, change this to an explicit `Asia/Jakarta` business timezone.
- Existing appointments are never silently moved when capacity is reduced; consider disallowing a setting below any booked day's count.
- Closing a day with existing appointments is rejected to avoid silently invalidating bookings.
- A unique SQLite index prevents duplicate (date, queue number) tokens; short retries handle simultaneous submissions.
- Saturday/Sunday are **not** automatically closed; enter closure dates individually. Weekly recurring days off could be added.

## Layering
- `Program.cs`: Web-layer HTTP endpoints, service registration, Swagger.
- `BookingService.cs`: Business layer for availability, capacity, allocation, and tokens.
- `AppDbContext.cs` and `Models.cs`: Domain/data layer.
- `wwwroot/index.html`: Lightweight customer booking and queue dashboard.
- `tests/`: Unit-style booking behavior tests using in-memory SQLite.

## Production improvements
- Authentication and role-based access for booking administration.
- Use EF Core migrations, centralized logging, and health checks.
- Define the agency business timezone, operating hours, and recurring closed weekdays.
- For high traffic, use a stronger allocation strategy/transactional row locking (e.g. SQL Server/PostgreSQL) to avoid contention and ensure daily limits across concurrent requests.
- Add customer privacy/retention rules and phone-number validation.
- Add optimistic concurrency control for capacity/holiday changes.
- Azure App Service can host the API, but SQLite should be replaced with managed Azure SQL or PostgreSQL for multi-instance deployments.

**Verification note:** source code was created in an environment without the .NET SDK. Compile and test on a computer with the .NET 8 SDK before submitting.
