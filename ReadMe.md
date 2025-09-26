# Flower Inventory (ASP.NET Core MVC + EF Core + SQL Server)

Production‑ready sample that implements a small flower inventory with **Categories** and **Flowers**. It includes:

* SQL Server schema + seed + CRUD & search stored procedures
* ASP.NET Core MVC (net9.0), EF Core models & DbContext
* Service layer with unit tests
* Front‑end pages (Home, List with search/sort/pagination, Details, Add/Edit, Delete)
* Docker & CI hints

---

## 1) Prerequisites

* **.NET SDK 9.0+**
* **Docker** (for local SQL Server), or an existing SQL Server 2019/2022 instance
* **Windows PowerShell** / Bash

> If you target .NET 8, change `<TargetFramework>` accordingly; this repo currently builds with **net9.0**.

---

## 2) Quick Start

### Option A — Run SQL Server in Docker (recommended for dev)

```powershell
# Start SQL Server 2022
$sa = "Your_strong_password_123!"
docker run -d --name mssql -e ACCEPT_EULA=Y -e SA_PASSWORD=$sa -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

Apply scripts in order (idempotent):

```powershell
# Adjust -S (server) and -P (password) if needed
sqlcmd -S 127.0.0.1,1433 -U sa -P $sa -i .\sql\00_create_database.sql
sqlcmd -S 127.0.0.1,1433 -U sa -P $sa -i .\sql\10_seed_data.sql
sqlcmd -S 127.0.0.1,1433 -U sa -P $sa -i .\sql\20_sp_search.sql
sqlcmd -S 127.0.0.1,1433 -U sa -P $sa -i .\sql\30_sp_crud.sql
```

### Option B — Use an existing SQL Server

* Create a database `FlowerShop` and run the same scripts as above.

---

## 3) Configure the app

Update the connection string **Default** (Development/Production as appropriate):

```json
{
  "ConnectionStrings": {
    "Default": "Server=127.0.0.1,1433;Database=FlowerShop;User Id=sa;Password=Your_strong_password_123!;TrustServerCertificate=True"
  }
}
```

Localization (already configured): default culture is **el-GR** so decimals like `3,50` are accepted. Change to `en-US` if you prefer `3.50`.

---

## 4) Build, Test, Run

```powershell
# From repository root
 dotnet restore
 dotnet build
 dotnet test                 # runs service tests
 dotnet run --project .\src\Web\FlowerShop.Web.csproj
```

Open: `http://localhost:5123/` → **Home** page. Click **Go to Flower List** to open the searchable list.

> Navbar also contains a **Flowers** link. The list supports text search, category filter, min/max price, sorting (clickable headers), and pagination.

---

## 5) Features

* **DB schema**: `inv.Category` ↔ `inv.Flower` (FK), constraints, indexes, computed `IsInStock`
* **Stored procedures**: search/sort/pagination (`inv.usp_Flower_Search`), CRUD for Flower & Category
* **EF Core**: code‑first models mapped to existing tables (no migrations required)
* **Service layer**: CRUD + sproc search; optimistic concurrency via `rowversion`
* **UI**: Home page, **List** with filters/sort/pager, **Details**, **Create/Edit** (image upload & “stay on Edit” after save), **Delete** page (plus optional modal)
* **Unit tests**: sample tests for services with EF InMemory

---

## 6) Project structure

```
src/
  Domain/                 # POCO entities (Category, Flower)
  Infrastructure/         # DbContext (EF Core), mappings
  Application/            # Services (IFlowerService, ICategoryService, implementations)
  Web/                    # MVC app, controllers, views, models
    Views/
      Flowers/            # Index, Details, Create, Edit, Delete
      Categories/         # (if enabled) Index, Details, Create, Edit, Delete
sql/                      # 00_create_database.sql, 10_seed_data.sql, 20_sp_search.sql, 30_sp_crud.sql
tests/                    # xUnit tests for services
```

---

## 7) Stored procedure notes

* `inv.usp_Flower_Search` uses safe dynamic SQL for ORDER BY and supports `@q`, `@categoryId`, `@minPrice`, `@maxPrice`, `@sort` (name|price|category|createdat), `@dir` (ASC|DESC), `@page`, `@pageSize`.
* If you need precise pager totals, extend the sproc to also **SELECT TotalCount** and modify the service to return it; the UI already renders based on the `Total` value.

---

## 8) Images

Images are saved under `wwwroot/images/` with GUID filenames; only the `ImageUrl` is stored in DB. Edit page allows **replace** and **delete image**.

---

## 9) DevOps

### Docker Compose (optional)

See `docker-compose.yml` sample to start SQL Server and the app together. Provide `ConnectionStrings__Default` via env var.

### GitHub Actions (optional)

Basic workflow `.github/workflows/build.yml` runs restore/build/test on push/PR.

### Rotate SQL SA password

```powershell
# Find container name
 docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}"
# Exec into the container and change SA password
 docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "OLD" -C -Q "ALTER LOGIN [sa] WITH PASSWORD='NEW!123'"
```

Update your connection strings after rotation.

---

## 10) Troubleshooting

* **FromSql non‑composable error**: don’t call `Include`/`OrderBy` on results of `EXEC`. The service materializes first and bulk‑loads categories.
* **Decimal validation (comma)**: app defaults to `el-GR`; if you prefer dot, set culture to `en-US` in `Program.cs`.
* **Port 1433 already in use**: stop other SQL instances or map a different host port.
* **Images not saving**: ensure `wwwroot/images` exists (controller creates it if missing) and the app has write permissions.

---

## 11) License

MIT (or your choice).
