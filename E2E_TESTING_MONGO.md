# E2E Testing Guide (MongoDB)

End-to-end testing of the NDjango.Admin Admin Dashboard using the MongoDB sample project and Playwright MCP.

## Prerequisites

```bash
# MongoDB must be running with replica set
docker compose up -d mongo mongoClusterSetup

# Wait for mongoClusterSetup to complete, then start the sample project
cd sample-project-mongodb/src && dotnet run -- api
```

- **App URL:** `http://localhost:8001/admin/`
- **DB:** MongoDB on `localhost:27017`, database `SampleProjectMongo`
- **Auto-setup:** The app seeds sample data on startup via `DataSeeder` (only if collections are empty)
- **Authentication:** None. The MongoDB sample project has `RequireAuthentication = false` and uses `AllowAllAdminDashboardAuthorizationFilter`
- **Read-only:** The MongoDB provider V1 is **read-only** — no create, edit, or delete operations

If port 8001 is already in use, kill the existing process first:
```bash
lsof -ti:8001 | xargs kill -9
```

## Playwright MCP Setup

Use the `mcp__playwright__browser_*` tools. Key tools:

| Tool | Purpose |
|---|---|
| `browser_navigate` | Go to a URL |
| `browser_snapshot` | Get accessibility tree (preferred over screenshots) |
| `browser_click` | Click an element by `ref` |

**Known issue:** If Chrome fails to launch with "Opening in existing browser session", clear the stale profile:
```bash
rm -rf ~/Library/Caches/ms-playwright/mcp-chrome-*
```

## Key Differences from EF Core Sample

| Aspect | EF Core (`sample-project`) | MongoDB (`sample-project-mongodb`) |
|---|---|---|
| Port | 8000 | 8001 |
| Database | SQL Server | MongoDB |
| Primary keys | `int` auto-increment | `ObjectId` (24-char hex string) |
| Authentication | Cookie-based login (admin/admin) | None (AllowAll) |
| Read-only | No (full CRUD) | **Yes** (list + detail only) |
| FK relationships | Lookup popups | Plain ObjectId text (no lookups) |
| Nested collections | Not applicable | Shown as JSON string (e.g., `IngredientIds`) |
| Auth entities | 5 auth models in sidebar | None |

## Dashboard Structure

The admin home at `/admin/` shows models organized by `EntityGroups`:

### Restaurant Group

| Model | URL prefix | Fields | `IAdminSettings` |
|---|---|---|---|
| Category | `/admin/Category/` | Name, Description | `SearchFields`: Name, Description |
| Restaurant | `/admin/Restaurant/` | Name, Address, Phone | `SearchFields`: Name |
| RestaurantProfile | `/admin/RestaurantProfile/` | RestaurantId (ObjectId), Website, OpeningHours, Capacity | None |
| MenuItem | `/admin/MenuItem/` | RestaurantId (ObjectId), Name, Description, Price, IsAvailable, IngredientIds (JSON) | None |
| Ingredient | `/admin/Ingredient/` | Name, IsAllergen | None |

### Shop Group

| Model | URL prefix | Fields | `IAdminSettings` |
|---|---|---|---|
| Gift | `/admin/Gift/` | Name, IsWrapped, TrackingCode (Guid), Price, Barcode, Weight, Rating, QuantityInStock, MinAge, ShippedAt (DateTimeOffset), Description, Notes | None |

### Seeded Data

The `DataSeeder` populates these collections on first startup:

| Collection | Count | Examples |
|---|---|---|
| categories | 3 | Italian, Japanese, Mexican |
| restaurants | 2 | Bella Napoli, Tokyo Garden |
| restaurantProfiles | 2 | One per restaurant |
| ingredients | 5 | Mozzarella, Tomato Sauce, Fresh Salmon, Soy Sauce, Basil |
| menuItems | 4 | Margherita Pizza, Spaghetti Carbonara, Salmon Sashimi, Miso Ramen |
| gifts | 2 | Gourmet Chocolate Box, Ceramic Tea Set |

## URL Patterns (Read-Only)

| Action | URL | Method |
|---|---|---|
| Dashboard home | `/admin/` | GET |
| List | `/admin/{Model}/` | GET |
| Detail (view) | `/admin/{Model}/{objectId}/change/` | GET |

**Not available in V1:** Add (`/add/`), POST create/update, Delete (`/delete/`), Bulk actions.

## ObjectId Primary Keys

MongoDB documents use `ObjectId` as the primary key (the `_id` field). In the dashboard:

- List views show `Id` as a 24-character hex string (e.g., `683f1a2b4c5d6e7f8a9b0c1d`)
- The `Id` column links to the detail view: `/admin/{Model}/{objectId}/change/`
- `ObjectId` values are hidden by default (`HidePrimaryKeys = true`) — they appear as the row link but not as a separate visible column
- `RestaurantId` on RestaurantProfile and MenuItem shows as a plain ObjectId string (no FK lookup popup)

## Verification Checklist

### Phase 1: Dashboard Home

1. **Dashboard loads** — Navigate to `http://localhost:8001/admin/`. Verify:
   - No login redirect (no authentication)
   - Page title contains "Sample Admin (MongoDB)"
   - Two entity group sections visible: "Restaurant" and "Shop"
2. **Restaurant group** — Verify 5 entities listed: Categories, Restaurants, Restaurant Profiles, Menu Items, Ingredients
3. **Shop group** — Verify 1 entity listed: Gifts
4. **No auth entities** — Verify there is NO "Authentication and Authorization" section
5. **No Add/Change links** — Since V1 is read-only, verify entity links go to list views only. There should be no "Add" links on the dashboard home (the `IsReadOnly` option disables them)

### Phase 2: Sidebar

6. **Sidebar present** — Navigate to any entity list. Verify the left sidebar exists with:
   - A "Filter models..." text input
   - "Restaurant" heading with 5 entity links
   - "Shop" heading with 1 entity link (Gift)
   - No "Authentication and Authorization" heading
7. **Sidebar filter** — Type "cat" in the filter input. Verify only "Categories" remains visible

### Phase 3: List Views with Seeded Data

#### Category List

8. **Category list loads** — Navigate to `/admin/Category/`. Verify:
   - Page title: "Categories | Sample Admin (MongoDB)"
   - Record count: "3 categories"
   - Table shows 3 rows: Italian, Japanese, Mexican
9. **Category columns** — Verify visible columns include: CreatedAt, Description, Name, UpdatedAt (Id is hidden by default)
10. **Row links** — Each row should have a clickable link (on the first visible column or Id) pointing to `/admin/Category/{objectId}/change/`

#### Restaurant List

11. **Restaurant list loads** — Navigate to `/admin/Restaurant/`. Verify:
    - Record count: "2 restaurants"
    - Table shows: Bella Napoli, Tokyo Garden
12. **Restaurant columns** — Verify columns: Address, Name, Phone (plus timestamps)

#### Ingredient List

13. **Ingredient list** — Navigate to `/admin/Ingredient/`. Verify:
    - Record count: "5 ingredients"
    - Table shows: Mozzarella, Tomato Sauce, Fresh Salmon, Soy Sauce, Basil
14. **Boolean column** — Verify `IsAllergen` column shows `True` for Mozzarella, Fresh Salmon, Soy Sauce and `False` for Tomato Sauce, Basil

#### RestaurantProfile List

15. **RestaurantProfile list** — Navigate to `/admin/RestaurantProfile/`. Verify:
    - Record count: "2 restaurant profiles"
    - Table shows 2 profiles with Capacity, OpeningHours, Website columns
16. **ObjectId reference** — Verify `RestaurantId` column shows ObjectId hex strings (not integer IDs, not restaurant names)

#### MenuItem List

17. **MenuItem list** — Navigate to `/admin/MenuItem/`. Verify:
    - Record count: "4 menu items"
    - Table shows: Margherita Pizza, Spaghetti Carbonara, Salmon Sashimi, Miso Ramen
18. **Price column** — Verify Price values display correctly (e.g., 14.99, 16.50, 18.00, 15.00)
19. **Boolean column** — Verify `IsAvailable` shows `True` for all items
20. **Nested collection** — Verify `IngredientIds` column displays as a JSON array string (e.g., `["683f...", "683f...", "683f..."]`). This is the embedded ObjectId list rendered as JSON.

#### Gift List

21. **Gift list** — Navigate to `/admin/Gift/`. Verify:
    - Record count: "2 gifts"
    - Table shows: Gourmet Chocolate Box, Ceramic Tea Set
22. **Diverse types** — Verify these columns render:
    - `IsWrapped`: `True` / `False`
    - `TrackingCode`: Guid string (e.g., `a1b2c3d4-...`)
    - `Price`: decimal (29.99, 45.00)
    - `Barcode`: long integer
    - `Weight`: double (0.5, 1.2)
    - `Rating`: float (4.8, 4.5)
    - `QuantityInStock`: short (150, 40)
    - `MinAge`: byte (3, 12)
    - `ShippedAt`: DateTimeOffset with timezone info

### Phase 4: Detail Views

23. **Category detail** — Click a category row link. Verify:
    - URL matches `/admin/Category/{objectId}/change/`
    - Page shows all fields as **read-only** (no editable text inputs)
    - Fields include: Name, Description, CreatedAt, UpdatedAt
    - There are NO "Save" buttons (read-only mode)
    - There is NO "Delete" link
24. **Restaurant detail** — Click a restaurant row. Verify read-only fields: Name, Address, Phone
25. **MenuItem detail** — Click a menu item row. Verify:
    - `RestaurantId` shows as an ObjectId hex string (no lookup popup)
    - `IngredientIds` shows as a JSON array string
    - `Price` shows the decimal value
    - `IsAvailable` shows True/False
26. **Gift detail** — Click a gift row. Verify all diverse types display correctly:
    - `TrackingCode` as Guid
    - `ShippedAt` as DateTimeOffset with timezone
    - Numeric types (Barcode, Weight, Rating, QuantityInStock, MinAge) display correctly

### Phase 5: Conditional Search

27. **Search box visible (Category)** — Navigate to `/admin/Category/`. Verify a search box ("Search..." textbox + "Search" button) is present. Category has `SearchFields => new(x => x.Name, x => x.Description)`.
28. **Search box visible (Restaurant)** — Navigate to `/admin/Restaurant/`. Verify search box is present. Restaurant has `SearchFields => new(x => x.Name)`.
29. **Search box hidden (Ingredient)** — Navigate to `/admin/Ingredient/`. Verify there is **no** search box. Ingredient does not implement `IAdminSettings`.
30. **Search box hidden (MenuItem)** — Navigate to `/admin/MenuItem/`. Verify no search box.
31. **Search box hidden (RestaurantProfile)** — Navigate to `/admin/RestaurantProfile/`. Verify no search box.
32. **Search box hidden (Gift)** — Navigate to `/admin/Gift/`. Verify no search box.
33. **Search filters correctly** — Navigate to `/admin/Category/?q=Italian`. Verify:
    - Only "Italian" row appears
    - Count shows "1 category"
    - "Japanese" and "Mexican" are not in the results
34. **Search on Restaurant** — Navigate to `/admin/Restaurant/?q=Bella`. Verify only "Bella Napoli" appears, count "1 restaurant".
35. **Search no match** — Navigate to `/admin/Category/?q=nonexistent`. Verify 0 results, count shows "0 categories".
36. **Search ignored on non-searchable** — Navigate to `/admin/Ingredient/?q=something`. Verify all 5 ingredients still display (the `?q=` param is ignored).

### Phase 6: Sorting

37. **Sort ascending** — Navigate to `/admin/Category/?sort=Name&dir=asc`. Verify rows ordered: Italian, Japanese, Mexican.
38. **Sort descending** — Navigate to `/admin/Category/?sort=Name&dir=desc`. Verify rows ordered: Mexican, Japanese, Italian.
39. **Sort column headers** — On any list view, verify column headers are clickable links with `?sort={FieldName}&dir=asc` URLs.
40. **Active sort indicator** — When sorting is active, verify the sorted column header shows an arrow indicator.

### Phase 7: Read-Only Enforcement

41. **No Add button on list** — On any entity list view, verify there is **no** "Add {entity}" link in the toolbar.
42. **No action bar** — On list views, verify there is **no** action dropdown, no checkboxes, no "Go" button (bulk actions disabled in read-only mode).
43. **No Save buttons on detail** — On detail/change views, verify there are **no** "Save", "Save and add another", or "Save and continue editing" buttons.
44. **No Delete link on detail** — On detail views, verify there is **no** "Delete" link.
45. **Add URL returns error** — Navigate directly to `/admin/Category/add/`. Verify it does NOT render a writable form (either 404, 403, or the form is read-only with no submit).

### Phase 8: Pagination

46. **Small collections** — Navigate to `/admin/Ingredient/` (5 items). Verify no pagination controls appear (all items fit on one page with default 25 per page).
47. **Record count accurate** — Verify each list view shows the correct count:
    - Categories: 3
    - Restaurants: 2
    - RestaurantProfiles: 2
    - Ingredients: 5
    - MenuItems: 4
    - Gifts: 2

### Phase 9: Breadcrumbs and Navigation

48. **List breadcrumb** — On `/admin/Category/`, verify breadcrumb shows: Home > Categories
49. **Detail breadcrumb** — On `/admin/Category/{id}/change/`, verify breadcrumb shows: Home > Category > {objectId}
50. **Home link** — Click "Home" in the breadcrumb. Verify redirect to `/admin/`
51. **Entity link** — Click "Category" in the detail breadcrumb. Verify redirect to `/admin/Category/` list

## What's NOT Tested (V1 Limitations)

- **Create operations** — `POST /admin/{Model}/add/` not supported
- **Update operations** — `POST /admin/{Model}/{id}/change/` not supported
- **Delete operations** — `POST /admin/{Model}/{id}/delete/` not supported
- **Bulk actions** — No checkboxes, no action dropdown, no bulk delete
- **FK lookup popups** — ObjectId references show as plain text, no popup navigation
- **Authentication** — No login/logout, no users, no permissions
- **SAML SSO** — Not applicable
- **Nested document expand/collapse UI** — V1 renders nested objects as flat JSON strings; the expandable UI is planned for a future version
