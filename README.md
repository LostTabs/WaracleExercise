# Hotel Booking API

An ASP.NET Core REST API for searching hotels, checking room availability, and creating and retrieving hotel room 
bookings. Data is persisted with Entity Framework Core and SQLite.

## Contents

- [Technology](#technology)
- [Getting started](#getting-started)
- [Using the API](#using-the-api)
- [Business rules](#business-rules)
- [Test data](#test-data)
- [Automated tests](#automated-tests)
- [Design notes and future improvements](#design-notes-and-future-improvements)

## Technology

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- SQLite
- Swagger / OpenAPI (Swashbuckle)
- NUnit



## Getting started

### Prerequisites

- .NET 10 SDK

## Project structure

```text
HotelBookingApi/
├── Controllers/
│   └── Test/
├── Data/
│   └── Configurations/
├── Domain/
│   └── Exceptions/
├── DTOs/
├── Services/
└── Program.cs

HotelBookingApiTests/
├── Domain/
├── Services/
├── Data/
└── Integration/
```

### Run locally

From the solution root:

```powershell
dotnet restore
dotnet run --project .\HotelBookingApi\HotelBookingApi.csproj
```

The development launch profile exposes the API at:

- `http://localhost:5089`
- `https://localhost:7058`

On application start, EF Core applies the included migrations. The default SQLite database is `hotelbooking.db`; its 
connection string is configured in `HotelBookingApi/appsettings.json`.

Swagger is enabled in the `Development` environment and is available at:

```text
http://localhost:5089/swagger
```

No authentication is required.

## Using the API

Dates use ISO 8601 format (`yyyy-MM-dd`). A booking occupies every night from `checkIn` up to, but excluding, 
`checkOut`. Therefore, a guest checking out on a date does not block another guest checking in on that same date.

### Find a hotel by name

```http
GET /api/hotels?name=Grand%20Hotel
```

Successful response (`200 OK`):

```json
{
  "id": 1,
  "name": "Grand Hotel"
}
```

The name search is case-insensitive. A missing hotel returns `404 Not Found`; a blank name returns `400 Bad Request`.

### Find available rooms

```http
GET /api/hotels/1/rooms/availability?numberOfGuests=2&checkIn=2026-09-10&checkOut=2026-09-15
```

Successful response (`200 OK`):

```json
[
  {
    "id": 3,
    "type": "Double",
    "capacity": 2
  }
]
```

The endpoint returns rooms that both accommodate the requested party and have no overlapping booking for the requested 
stay. `checkOut` must be later than `checkIn`, and `numberOfGuests` must be between 1 and 4. An unknown hotel returns 
`404 Not Found`; invalid query values return `400 Bad Request`.

### Create a booking

```http
POST /api/bookings
Content-Type: application/json

{
  "hotelId": 1,
  "numberOfGuests": 2,
  "checkIn": "2026-09-10",
  "checkOut": "2026-09-15"
}
```

Successful response (`201 Created`):

```json
{
  "reference": "A1B2C3D4E5F",
  "hotelId": 1,
  "roomId": 3,
  "numberOfGuests": 2,
  "checkIn": "2026-09-10",
  "checkOut": "2026-09-15"
}
```

The `Location` header points to `/api/bookings/{reference}`. If no suitable room is available, the API returns 
`409 Conflict`; an invalid request returns `400 Bad Request`; and an unknown hotel returns `404 Not Found`.

### Get a booking by reference

```http
GET /api/bookings/A1B2C3D4E5F
```

Successful responses use the same payload as booking creation. An unrecognised reference returns `404 Not Found`.

## Business rules

- Hotels are limited to a maximum of six rooms. The seeded test hotel contains exactly six rooms. 
- The seeded hotel contains exactly six rooms: two single rooms, two double rooms, and two deluxe rooms.
- Room capacities are single: 1 guest, double: 2 guests, and deluxe: 4 guests.
- A booking is assigned to one room for its full stay, so guests never need to change rooms.
- A room is unavailable when an existing booking overlaps the requested date range.
- Booking references are generated for new bookings and backed by a unique database index.
- Hotel names are unique in the database.

## Test data

Two unauthenticated endpoints make it easy to start a manual test from a known state. They are intended for local 
development and automated testing only.

```http
POST /api/test-data/reset
POST /api/test-data/seed
```

`reset` deletes and recreates the database schema. `seed` is idempotent: when no hotel exists, it creates 
**Grand Hotel** and its six rooms; otherwise it makes no change. Both return `204 No Content`.

For a repeatable manual test:

1. Call `POST /api/test-data/reset`.
2. Call `POST /api/test-data/seed`.
3. Search for `Grand Hotel` to obtain its identifier (normally `1` after a reset).
4. Check availability, then create and retrieve a booking using the returned reference.

## Automated tests

The test project includes domain, service, data, and API integration tests. Run the full suite from the solution root:

```powershell
dotnet test .\HotelBooking.sln
```

Integration tests run against an in-memory SQLite database, keeping them isolated from the local `hotelbooking.db` file.
The test database is recreated for each test fixture, so tests do not depend on execution order or persisted test data.

## Design notes and future improvements

The API uses controllers for the HTTP boundary, services for booking and availability rules, domain entities for core 
invariants, DTOs for public payloads, and EF Core configurations for persistence constraints. At the time of submission
the test suite contains 90 tests, covering 100% of Services, DTOs and Data, 98% of Domain and 61% of Controllers. 

Migrations are not test covered.

For a production deployment, the test-data endpoints should be disabled or protected outside development. Booking 
creation should also use a concurrency strategy (for example, a transaction with an appropriate isolation level or 
optimistic concurrency/retry) so two simultaneous requests cannot select the same last available room. A production 
database such as Azure SQL or PostgreSQL, structured logging, health checks, and CI/CD deployment would be natural 
next steps.
