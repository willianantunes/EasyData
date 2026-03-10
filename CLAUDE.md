# NDjango.Admin

## What This Is

NDjango.Admin — a Django-admin-inspired CRUD dashboard for ASP.NET Core + Entity Framework Core. Auto-generates a full admin interface from a `DbContext`: list views with pagination/search/sorting, create/edit forms with FK dropdowns, and delete confirmations. All server-rendered HTML, no client-side framework.

## Build & Test Commands

```bash
# Start SQL Server (required for integration tests and sample project)
docker compose up -d db

# Build everything
dotnet build NDjango.Admin.sln

# Run all tests (~217 tests)
dotnet test NDjango.Admin.sln

# Run a single test project
dotnet test test/NDjango.Admin.AspNetCore.AdminDashboard.Tests/

# Run a single test
dotnet test NDjango.Admin.sln --filter "FullyQualifiedName~ForeignKeyTests.RestaurantAddForm_RendersCategory_DropdownAsync"

# Run the sample project (requires SQL Server running)
cd sample-project/src && dotnet run -- api
# Dashboard at http://localhost:8000/admin/
```

**Solution file:** `NDjango.Admin.sln` — includes all source and test projects.

## Architecture

### Project Dependency Graph

```
NDjango.Admin.Core                              # Metadata model (MetaData, MetaEntity, MetaEntityAttr)
├── NDjango.Admin.AspNetCore                    # ASP.NET Core integration (NDjangoAdminManager, middleware)
├── NDjango.Admin.EntityFrameworkCore.Relational # EF Core → metadata loader (DbContextMetaDataLoader)
│
NDjango.Admin.AspNetCore.AdminDashboard         # The admin dashboard (depends on all three above)
```

All projects target **net8.0**.

### Request Flow

`AdminDashboardMiddleware` intercepts requests under the configured base path (e.g., `/admin`). It:
1. Checks authorization filters
2. Matches the URL against `DashboardRoutes` (regex-based route table)
3. Dispatches to the appropriate handler:
   - `RazorViewDispatcher` — renders HTML views (dashboard home, list, form, delete confirmation)
   - `ApiDispatcher` — handles form POST submissions (create, update, delete) and JSON lookups
   - `EmbeddedResourceDispatcher` — serves CSS/JS from embedded resources
   - `SamlDispatcher` — handles `/saml/init/` (SP-initiated redirect to IdP) and ACS callback (validates SAMLResponse, creates/updates user, syncs groups, sets cookie)

### Key Data Flow

`DbContextMetaDataLoader` scans a `DbContext` to produce `MetaData` (entities + attributes). The dashboard reads this metadata to auto-generate views. CRUD operations go through `NDjangoAdminManager` → `NDjangoAdminManagerEF` which uses EF Core directly.

### Important Metadata Properties

On `MetaEntityAttr`:
- `ShowOnCreate` / `ShowOnEdit` / `ShowOnView` — control field visibility per view
- `IsEditable` — false for value-generated fields (renders as readonly text, excluded from form POST)
- `Kind == EntityAttrKind.Lookup` — FK navigation properties; `DataAttr` points to the underlying FK data attribute (e.g., `Restaurant` lookup → `RestaurantId`)
- `ValueGenerated` flags on EF Core properties drive `ShowOnCreate`/`ShowOnEdit`/`IsEditable`

### Form POST Handling

`ApiDispatcher.FormToJObject()` maps HTML form fields to a `JObject` for `NDjangoAdminManagerEF`. For FK dropdowns, it uses `attr.DataAttr.PropName` (e.g., `CategoryId`) — the select element `name` attribute must match this.

### URL Routing Pattern

Routes are Django-style: `/{entityId}/` (list), `/{entityId}/add/` (create), `/{entityId}/{id}/change/` (edit), `/{entityId}/{id}/delete/` (delete). The `entityId` is the short class name from `MetaEntity.Id` (e.g., `Category`, `MenuItem`).

## Testing

Tests use **xUnit** + **FluentAssertions** + **Microsoft.AspNetCore.TestHost**. The `AdminDashboardFixture` creates a unique SQL Server database per test run and seeds restaurant domain data (Category, Restaurant, RestaurantProfile, Ingredient, MenuItem). Test fixture implements `IDisposable` to drop the DB after tests.

SQL Server must be running on `localhost:1433` (sa/Password1) — use `docker compose up -d db`.

## Auto-Generated Field Handling

EF Core properties with `HasDefaultValueSql()` or `ValueGeneratedOnAdd()` are automatically hidden from create forms and rendered as readonly text on edit forms. This is the intended pattern for timestamps (`CreatedAt`/`UpdatedAt`) and identity columns.

## SAML SSO

When `EnableSaml = true`, the dashboard adds SAML 2.0 SSO alongside password login. The ACS callback (`/api/security/saml/callback`) is registered as a separate middleware branch via `app.Map()` outside the admin base path. IdP metadata can be auto-fetched at startup from `SamlMetadataUrl`, or certificate/SSO URL can be set manually. Group UUIDs from the SAML response are matched against `auth_group.name` to assign permissions. Uses the `AspNetSaml` NuGet package (v2.1.4).

## Time-Limited Pagination COUNT

On every list view the dashboard runs `SELECT COUNT(*)` to display the total record count and compute pagination. On tables with millions of rows this query can take seconds.

`PaginationCountTimeoutMs` (default 200 ms, configurable on `AdminDashboardOptions` and `NDjangoAdminOptions`) caps how long the COUNT query may run. If it exceeds the timeout, the dashboard returns the fallback value `NDjangoAdminOptions.PaginationCountFallbackValue` (9,999,999,999) instead of blocking the page. Data rows load independently and are unaffected.

Key implementation details:
- `CountRecordsAsync<T>` in `NDjangoAdminManagerEF.cs` uses `CancellationTokenSource.CreateLinkedTokenSource` to combine the HTTP request token with the timeout CTS, so client disconnects also cancel the query
- `catch (OperationCanceledException) when (!callerToken.IsCancellationRequested)` ensures only timeout-caused cancellations return the fallback; caller cancellations propagate
- A second `catch (Exception) when (...)` handles SQL Server wrapping cancellation in `SqlException`
- Setting `PaginationCountTimeoutMs = -1` disables the timeout entirely
- `ViewRenderer` renders a sliding window of max 10 pagination links with ellipsis to prevent OOM from the fallback count

## Known Gaps

- **SP-initiated SAML login** does not work with AWS IAM Identity Center (IdP-initiated works). See `sample-project-sso/README.md` for details.
- **M2M relationships** not yet supported in the dashboard (planned Phase 5)
- Price fields with decimal types may show locale-specific formatting issues in HTML number inputs
