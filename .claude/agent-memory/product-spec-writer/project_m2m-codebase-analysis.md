---
name: M2M Codebase Analysis
description: Analysis of the NDjango.Admin codebase for M2M relationship support, identifying key components, limitations, and patterns
type: project
---

## Key Architecture Findings for M2M Support

### Current FK Handling Flow (EF Core)
1. `DbContextMetaDataLoader.LoadFromDbContext()` processes entities in two passes:
   - First pass: `ProcessEntityType()` creates entities and their scalar attributes
   - Second pass: iterates navigations via `ProcessNavigationProperty()`
2. `ProcessNavigationProperty()` explicitly skips collection navigations (`if (navigation.IsCollection) return;`)
3. For non-collection navigations, it creates a `Lookup` attribute linked to the FK data attribute

### Composite Key Handling
- `NDjangoAdminManagerEF.GetKeys()` already supports multiple PK properties - extracts all properties where `prop.IsPrimaryKey()`
- `MetaEntity` does NOT have a concept of composite PKs at the metadata level - it just marks individual attrs as `IsPrimaryKey = true`
- **Critical issue**: The URL routing uses `/{entityId}/{id}/change/` with a SINGLE id segment. Composite keys need a URL-safe encoding scheme.
- `ApiDispatcher.HandleUpdateAsync()` gets `recordId = match.Values["id"]` as a single string

### Sample Project State
- EF Core sample has implicit M2M: `MenuItem.Ingredients` via `entity.HasMany(mi => mi.Ingredients).WithMany(i => i.MenuItems)` - EF auto-creates a shadow junction table
- MongoDB sample stores `List<ObjectId> IngredientIds` on MenuItem - shown as read-only JSON

### Test Infrastructure
- EF Core integration tests use `TestDbContext` with InMemory SQLite
- Test entities in `TestDbContext` do NOT currently have M2M relationships
- MongoDB tests are in a separate `NDjango.Admin.MongoDB.Tests` project

### Routing Constraint
- All routes use a single `{id}` capture group: `^/([^/]+?)/([^/]+?)/change/$`
- For composite keys, the ID must be serialized into a single URL segment

**Why:** Understanding these constraints is critical for correctly scoping the M2M implementation. The composite key URL encoding is the most technically challenging aspect.

**How to apply:** When producing the M2M spec, address composite key URL encoding explicitly as a prerequisite that affects routing, ApiDispatcher, and RazorViewDispatcher.
