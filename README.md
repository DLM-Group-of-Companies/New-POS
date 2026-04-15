# NLI-POS (NobleLife International Point of Sale)

A **Point of Sale and back-office system** for multi-office retail: orders, inventory, customers, payments, reporting, and audit trails.

Built with **ASP.NET Core 8**, **Razor Pages**, **ASP.NET Core Identity**, **Entity Framework Core**, and **MySQL** (via **Pomelo**).

---

## Features

- **Multi-office** — Offices, locations, and user–office access
- **Sales** — Orders, carts, payments (multiple methods), voiding, sales sources
- **Catalog & pricing** — Products, combos, conversions, price lists, promos
- **Inventory** — Stockrooms, warehouses, transfers, stock levels and history
- **Customers** — Profiles, classes, and related utilities (e.g. countries, time zones)
- **Reporting** — Sales and operational reports; Excel export (EPPlus / ClosedXML)
- **Security** — Identity with roles, confirmed accounts, lockout; audit logging

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [MySQL 8](https://dev.mysql.com/downloads/) (or compatible hosted MySQL)
- [LibMan](https://learn.microsoft.com/en-us/aspnet/client-side/libman/libman-cli) CLI (for restoring client libraries from `libman.json`)
- Git

Optional: Visual Studio 2022 or VS Code with the C# extension.

---

## Getting started

### 1. Clone the repository

```bash
git clone https://github.com/YOUR_USERNAME/YOUR_REPO.git
cd YOUR_REPO
```

### 2. Configure the database

The app reads the **`NLPOSLiveConn`** connection string (see `Program.cs`). Set it in `appsettings.json`, **User Secrets**, or environment variables—**do not commit production credentials**.

Example (local MySQL):

```json
{
  "ConnectionStrings": {
    "NLPOSLiveConn": "Server=127.0.0.1;Database=nlpos;Uid=YOUR_USER;Pwd=YOUR_PASSWORD;Connection Timeout=60;"
  }
}
```

For local development you can use:

```bash
dotnet user-secrets set "ConnectionStrings:NLPOSLiveConn" "Server=127.0.0.1;Database=nlpos;Uid=root;Pwd=YOUR_PASSWORD;Connection Timeout=60;"
```

### 3. Restore client libraries

```bash
libman restore
```

### 4. Apply database migrations

```bash
dotnet ef database update
```

Ensure the EF Core tools are available (`dotnet tool install --global dotnet-ef` if needed).

### 5. Run the app

```bash
dotnet run
```

Default URLs from `Properties/launchSettings.json`:

- **HTTPS:** `https://localhost:7216`
- **HTTP:** `http://localhost:5136`

---

## Project layout

| Area | Purpose |
|------|---------|
| `Areas/Identity/` | Login, registration, Identity UI |
| `Controllers/` | APIs (e.g. sales-related endpoints) |
| `Data/` | `ApplicationDbContext`, seeding |
| `Models/` | Entities and view models |
| `Pages/` | Razor Pages (main UI) |
| `Services/` | Business and reporting services |
| `Migrations/` | EF Core migrations |
| `wwwroot/` | Static assets; LibMan output under `wwwroot/lib/` |

Entry point: `Program.cs`. Project file: `NLI-POS.csproj`.

---

## Configuration notes

- **Logging** — `appsettings.json` / `appsettings.Development.json`
- **CORS** — A sample policy exists in `Program.cs` (adjust origins for your deployment)
- **EPPlus** — Non-commercial license context is set in `Program.cs`; align usage with [EPPlus licensing](https://epplussoftware.com/) for your scenario

---

## Common commands

```bash
dotnet build
dotnet watch run
dotnet publish -c Release -o ./publish
```

Create a new migration after model changes:

```bash
dotnet ef migrations add YourMigrationName
dotnet ef database update
```

---

## Key NuGet dependencies

See `NLI-POS.csproj` for versions. Highlights:

- `Pomelo.EntityFrameworkCore.MySql` — MySQL provider  
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` — Identity  
- `EPPlus`, `ClosedXML` — Spreadsheet generation  
- `MailKit` — Email  
- `AspNetCoreHero.ToastNotification`, `NToastNotify` — Notifications  
- `System.Linq.Dynamic.Core`, `TimeZoneConverter` — Queries and time zones  

---

## Security

- Keep database passwords and API secrets out of source control; prefer User Secrets or a secure store in production.
- Use HTTPS in production and restrict CORS to known origins.
- Review Identity options (password policy, lockout, confirmed accounts) for your environment.

---

## Contributing

1. Fork the repository and create a branch for your change.  
2. Make focused commits with clear messages.  
3. Open a pull request describing the change and how you tested it.

---

## Troubleshooting

| Issue | What to try |
|-------|-------------|
| Cannot connect to MySQL | Check server is running, firewall, user permissions, and the `NLPOSLiveConn` string. |
| `dotnet ef` not found | `dotnet tool install --global dotnet-ef` |
| Missing Bootstrap under `wwwroot/lib` | Run `libman restore` |
| Port in use | Change URLs in `Properties/launchSettings.json` or run `dotnet run --urls "http://localhost:OTHER_PORT"` |

---

## License

> **License:** Proprietary — © 2026 NobleLife International. All rights reserved. Unauthorized use, copying, or distribution is strictly prohibited.

---

*Last updated: April 2026*
