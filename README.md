# Time Off Accrual

A Time-Off & Accrual module for a Workforce Management system.

## Tech stack
React (Vite) + ASP.NET Core Web API (.NET 8) + EF Core + SQLite

## How to run
dotnet user-secrets set "Jwt:Key" "<32+ character string>

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