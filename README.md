# Time Off Accrual

A Time-Off & Accrual module for a Workforce Management system.

## Tech stack
React (Vite) + ASP.NET Core Web API (.NET 8) + EF Core + SQLite

## How to run

### Prerequisites
- .NET 8 SDK
- Node.js 18 or newer

### 1. Backend
```bash
cd backend/TimeOffAccrual/TimeOffAccrual.Api
dotnet user-secrets set "Jwt:Key" "replace-with-any-random-string-of-32-or-more-characters"
dotnet run --launch-profile https
```

The JWT signing key is a secret, so it is not stored in the repo. The command above stores it on your machine. The app refuses to start without it.

On first start, the app creates the SQLite database (`timeoffaccrual.db`), applies the migrations, and seeds demo data. 

If the browser warns about the HTTPS certificate, run `dotnet dev-certs https --trust` once.

Swagger is at `/swagger` in the Dev environment.

### 2. Frontend
In a second terminal:
```bash
cd frontend
npm install
npm run dev
```
Open http://localhost:5173.

The frontend calls the API at `https://localhost:7123` by default. If your API runs on a different port, create `frontend/.env.local` with `VITE_API_URL=<your API address>` and restart `npm run dev`.

### Demo accounts (demo only, not for production)
All accounts use the password `Temp#12345`.

| Email | Role | Earned | Taken |
|---|---|---|---|
| admin@sample.com | Admin | n/a | n/a |
| agent1@sample.com | Agent | 80 | 16 |
| agent2@sample.com | Agent | 120 | 0 |
| agent3@sample.com | Agent | 8 | 0 |

### Reset the data
Stop the API, delete `timeoffaccrual.db` (and any `-shm` or `-wal` files), and start it again. The database is rebuilt and reseeded.

## Assumptions
- Requests can span a date range. Full Day counts as 8 hours per calendar day in the range.
- Partial Day requests are limited to a single date, with 0 < hours <= 8.
- Weekends and holidays are counted as regular days (simplification).
- Pending requests do not reserve balance. The balance is re-checked at approval time.
- Requests with a start date in the past are rejected.
- Authentication is simplified: the UI sends the selected user's ID, and the API enforces Admin/Agent roles on each endpoint.
- Available balance is calculated as Earned - Taken and is never stored.

## Schema overview
_TODO_

## Tradeoffs and next steps
_TODO_