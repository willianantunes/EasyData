# EasyData Admin Dashboard

A Django-admin-inspired CRUD dashboard for ASP.NET Core + Entity Framework Core. Automatically generates a full admin interface from your `DbContext` — list views with pagination/search/sorting, create/edit forms with FK dropdowns, and delete confirmations.

## What you get

- **Dashboard home** at `/admin/` listing all entities with Add/Change links
- **List view** with search, column sorting, and pagination
- **Create/Edit forms** that auto-detect fields, hide auto-generated properties (`Id`, `CreatedAt`, `UpdatedAt`), and render FK relationships as dropdowns
- **Delete confirmation** page
- **Sidebar** with model navigation and filtering
- **Zero client-side framework dependency** — all HTML is server-rendered

## Quick start

### 1. Install packages

Add a reference to the `EasyData.AspNetCore.AdminDashboard` project (or NuGet package when published).

### 2. Register services

```csharp
// Program.cs or Startup.ConfigureServices
services.AddEasyDataAdminDashboard<AppDbContext>();
```

### 3. Add the middleware

```csharp
// Program.cs or Startup.Configure
app.UseEasyDataAdminDashboard("/admin", new AdminDashboardOptions
{
    Authorization = new[] { new AllowAllAdminDashboardAuthorizationFilter() },
    DashboardTitle = "My Admin",
});
```

That's it. Navigate to `/admin/` and you have a working admin panel.

## Configuration

### Authorization

```csharp
using EasyData.AspNetCore.AdminDashboard.Authorization;

// Allow all (development only)
new AllowAllAdminDashboardAuthorizationFilter()

// Restrict to localhost
new LocalRequestsOnlyAuthorizationFilter()

// Custom: implement IAdminDashboardAuthorizationFilter
```

### Options

| Option | Default | Description |
|---|---|---|
| `DashboardTitle` | `"Admin Dashboard"` | Title shown in the header |
| `AppPath` | `"/"` | Base path of your application |
| `DefaultRecordsPerPage` | `25` | Pagination page size |
| `IsReadOnly` | `false` | Disable all write operations |
| `EntityGroups` | `null` | Group entities in the sidebar (dictionary of group name to entity names) |

### Authentication (optional)

The dashboard supports built-in cookie-based authentication with users, groups, and permissions — similar to Django Admin. Auth tables are created automatically in your existing database on startup.

```csharp
app.UseEasyDataAdminDashboard("/admin", new AdminDashboardOptions
{
    DashboardTitle = "My Admin",
    RequireAuthentication = true,
    CreateDefaultAdminUser = true,
    DefaultAdminPassword = "admin",
});
```

When `RequireAuthentication` is enabled:
- All dashboard pages require login (unauthenticated requests redirect to `/admin/login/`)
- A default superuser `admin` is created on first startup (when `CreateDefaultAdminUser = true`)
- Permissions are auto-generated for every entity (`add_`, `view_`, `change_`, `delete_`)
- Superusers bypass all permission checks; regular users need permissions assigned through groups
- Auth entities (Users, Groups, Permissions) appear in the dashboard under "Authentication and Authorization" and are manageable through the same CRUD interface

| Option | Default | Description |
|---|---|---|
| `RequireAuthentication` | `false` | Enable login and permission enforcement |
| `CreateDefaultAdminUser` | `false` | Create an `admin` superuser on startup if it doesn't exist |
| `DefaultAdminPassword` | `"admin"` | Password for the default admin user |
| `CookieName` | `".EasyData.Admin.Auth"` | Name of the authentication cookie |
| `CookieExpiration` | `24 hours` | How long the session cookie remains valid |

**Important:** Your database must exist before the auth bootstrap runs. If you use `EnsureCreated()`, call it **before** `UseEasyDataAdminDashboard()`:

```csharp
// Correct order
using var scope = app.ApplicationServices.CreateScope();
using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.EnsureCreated();

app.UseEasyDataAdminDashboard("/admin", new AdminDashboardOptions { ... });
```

### Auto-generated fields

Fields configured with `ValueGeneratedOnAdd` or `HasDefaultValueSql` in EF Core are automatically:
- Hidden from create forms
- Shown as read-only text on edit forms

Example for timestamp fields:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes()
        .Where(t => typeof(StandardEntity).IsAssignableFrom(t.ClrType)))
    {
        modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(StandardEntity.CreatedAt))
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity(entityType.ClrType)
            .Property(nameof(StandardEntity.UpdatedAt))
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
```

## Sample project

A working sample is at `./sample-project`. To run it:

```bash
# Start SQL Server (required)
docker compose up -d db

# Run the app
cd sample-project/src
dotnet run -- api
```

Open `http://localhost:8000/admin/` to see the dashboard with restaurant domain models (Category, Restaurant, RestaurantProfile, Ingredient, MenuItem).

## Running tests

```bash
# Start SQL Server
docker compose up -d db

# Run all tests
dotnet test EasyData.sln
```

## Project structure

```
src/
  EasyData.Core/                              # Core metadata model
  EasyData.AspNetCore/                        # ASP.NET Core integration
  EasyData.EntityFrameworkCore.Relational/    # EF Core metadata loader
  EasyData.AspNetCore.AdminDashboard/         # Admin dashboard (this package)
    Authentication/                           # Login, permissions, password hashing, auth DB
    Authorization/                            # Auth filters
    Configuration/                            # DI extensions and options
    Dispatchers/                              # View rendering and API handlers
    Middleware/                               # Request pipeline
    Routing/                                  # URL dispatch
    Services/                                 # Metadata, entity grouping, composite manager
    ViewModels/                               # Form and list models
    wwwroot/                                  # Embedded CSS/JS

test/                                         # Integration & unit tests
sample-project/                               # Working example app
```
