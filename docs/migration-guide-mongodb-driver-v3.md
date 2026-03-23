# Migration Guide: Upgrading MongoDB.Driver from v2.x to v3.x

## Overview

NDjango.Admin.MongoDB currently targets MongoDB.Driver v2.x (specifically `[2.19.0, 3.0.0)`) to maximize compatibility with the broadest range of existing projects. Most production .NET applications still use v2.x, and shipping the library on v2.x avoids forcing consumers into an immediate major-version upgrade.

Consider upgrading to v3.x when:

- Your application already uses MongoDB.Driver v3.x elsewhere and you want to unify the dependency.
- You need features exclusive to v3.x (e.g., the new LINQ3-only provider without the LINQ2 fallback, simplified Guid handling).
- You are starting a new project on .NET 8+ and have no legacy constraints.

## Prerequisites

| Requirement           | v2.x (current)       | v3.x (target)       |
|-----------------------|----------------------|----------------------|
| .NET                  | .NET 6+              | .NET 8+             |
| MongoDB Server        | 3.6+                 | 4.0+ recommended    |
| MongoDB.Driver NuGet  | >= 2.19.0, < 3.0.0  | >= 3.0.0            |

Ensure your project targets at least .NET 8 before attempting the upgrade to v3.x.

## Package Reference Change

Update every `.csproj` that references MongoDB.Driver.

**NDjango.Admin.MongoDB (library)**

```xml
<!-- Before -->
<PackageReference Include="MongoDB.Driver" Version="[2.19.0, 3.0.0)" />

<!-- After -->
<PackageReference Include="MongoDB.Driver" Version="3.*" />
```

**Sample project / your application**

```xml
<!-- Before -->
<PackageReference Include="MongoDB.Driver" Version="[2.19.0, 3.0.0)" />

<!-- After -->
<PackageReference Include="MongoDB.Driver" Version="3.*" />
```

If your test project references MongoDB.Driver directly, update it too. If it only receives the dependency transitively, no change is needed.

## Breaking Changes

### 1. `AsQueryable()` return type

In v2, `collection.AsQueryable()` returns `IMongoQueryable<T>`.
In v3, it returns `IQueryable<T>`.

NDjango.Admin.MongoDB already treats the result as `IQueryable<T>`, so no source changes are required. However, if your own code casts or stores the result as `IMongoQueryable<T>`, you will need to update those references.

### 2. `IMongoQueryable` and `IOrderedMongoQueryable` interfaces removed

These interfaces no longer exist in v3. Any code that references them directly will fail to compile. Replace usages with `IQueryable<T>` and `IOrderedQueryable<T>` respectively.

### 3. `using MongoDB.Driver.Linq` namespace

In v2 this import is required to bring the `AsQueryable()` extension method into scope. In v3 the method lives directly on `IMongoCollection<T>`, so the import may become unnecessary. It is safe to leave it in place -- the namespace still exists in v3, it simply contains fewer types.

### 4. LINQ2 provider removed

v3 ships only the LINQ3 provider. The `LinqProvider.V2` enum value and the `LinqProvider` client setting are removed. If your `MongoClientSettings` explicitly sets `LinqProvider = LinqProvider.V2`, you must remove that line. NDjango.Admin.MongoDB does not set this, so no change is needed in the library.

### 5. `GuidRepresentationMode` removed

In v2, Guid serialization can be configured globally via `BsonDefaults.GuidRepresentationMode` and `BsonDefaults.GuidRepresentation`. In v3, these global settings are removed. All Guid serialization is configured per-property using `BsonGuidRepresentation` attributes or per-serializer registration.

If your document classes rely on the global `GuidRepresentation` setting, you must add explicit `[BsonGuidRepresentation(GuidRepresentation.Standard)]` (or your desired format) attributes to each Guid property.

### 6. `MongoDB.Driver.Core` merged into `MongoDB.Driver`

In v3, the `MongoDB.Driver.Core` NuGet package is merged into `MongoDB.Driver`. If your project has a direct reference to `MongoDB.Driver.Core`, remove it after upgrading.

### 7. `MongoDB.Driver.Legacy` package removed

The legacy compatibility package (`MongoDB.Driver.Legacy`) is no longer available in v3. If your code uses types from this package (e.g., `MongoServer`, `MongoDatabase`), you must migrate to the modern `MongoClient` / `IMongoDatabase` / `IMongoCollection<T>` APIs before upgrading.

## Data Migration Considerations

### Guid serialization format

v2 defaults to `GuidRepresentation.CSharpLegacy`, which stores Guids as BSON Binary subtype 3 in a byte-order specific to the C# driver. v3 defaults to `GuidRepresentation.Unspecified` and requires explicit configuration.

If your existing MongoDB collections contain Guid fields written by v2 with the default legacy format, you have two options:

1. **Annotate properties** to continue reading the legacy format:
   ```csharp
   [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
   public Guid Id { get; set; }
   ```

2. **Migrate data** to the standard UUID format (Binary subtype 4) and use:
   ```csharp
   [BsonGuidRepresentation(GuidRepresentation.Standard)]
   public Guid Id { get; set; }
   ```

Option 1 is non-destructive and does not require any data changes. Option 2 is recommended for new projects or when you can afford a one-time data migration.

NDjango.Admin.MongoDB uses `ObjectId`-based `_id` fields for its authentication entities (users, groups, permissions), so the GUID serialization concern does not apply to the library's own collections. It may apply to your application's document classes if they use `Guid` identifiers.

## Verification

After upgrading, run the full test suite to confirm nothing is broken.

### Selective MongoDB tests

```shell
docker compose run --volume "$(PWD):/app" --rm --remove-orphans integration-tests \
  bash -c 'dotnet test NDjango.Admin.sln --settings "./runsettings.xml" \
    --filter "MongoMetaDataLoaderTests|MongoSubstringFilterTests|MongoAuthStorageInitializerTests|MongoAuthStorageQueriesTests|MongoCrudTests|MongoDashboardHomeTests|MongoListViewTests|MongoDetailViewTests|MongoSearchTests|MongoSortingTests|MongoLoginTests|MongoPermissionTests|MongoAuthUserFieldVisibilityTests|MongoUserEntityCrudTests" \
    > /tmp/test-output.txt 2>&1; \
    cat /tmp/test-output.txt | dotnet dotnet-script ./scripts/filter-failed-tests.csx'
```

### Full test suite

```shell
docker compose run --volume "$(PWD):/app" --rm --remove-orphans integration-tests \
  bash -c 'dotnet test NDjango.Admin.sln --settings "./runsettings.xml" \
    > /tmp/test-output.txt 2>&1; \
    cat /tmp/test-output.txt | dotnet dotnet-script ./scripts/filter-failed-tests.csx'
```

All existing tests should pass without modification, as the NDjango.Admin.MongoDB codebase uses only APIs that are compatible across both v2.x and v3.x.
