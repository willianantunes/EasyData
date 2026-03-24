# Architecture

## Project Dependency Graph

```
NDjango.Admin.Core                                    # Metadata model (MetaData, MetaEntity, MetaEntityAttr)
├── NDjango.Admin.AspNetCore                          # ASP.NET Core integration
│
NDjango.Admin.AspNetCore.AdminDashboard.Core          # Provider-agnostic dashboard (middleware, dispatchers, views, wwwroot)
│   → NDjango.Admin.Core
│   → NDjango.Admin.AspNetCore
│
NDjango.Admin.AspNetCore.AdminDashboard               # EF Core dashboard shell (auth DB, DI extension, composite manager)
│   → NDjango.Admin.AspNetCore.AdminDashboard.Core
│   → NDjango.Admin.EntityFrameworkCore.Relational
│
NDjango.Admin.EntityFrameworkCore.Relational           # EF Core provider (DbContextMetaDataLoader, NDjangoAdminManagerEF)
│   → NDjango.Admin.Core
│
NDjango.Admin.MongoDB                                  # MongoDB provider (MongoMetaDataLoader, NDjangoAdminManagerMongo)
    → NDjango.Admin.Core
    → NDjango.Admin.AspNetCore.AdminDashboard.Core
```

## Project Structure

```
src/
  NDjango.Admin.Core/                                  # Core metadata model
  NDjango.Admin.AspNetCore/                            # ASP.NET Core integration
  NDjango.Admin.EntityFrameworkCore.Relational/        # EF Core provider (metadata loader, manager, filters)
  NDjango.Admin.AspNetCore.AdminDashboard.Core/        # Provider-agnostic dashboard core
    Authentication/                               # Login, permissions, password hashing, IAdminAuthQueries
    Authorization/                                # Auth filters
    Configuration/                                # AdminDashboardOptions, UseNDjangoAdminDashboard()
    Dispatchers/                                  # View rendering and API handlers
    Middleware/                                   # Request pipeline
    Routing/                                      # URL dispatch
    Services/                                     # Metadata service, entity grouping
    ViewModels/                                   # Form and list models
    wwwroot/                                      # Embedded CSS/JS
  NDjango.Admin.AspNetCore.AdminDashboard/             # EF-specific dashboard shell
    Authentication/                               # AuthDbContext, AuthBootstrapper, AuthStorageQueries
    Configuration/                                # AddNDjangoAdminDashboard<TDbContext>()
    Services/                                     # CompositeNDjangoAdminManager
  NDjango.Admin.MongoDB/                               # MongoDB provider
    Authentication/
      Entities/                                   # MongoAuthUser, MongoAuthGroup, MongoAuthPermission, etc.
      Storage/                                    # MongoAuthStorageQueries, MongoAuthStorageInitializer, AuthCollectionNames
    Extensions/                                   # UseMongoDB(), AddNDjangoAdminDashboardMongo()
    Filters/                                      # MongoSubstringFilter
    Services/                                     # NDjangoAdminManagerMongo

test/                                             # Integration & unit tests
sample-project/                                   # EF Core example app (SQL Server)
sample-project-mongodb/                           # MongoDB example app
sample-project-sso/                               # SSO example (AWS IAM Identity Center)
```

## Request Flow

`AdminDashboardMiddleware` intercepts requests under the configured base path (e.g., `/admin`). It:
1. Checks authorization filters
2. Matches the URL against `DashboardRoutes` (regex-based route table)
3. Dispatches to the appropriate handler:
    - `RazorViewDispatcher` — renders HTML views (dashboard home, list, form, delete confirmation)
    - `ApiDispatcher` — handles form POST submissions (create, update, delete) and JSON lookups
    - `EmbeddedResourceDispatcher` — serves CSS/JS from embedded resources
    - `SamlDispatcher` — handles `/saml/init/` (SP-initiated redirect to IdP) and ACS callback (validates SAMLResponse, creates/updates user, syncs groups, sets cookie)

## Key Data Flow

The dashboard is provider-agnostic. The abstract `NDjangoAdminManager` defines the contract; providers implement it:

- **EF Core**: `DbContextMetaDataLoader` scans a `DbContext` to produce `MetaData`. CRUD goes through `NDjangoAdminManagerEF`. Junction entities with composite primary keys are fully supported — both FK navigation properties produce Lookup attributes with correct `DataAttr` references.
- **MongoDB**: `MongoMetaDataLoader` scans registered document types via reflection + BSON attributes. CRUD operations go through `NDjangoAdminManagerMongo` using `collection.AsQueryable()` (LINQ3) for reads and direct MongoDB driver calls for create/update/delete. Authentication is supported via MongoDB-backed auth collections (users, groups, permissions). MongoDB junction documents use a standard single `ObjectId` PK (no composite keys).

## Important Metadata Properties

On `MetaEntity`:
- `HasCompositeKey` — computed property, `true` when 2+ attributes have `IsPrimaryKey == true` (junction/through entities)
- `GetPrimaryKeyDataAttributes()` — returns non-Lookup PK attributes (used by dispatchers to encode/decode composite keys)

On `MetaEntityAttr`:
- `ShowOnCreate` / `ShowOnEdit` / `ShowOnView` — control field visibility per view
- `IsEditable` — false for value-generated fields (renders as readonly text, excluded from form POST)
- `Kind == EntityAttrKind.Lookup` — FK navigation properties; `DataAttr` points to the underlying FK data attribute (e.g., `Restaurant` lookup → `RestaurantId`)
- `ValueGenerated` flags on EF Core properties drive `ShowOnCreate`/`ShowOnEdit`/`IsEditable`

## Form POST Handling

`ApiDispatcher.FormToJObject()` maps HTML form fields to a `JObject` for `NDjangoAdminManagerEF`. For FK fields, it uses `attr.DataAttr.PropName` (e.g., `CategoryId`) — the raw ID text input `name` attribute must match this. FK fields render as a text input with a popup lookup icon (similar to Django's `raw_id_fields`).

## URL Routing Pattern

Routes are Django-style: `/{entityId}/` (list), `/{entityId}/add/` (create), `/{entityId}/{id}/change/` (edit), `/{entityId}/{id}/delete/` (delete). The `entityId` is the short class name from `MetaEntity.Id` (e.g., `Category`, `MenuItem`).

For entities with composite primary keys (e.g., junction tables), the `{id}` segment contains comma-separated URL-encoded key values: `/{entityId}/{key1},{key2}/change/`. The `CompositeKeyEncoder` static class in `NDjango.Admin.Core` handles encoding/decoding. Individual key values containing commas are percent-encoded as `%2C`.

# Auto-Generated Field Handling

EF Core properties with `HasDefaultValueSql()` or `ValueGeneratedOnAdd()` are automatically hidden from create forms and rendered as readonly text on edit forms. This is the intended pattern for timestamps (`CreatedAt`/`UpdatedAt`) and identity columns.

MongoDB uses a convention-based approach: properties named `CreatedAt`, `UpdatedAt`, `CreatedDate`, `UpdatedDate`, `CreationDate`, or `ModificationDate` (of type `DateTime`/`DateTimeOffset`, including nullable) are treated as auto-timestamps — hidden from create forms, shown as readonly on edit, and set automatically by `NDjangoAdminManagerMongo.SetTimestamps()`. The `IsAutoTimestamp()` method in `MongoMetaDataLoader` checks both the property name and type.

MongoDB collections can be marked as read-only via `MongoCollectionDescriptor.IsReadOnly` (set via `AddCollection<T>("name", readOnly: true)`). The `MongoMetaDataLoader` checks `descriptor.IsReadOnly` and sets `entity.IsEditable = !descriptor.IsReadOnly`, making all attributes non-editable for read-only collections.

# SAML SSO

When `EnableSaml = true`, the dashboard adds SAML 2.0 SSO alongside password login. The ACS callback (`/api/security/saml/callback`) is registered as a separate middleware branch via `app.Map()` outside the admin base path. IdP metadata can be auto-fetched at startup from `SamlMetadataUrl`, or certificate/SSO URL can be set manually. Group UUIDs from the SAML response are matched against `auth_group.name` to assign permissions. Uses the `AspNetSaml` NuGet package.

# M2M Relationships

Supported via explicit junction entities (Django Admin's `through` model pattern). Junction entities appear as first-class entities in the dashboard.

**EF Core**: Junction entities use composite PKs (`HasKey(e => new { e.MenuItemId, e.IngredientId })`). `CompositeKeyEncoder` in `NDjango.Admin.Core` encodes/decodes composite keys for URLs (`/admin/MenuItemIngredient/1,3/change/`). FK lookup fields are editable on add forms, read-only on edit forms (enforced in `RazorViewDispatcher`, not metadata). Malformed keys return 400.

**MongoDB**: Junction documents use standard single `ObjectId` PK. No composite key handling. No cascade delete.