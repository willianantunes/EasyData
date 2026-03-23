# E2E Testing Guide (MongoDB)

End-to-end testing of the NDjango.Admin Admin Dashboard using the MongoDB sample project and Playwright MCP.

## Prerequisites

```bash
# MongoDB must be running with replica set
docker compose up --detach --wait --wait-timeout 120 --remove-orphans mongoClusterSetup

# Wait for mongoClusterSetup to complete, then start the sample project
cd sample-project-mongodb/src && dotnet run -- api
```

- **App URL:** `http://localhost:8001/admin/`
- **DB:** MongoDB on `localhost:27017`, database `SampleProjectMongo`
- **Auto-setup:** The app seeds sample data on startup via `DataSeeder` (only if collections are empty)
- **Authentication:** Cookie-based login. Default credentials: **admin / admin**
- **User collections:** Read-only — list views, detail views, search, sort, and pagination work; create/edit/delete are not available for user-defined collections
- **Auth collections:** Fully editable through the dashboard (Users, Groups, Permissions)

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
| `browser_fill_form` | Fill form fields by `ref` |

**Known issue:** If Chrome fails to launch with "Opening in existing browser session", clear the stale profile:
```bash
rm -rf ~/Library/Caches/ms-playwright/mcp-chrome-*
```

## Authentication Flow

When `RequireAuthentication = true`, all dashboard pages require login. The auth system uses cookie-based sessions with DataProtection encryption. Auth data is stored in MongoDB collections (`auth_users`, `auth_groups`, `auth_permissions`, `auth_group_permissions`, `auth_user_groups`).

### Login

1. Any unauthenticated request to `/admin/*` redirects to `/admin/login/?next={originalPath}`
2. The login page shows a form with Username and Password fields and a "Log in" button
3. POST `/admin/login/` validates credentials against the `auth_users` collection (SHA256 hash)
4. On success: sets `.NDjango.Admin.Auth` cookie, redirects to `?next` param or `/admin/`
5. On failure: re-renders login page with "Invalid credentials" error
6. Inactive users (`IsActive = false`) are treated as invalid credentials

### Logout

1. Navigate to `/admin/logout/` or click "Log out" in the header
2. Clears the auth cookie
3. Redirects to `/admin/login/`

### Auth-Exempt Paths

These paths are served without authentication: `/css/*`, `/js/*`, `/login/`, `/logout/`, `/saml/*`

## Key Differences from EF Core Sample

| Aspect | EF Core (`sample-project`) | MongoDB (`sample-project-mongodb`) |
|---|---|---|
| Port | 8000 | 8001 |
| Database | SQL Server | MongoDB |
| Primary keys | `int` auto-increment | `ObjectId` (24-char hex string) |
| Authentication | Cookie-based (admin/admin) | Cookie-based (admin/admin) |
| Auth storage | SQL Server tables | MongoDB collections |
| User collections | Full CRUD | Read-only (list + detail) |
| Auth collections | Full CRUD | Full CRUD |
| FK relationships | Lookup popups | Plain ObjectId text (no lookups) |
| Nested collections | Not applicable | Shown as JSON string (e.g., `IngredientIds`) |

## Dashboard Structure

The admin home at `/admin/` shows models organized by `EntityGroups`, plus an auto-generated "Authentication and Authorization" section for auth entities.

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

### Authentication and Authorization (auto-generated)

| Model | URL prefix | Notes |
|---|---|---|
| MongoAuthUser | `/admin/MongoAuthUser/` | Username, Password (hashed), IsSuperuser, IsActive, LastLogin, DateJoined |
| MongoAuthGroup | `/admin/MongoAuthGroup/` | Named groups for permission assignment |
| MongoAuthPermission | `/admin/MongoAuthPermission/` | Auto-generated per entity (add/change/delete/view) |
| MongoAuthGroupPermission | `/admin/MongoAuthGroupPermission/` | Links groups to permissions |
| MongoAuthUserGroup | `/admin/MongoAuthUserGroup/` | Links users to groups |

### Seeded Data

The `DataSeeder` populates user collections on first startup:

| Collection | Count | Examples |
|---|---|---|
| categories | 3 | Italian, Japanese, Mexican |
| restaurants | 2 | Bella Napoli, Tokyo Garden |
| restaurantProfiles | 2 | One per restaurant |
| ingredients | 5 | Mozzarella, Tomato Sauce, Fresh Salmon, Soy Sauce, Basil |
| menuItems | 4 | Margherita Pizza, Spaghetti Carbonara, Salmon Sashimi, Miso Ramen |
| gifts | 2 | Gourmet Chocolate Box, Ceramic Tea Set |

Auth collections are auto-populated by the bootstrap service:
- 1 admin superuser (`admin`/`admin`)
- Permissions: 4 per entity (add/change/delete/view) × 11 entities = 44 permissions

## URL Patterns

| Action | URL | Method | User Collections | Auth Collections |
|---|---|---|---|---|
| Dashboard home | `/admin/` | GET | Yes | Yes |
| List | `/admin/{Model}/` | GET | Yes | Yes |
| Detail (view/change) | `/admin/{Model}/{id}/change/` | GET | Read-only | Editable |
| Add form | `/admin/{Model}/add/` | GET | No | Yes |
| Create | `/admin/{Model}/add/` | POST | No | Yes |
| Update | `/admin/{Model}/{id}/change/` | POST | No | Yes |
| Delete form | `/admin/{Model}/{id}/delete/` | GET | No | Yes |
| Delete | `/admin/{Model}/{id}/delete/` | POST | No | Yes |

## ObjectId Primary Keys

MongoDB documents use `ObjectId` as the primary key (the `_id` field). In the dashboard:

- List views show `Id` as a 24-character hex string (e.g., `683f1a2b4c5d6e7f8a9b0c1d`)
- The `Id` column links to the detail view: `/admin/{Model}/{objectId}/change/`
- `RestaurantId` on RestaurantProfile and MenuItem shows as a plain ObjectId string (no FK lookup popup)

## Verification Checklist

### Phase 1: Authentication

1. **Login redirect** — Navigate to `http://localhost:8001/admin/`. Verify redirect to `/admin/login/?next=%2Fadmin%2F`
2. **Login page** — Verify login form has Username, Password fields and "Log in" button
3. **Invalid login** — Submit with `admin` / `wrong`. Verify stays on login page with "Please enter the correct username and password" error
4. **Valid login** — Submit with `admin` / `admin`. Verify redirect to `/admin/` dashboard
5. **Header** — Verify header shows "Welcome, admin" and "Log out" link

### Phase 2: Dashboard Home & Sidebar

6. **Dashboard home** — `/admin/` loads after login. Verify:
   - Page title contains "Sample Admin (MongoDB)"
   - Three entity group sections visible: "Restaurant", "Shop", and "Authentication and Authorization"
7. **Restaurant group** — Verify 5 entities listed: Categories, Restaurants, Restaurant Profiles, Menu Items, Ingredients
8. **Shop group** — Verify 1 entity listed: Gifts
9. **Auth group** — Verify 5 auth entities listed: Mongo Auth Users, Mongo Auth Groups, Mongo Auth Permissions, Mongo Auth Group Permissions, Mongo Auth User Groups
10. **Sidebar** — Navigate to any entity list. Verify the left sidebar has:
    - A "Filter models..." text input
    - "Restaurant" heading with 5 entity links
    - "Shop" heading with 1 entity link
    - "Authentication and Authorization" heading with 5 auth entity links
11. **Sidebar filter** — Type "cat" in the filter input. Verify only "Categories" remains visible

### Phase 3: User Collection List Views

#### Category List

12. **Category list loads** — Navigate to `/admin/Category/`. Verify:
    - Record count: "3 categories"
    - Table shows 3 rows: Italian, Japanese, Mexican
13. **Category columns** — Verify visible columns include: CreatedAt, Description, Name, UpdatedAt
14. **Row links** — Each row has a clickable link pointing to `/admin/Category/{objectId}/change/`

#### Restaurant List

15. **Restaurant list loads** — Navigate to `/admin/Restaurant/`. Verify:
    - Record count: "2 restaurants"
    - Table shows: Bella Napoli, Tokyo Garden

#### Ingredient List

16. **Ingredient list** — Navigate to `/admin/Ingredient/`. Verify:
    - Record count: "5 ingredients"
17. **Boolean column** — Verify `IsAllergen` shows `True`/`False` correctly

#### RestaurantProfile List

18. **RestaurantProfile list** — Navigate to `/admin/RestaurantProfile/`. Verify:
    - Record count: "2 restaurant profiles"
19. **ObjectId reference** — Verify `RestaurantId` column shows ObjectId hex strings

#### MenuItem List

20. **MenuItem list** — Navigate to `/admin/MenuItem/`. Verify:
    - Record count: "4 menu items"
21. **Nested collection** — Verify `IngredientIds` column displays as a JSON array string

#### Gift List

22. **Gift list** — Navigate to `/admin/Gift/`. Verify:
    - Record count: "2 gifts"
23. **Diverse types** — Verify columns render: IsWrapped (bool), TrackingCode (Guid), Price (decimal), Barcode (long), Weight (double), Rating (float), QuantityInStock (short), MinAge (byte), ShippedAt (DateTimeOffset)

### Phase 4: User Collection Detail Views

24. **Category detail** — Click a category row link. Verify:
    - URL matches `/admin/Category/{objectId}/change/`
    - Page shows "Change category" heading
    - Fields displayed as **read-only** values (class `readonly-value`) with actual data (not empty)
    - Fields include: Name, Description, CreatedAt, UpdatedAt
    - No "Save" buttons, no "Delete" link
25. **Restaurant detail** — Click a restaurant row. Verify read-only fields with actual values: Name, Address, Phone
26. **MenuItem detail** — Click a menu item row. Verify:
    - `RestaurantId` shows as ObjectId hex string
    - `Price` shows decimal value
    - `IsAvailable` shows True/False
27. **Gift detail** — Click a gift row. Verify diverse types display: TrackingCode (Guid), ShippedAt (DateTimeOffset), numeric types

### Phase 5: Conditional Search

28. **Search box visible (Category)** — Navigate to `/admin/Category/`. Verify search box present
29. **Search box visible (Restaurant)** — Navigate to `/admin/Restaurant/`. Verify search box present
30. **Search box hidden (Ingredient)** — Navigate to `/admin/Ingredient/`. Verify **no** search box
31. **Search box hidden (MenuItem)** — Navigate to `/admin/MenuItem/`. Verify no search box
32. **Search box hidden (Gift)** — Navigate to `/admin/Gift/`. Verify no search box
33. **Search filters correctly** — Navigate to `/admin/Category/?q=Italian`. Verify only "Italian" row appears, count "1 category"
34. **Search on Restaurant** — Navigate to `/admin/Restaurant/?q=Bella`. Verify only "Bella Napoli", count "1 restaurant"
35. **Search no match** — Navigate to `/admin/Category/?q=nonexistent`. Verify "0 categories"
36. **Search ignored on non-searchable** — Navigate to `/admin/Ingredient/?q=something`. Verify all 5 ingredients still display

### Phase 6: Sorting

37. **Sort ascending** — Navigate to `/admin/Category/?sort=Name&dir=asc`. Verify order: Italian, Japanese, Mexican
38. **Sort descending** — Navigate to `/admin/Category/?sort=Name&dir=desc`. Verify order: Mexican, Japanese, Italian
39. **Sort column headers** — Verify column headers are clickable sort links

### Phase 7: Read-Only Enforcement (User Collections)

40. **No Add button on list** — On `/admin/Category/`, verify no "Add category" link in toolbar
41. **No action bar** — On user entity lists, verify no action dropdown, no checkboxes
42. **No Save buttons on detail** — On user entity detail views, verify no Save buttons
43. **No Delete link on detail** — On user entity detail views, verify no Delete link

### Phase 8: Auth Entity CRUD

44. **MongoAuthPermission list** — Navigate to `/admin/MongoAuthPermission/`. Verify auto-generated permissions exist (e.g., `add_category`, `view_category`). Should have 44 permissions (4 per entity × 11 entities), paginated at 25 per page.
45. **MongoAuthGroup create** — Navigate to `/admin/MongoAuthGroup/add/`. Fill Name = "viewers". Click Save. Verify redirect to list, "viewers" appears.
46. **MongoAuthGroupPermission create** — Navigate to `/admin/MongoAuthGroupPermission/add/`. Assign `view_category` permission to the "viewers" group by entering the GroupId and PermissionId. Click Save.
47. **MongoAuthUser create** — Navigate to `/admin/MongoAuthUser/add/`. Fill Username = "testuser", Password = "test123". Click Save. Verify redirect to list, "testuser" appears with IsActive = True.
48. **MongoAuthUserGroup create** — Navigate to `/admin/MongoAuthUserGroup/add/`. Assign "testuser" to "viewers" group by entering UserId and GroupId. Click Save.
49. **MongoAuthGroup edit** — Navigate to the "viewers" group edit form. Change Name to "category_viewers". Click Save. Verify updated name in list.
50. **MongoAuthGroup delete** — Create a temporary group "temp_group". Navigate to its edit form. Click Delete. Confirm deletion. Verify "temp_group" is gone from list.

### Phase 9: Permission Enforcement

51. **Logout admin** — Click "Log out". Verify redirect to login page.
52. **Login as testuser** — Login with `testuser` / `test123`. Verify redirect to dashboard.
53. **Allowed: view Category list** — Navigate to `/admin/Category/`. Verify 200 OK with category data (testuser has `view_category` permission).
54. **Denied: view Restaurant list** — Navigate to `/admin/Restaurant/`. Verify 403 "Permission denied" (testuser has no `view_restaurant` permission).
55. **Denied: add Category** — Navigate to `/admin/Category/add/`. Verify 403 (testuser has no `add_category` permission).
56. **Login back as admin** — Logout testuser, login as `admin` / `admin`. Verify full access restored.

### Phase 10: Pagination

57. **Small collections** — Navigate to `/admin/Ingredient/` (5 items). Verify no pagination controls.
58. **Pagination on permissions** — Navigate to `/admin/MongoAuthPermission/`. With 44 permissions and 25 per page, verify pagination shows page 1 of 2. Navigate to page 2 and verify remaining permissions.
59. **Record counts accurate** — Verify counts: Categories: 3, Restaurants: 2, Ingredients: 5, MenuItems: 4, Gifts: 2

### Phase 11: Breadcrumbs and Navigation

60. **List breadcrumb** — On `/admin/Category/`, verify breadcrumb: Home > Categories
61. **Detail breadcrumb** — On `/admin/Category/{id}/change/`, verify breadcrumb: Home > Category > {objectId}
62. **Home link** — Click "Home" in breadcrumb. Verify redirect to `/admin/`

### Phase 12: Logout

63. **Logout** — Click "Log out". Verify redirect to login page.
64. **Session cleared** — Navigate to `/admin/`. Verify redirect to login (cookie cleared).

## What's NOT Tested (Current Limitations)

- **User collection CRUD** — create/edit/delete for user-defined collections (Category, Restaurant, etc.) is not yet implemented
- **FK lookup popups** — ObjectId references show as plain text, no popup navigation
- **Nested document expand/collapse UI** — nested objects rendered as flat JSON strings
- **Dashboard home "Add" links for user collections** — "Add" links appear even though user collections are read-only (Bug 3 in ISSUES-TO-BE-FIXED.md)
- **SAML SSO** — not configured in the MongoDB sample project
