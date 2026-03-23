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
- **User collections:** Full CRUD — list views, detail views, create/edit/delete forms, search, sort, and pagination
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
| User collections | Full CRUD | Full CRUD |
| Auth collections | Full CRUD | Full CRUD |
| FK relationships | Lookup popups | Plain ObjectId text (no lookups) |
| Nested collections | Not applicable | Shown as read-only JSON string (e.g., `IngredientIds`) |
| Auto-timestamps | `HasDefaultValueSql()` / `ValueGeneratedOnAdd()` | Convention-based (`CreatedAt`, `UpdatedAt` names) |
| Date/time types | Gift has DateOnly, TimeOnly, TimeSpan | Gift does NOT have DateOnly, TimeOnly, TimeSpan |
| Auth entity defaults | `IsActive`/`IsSuperuser` have DB defaults (readonly) | All non-PK auth fields are editable on forms (no DB defaults) |

## Dashboard Structure

The admin home at `/admin/` shows models organized by `EntityGroups`, plus an auto-generated "Authentication and Authorization" section for auth entities.

### Restaurant Group

| Model | URL prefix | Editable Fields | Auto-Timestamp / Read-Only Fields | `IAdminSettings` |
|---|---|---|---|---|
| Category | `/admin/Category/` | Name, Description | Id (PK), CreatedAt, UpdatedAt | `SearchFields`: Name, Description |
| Restaurant | `/admin/Restaurant/` | Name, Address, Phone | Id (PK), CreatedAt, UpdatedAt | `SearchFields`: Name |
| RestaurantProfile | `/admin/RestaurantProfile/` | RestaurantId (ObjectId text), Website, OpeningHours, Capacity | Id (PK), CreatedAt, UpdatedAt | None |
| MenuItem | `/admin/MenuItem/` | RestaurantId (ObjectId text), Name, Description, Price, IsAvailable | Id (PK), CreatedAt, UpdatedAt; IngredientIds (read-only JSON) | None |
| Ingredient | `/admin/Ingredient/` | Name, IsAllergen | Id (PK), CreatedAt, UpdatedAt | None |

### Shop Group

| Model | URL prefix | Editable Fields | Auto-Timestamp / Read-Only Fields | `IAdminSettings` |
|---|---|---|---|---|
| Gift | `/admin/Gift/` | Name, IsWrapped, TrackingCode (Guid), Price, Barcode, Weight, Rating, QuantityInStock, MinAge, ShippedAt (DateTimeOffset), Description, Notes | Id (PK), CreatedAt, UpdatedAt | None |

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

| Action | URL | Method |
|---|---|---|
| Dashboard home | `/admin/` | GET |
| List | `/admin/{Model}/` | GET |
| Add form | `/admin/{Model}/add/` | GET |
| Create | `/admin/{Model}/add/` | POST |
| Edit form | `/admin/{Model}/{id}/change/` | GET |
| Update | `/admin/{Model}/{id}/change/` | POST |
| Delete form | `/admin/{Model}/{id}/delete/` | GET |
| Delete | `/admin/{Model}/{id}/delete/` | POST |
| Execute bulk action | `/admin/{Model}/action/` | POST |
| Bulk delete confirmation | `/admin/{Model}/action/delete/` | GET |
| Bulk delete execute | `/admin/{Model}/action/delete/` | POST |

## ObjectId Primary Keys

MongoDB documents use `ObjectId` as the primary key (the `_id` field). In the dashboard:

- List views show `Id` as a 24-character hex string (e.g., `683f1a2b4c5d6e7f8a9b0c1d`)
- The `Id` column links to the detail view: `/admin/{Model}/{objectId}/change/`
- `RestaurantId` on RestaurantProfile and MenuItem shows as a plain ObjectId string (no FK lookup popup)

## Save Actions

Every create/edit form has three save buttons:

| Button | `_save_action` value | Redirect after save |
|---|---|---|
| Save | `save` | `/admin/{Model}/` (list) |
| Save and add another | `add_another` | `/admin/{Model}/add/` (new form) |
| Save and continue editing | `continue` | `/admin/{Model}/{id}/change/` (same record) |

## Auto-Timestamp Convention

Properties named `CreatedAt`, `UpdatedAt`, `CreatedDate`, `UpdatedDate`, `CreationDate`, or `ModificationDate` (of type `DateTime` or `DateTimeOffset`) are automatically treated as system-managed:
- Hidden from create forms (`ShowOnCreate = false`)
- Shown as read-only on edit forms (`ShowOnEdit = true`, `IsEditable = false`)
- Set automatically by the manager (`DateTime.UtcNow` on create/update)

All sample models inherit from `StandardDocument` which has `CreatedAt` and `UpdatedAt`.

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

### Phase 3: User Entity CRUD

#### Category CRUD (standalone, no FK)

12. **Category add form** — Navigate to `/admin/Category/add/`. Verify:
    - Form has editable fields: Name, Description
    - No `Id`, `CreatedAt`, or `UpdatedAt` fields on the create form (auto-generated, hidden)
    - Three save buttons: "Save", "Save and add another", "Save and continue editing"
13. **Category create** — Fill Name = "Thai", Description = "Thai cuisine". Click "Save". Verify redirect to `/admin/Category/` list. Verify "Thai" appears in the list. Record count shows "4 categories".
14. **Category edit form** — Click the "Thai" row link. Verify:
    - URL matches `/admin/Category/{objectId}/change/`
    - Page shows "Change category" heading
    - Name and Description are editable text inputs pre-filled with "Thai" and "Thai cuisine"
    - `Id`, `CreatedAt`, `UpdatedAt` shown as **read-only** text (class `readonly-value`), not inputs
    - `CreatedAt` and `UpdatedAt` have non-empty date/time values (not `0001-01-01`)
    - Three save buttons and a "Delete" link are present
15. **Category update** — Change Name to "Thai Food", click "Save". Verify redirect to list, "Thai Food" appears instead of "Thai".
16. **Category save and continue** — Click "Thai Food" row. Change Description to "Updated description". Click "Save and continue editing". Verify stays on the same edit form (URL unchanged), Description now shows "Updated description".
17. **Category save and add another** — On the edit form, click "Save and add another". Verify redirect to `/admin/Category/add/` (blank form).
18. **Category delete** — Navigate to the "Thai Food" edit form. Click "Delete" link. Verify:
    - Redirect to `/admin/Category/{objectId}/delete/`
    - Confirmation page shows "Are you sure?" and the category details
    - "Yes, I'm sure" button and "No, take me back" link
19. **Category confirm delete** — Click "Yes, I'm sure". Verify redirect to `/admin/Category/`. Verify "Thai Food" is gone. Record count shows "3 categories".
20. **Category cancel delete** — Create "Temp Category". Navigate to its delete page. Click "No, take me back". Verify redirect to list, "Temp Category" still exists. Delete it afterward to clean up.

#### Ingredient CRUD (boolean field)

21. **Ingredient create (checkbox unchecked)** — Navigate to `/admin/Ingredient/add/`. Fill Name = "Garlic". Leave IsAllergen unchecked. Click "Save". Verify redirect to list, "Garlic" appears with IsAllergen = `False`.
22. **Ingredient create (checkbox checked)** — Navigate to `/admin/Ingredient/add/`. Fill Name = "Peanut". Check IsAllergen. Click "Save". Verify "Peanut" appears with IsAllergen = `True`.
23. **Ingredient edit (toggle checkbox)** — Click "Peanut" row. Verify IsAllergen checkbox is checked. Uncheck it. Click "Save". Verify list shows IsAllergen = `False` for "Peanut".
24. **Ingredient delete** — Delete "Garlic" and "Peanut" to clean up. Verify count returns to "5 ingredients".

#### Restaurant CRUD (standalone)

25. **Restaurant create** — Navigate to `/admin/Restaurant/add/`. Fill Name = "Test Bistro", Address = "123 Test St", Phone = "555-0100". Click "Save". Verify "Test Bistro" appears in list. Count: "3 restaurants".
26. **Restaurant edit** — Click "Test Bistro". Change Address to "456 New Ave". Click "Save". Verify updated address in the edit form (click row again to verify).

#### RestaurantProfile CRUD (ObjectId reference)

27. **RestaurantProfile create** — Navigate to `/admin/RestaurantProfile/add/`. Verify `RestaurantId` renders as a plain text input (not a lookup popup — MongoDB has no FK lookups). Copy an existing Restaurant's ObjectId from the Restaurant list, paste into the RestaurantId field. Fill Website = "https://test.com", OpeningHours = "9-5", Capacity = 50. Click "Save". Verify redirect to list, count: "3 restaurant profiles".
28. **RestaurantProfile edit** — Click the new profile row. Verify RestaurantId shows the ObjectId hex string. Change Capacity to 75. Click "Save". Verify change persisted.

#### MenuItem CRUD (ObjectId reference + decimal + boolean + read-only collection field)

29. **MenuItem create** — Navigate to `/admin/MenuItem/add/`. Verify:
    - `RestaurantId` is a plain text input
    - `IngredientIds` is NOT shown on the create form (collection type, hidden from create)
    - `Price` is a number input
    - `IsAvailable` is a checkbox
30. **MenuItem create submit** — Paste a Restaurant ObjectId into RestaurantId. Fill Name = "Pad Thai", Description = "Classic noodles", Price = 12.99. Leave IsAvailable checked. Click "Save". Verify redirect to list, "Pad Thai" appears. Count: "5 menu items".
31. **MenuItem edit** — Click "Pad Thai" row. Verify:
    - All fields pre-filled correctly
    - `IngredientIds` shown as read-only JSON on the edit form (e.g., `[]`)
    - `Price` shows 12.99
    - `IsAvailable` checkbox is checked
32. **MenuItem update** — Change Price to 14.50, uncheck IsAvailable. Click "Save". Verify changes persisted.

#### Gift CRUD (diverse types: Guid, DateTimeOffset, decimal, long, double, float, short, byte)

33. **Gift create** — Navigate to `/admin/Gift/add/`. Fill:
    - Name = "Test Gift"
    - IsWrapped = checked
    - TrackingCode = a valid GUID (e.g., `a1b2c3d4-e5f6-7890-abcd-ef1234567890`)
    - Price = 29.99
    - Barcode = 1234567890
    - Weight = 1.5
    - Rating = 4.5
    - QuantityInStock = 100
    - MinAge = 12
    - ShippedAt = `2025-06-15T10:30:00+00:00`
    - Description = "A test gift"
    - Notes = "Handle with care"
    - Click "Save". Verify redirect to list, "Test Gift" appears. Count: "3 gifts".
34. **Gift edit** — Click "Test Gift" row. Verify:
    - All fields pre-filled with correct values
    - TrackingCode shows the GUID
    - ShippedAt shows the DateTimeOffset value
    - Numeric fields show correct values
35. **Gift update** — Change Price to 39.99, Rating to 3.5. Click "Save". Verify changes persisted.

#### Cleanup

36. **Delete test records** — Delete "Test Bistro" restaurant (and its profile), "Pad Thai" menu item, "Test Gift", and "Temp Category" (if still exists). Verify counts return to seeded values: Categories 3, Restaurants 2, RestaurantProfiles 2, Ingredients 5, MenuItems 4, Gifts 2.

### Phase 4: Conditional Search

37. **Search box visible (Category)** — Navigate to `/admin/Category/`. Verify search box present
38. **Search box visible (Restaurant)** — Navigate to `/admin/Restaurant/`. Verify search box present
39. **Search box hidden (Ingredient)** — Navigate to `/admin/Ingredient/`. Verify **no** search box
40. **Search box hidden (MenuItem)** — Navigate to `/admin/MenuItem/`. Verify no search box
41. **Search box hidden (RestaurantProfile)** — Navigate to `/admin/RestaurantProfile/`. Verify no search box
42. **Search box hidden (Gift)** — Navigate to `/admin/Gift/`. Verify no search box
43. **Search filters correctly** — Navigate to `/admin/Category/?q=Italian`. Verify only "Italian" row appears, count "1 category"
44. **Search on Restaurant** — Navigate to `/admin/Restaurant/?q=Bella`. Verify only "Bella Napoli", count "1 restaurant"
45. **Search no match** — Navigate to `/admin/Category/?q=nonexistent`. Verify "0 categories"
46. **Search ignored on non-searchable** — Navigate to `/admin/Ingredient/?q=something`. Verify all 5 ingredients still display

### Phase 5: Sorting

47. **Sort ascending** — Navigate to `/admin/Category/?sort=Name&dir=asc`. Verify order: Italian, Japanese, Mexican
48. **Sort descending** — Navigate to `/admin/Category/?sort=Name&dir=desc`. Verify order: Mexican, Japanese, Italian
49. **Sort column headers** — Verify column headers are clickable sort links

### Phase 6: Bulk Actions

50. **Action bar renders** — Navigate to `/admin/Category/`. Verify:
    - An action dropdown (`<select name="action">`) with "Delete selected categories" option
    - A "Go" button
    - Each row has a checkbox
    - A select-all checkbox in the header
    - A counter text showing "0 of 3 selected"
51. **Bulk delete flow** — Create 2 temporary categories ("BulkDel1", "BulkDel2"). Select both checkboxes. Select "Delete selected categories" from dropdown. Click "Go". Verify redirect to bulk delete confirmation page showing "2 categories" with "Yes, I'm sure" and "No, take me back".
52. **Confirm bulk delete** — Click "Yes, I'm sure". Verify redirect to `/admin/Category/` with a **green success banner** "Successfully deleted 2 categories." Verify both records gone, count back to "3 categories".
53. **Flash message is transient** — Navigate away from the list and come back. Verify the green success banner is gone (one-time query param, not stored in session).
54. **Cancel bulk delete** — Create "BulkCancel". Select it, choose delete action, click "Go" to reach confirmation. Click "No, take me back". Verify redirect to list, "BulkCancel" still exists. Delete it to clean up.
55. **Select-all and counter** — On `/admin/Category/`, click the select-all checkbox. Verify all row checkboxes become checked and the counter shows "3 of 3 selected". Uncheck select-all, verify counter resets to "0 of 3 selected".
56. **Individual checkbox updates counter** — Check 2 of 3 rows individually. Verify counter shows "2 of 3 selected". Only checked rows are highlighted. Header checkbox remains unchecked.
57. **Uncheck one row unchecks header** — Click select-all (all checked). Uncheck a single row. Verify header checkbox becomes unchecked, counter shows "2 of 3 selected".
58. **Empty selection guard** — Do NOT check any rows. Select "Delete selected categories" from dropdown. Click "Go". Verify nothing happens (JavaScript prevents submission with empty selection).
59. **No action selected guard** — Check some rows but leave the dropdown on "---". Click "Go". Verify nothing happens (JavaScript prevents submission without action selected).

### Phase 7: Auth Entity CRUD

60. **MongoAuthPermission list** — Navigate to `/admin/MongoAuthPermission/`. Verify auto-generated permissions exist (e.g., `add_category`, `view_category`). Should have 44 permissions (4 per entity × 11 entities), paginated at 25 per page.
61. **MongoAuthGroup create** — Navigate to `/admin/MongoAuthGroup/add/`. Fill Name = "viewers". Click Save. Verify redirect to list, "viewers" appears.
62. **MongoAuthGroupPermission create** — Navigate to `/admin/MongoAuthGroupPermission/add/`. Assign `view_category` permission to the "viewers" group by entering the GroupId and PermissionId. Click Save.
63. **MongoAuthUser create** — Navigate to `/admin/MongoAuthUser/add/`. Note: unlike EF Core, ALL non-PK fields are editable (no DB defaults). Fill Username = "testuser", Password = "test123". **Check the `IsActive` checkbox** (browsers don't submit unchecked checkboxes, so leaving it unchecked sets `IsActive = false` and the user can't login). Leave IsSuperuser unchecked. Click Save. Verify redirect to list, "testuser" appears with IsActive = True.
64. **MongoAuthUserGroup create** — Navigate to `/admin/MongoAuthUserGroup/add/`. Assign "testuser" to "viewers" group by entering UserId and GroupId. Click Save.
65. **MongoAuthGroup edit** — Navigate to the "viewers" group edit form. Change Name to "category_viewers". Click Save. Verify updated name in list.
66. **MongoAuthGroup delete** — Create a temporary group "temp_group". Navigate to its edit form. Click Delete. Confirm deletion. Verify "temp_group" is gone from list.

### Phase 8: Permission Enforcement

67. **Logout admin** — Click "Log out". Verify redirect to login page.
68. **Login as testuser** — Login with `testuser` / `test123`. Verify redirect to dashboard.
69. **Allowed: view Category list** — Navigate to `/admin/Category/`. Verify 200 OK with category data (testuser has `view_category` permission).
70. **Denied: view Restaurant list** — Navigate to `/admin/Restaurant/`. Verify 403 "Permission denied" (testuser has no `view_restaurant` permission).
71. **Denied: add Category** — Navigate to `/admin/Category/add/`. Verify 403 (testuser has no `add_category` permission).
72. **Login back as admin** — Logout testuser, login as `admin` / `admin`. Verify full access restored.

### Phase 9: Pagination

73. **Small collections** — Navigate to `/admin/Ingredient/` (5 items). Verify no pagination controls.
74. **Pagination on permissions** — Navigate to `/admin/MongoAuthPermission/`. With 44 permissions and 25 per page, verify pagination shows page 1 of 2. Navigate to page 2 and verify remaining 19 permissions.
75. **Record counts accurate** — Verify counts: Categories: 3, Restaurants: 2, Ingredients: 5, MenuItems: 4, Gifts: 2

### Phase 10: Breadcrumbs and Navigation

76. **List breadcrumb** — On `/admin/Category/`, verify breadcrumb: Home > Categories
77. **Detail breadcrumb** — On `/admin/Category/{id}/change/`, verify breadcrumb: Home > Category > {objectId}
78. **Home link** — Click "Home" in breadcrumb. Verify redirect to `/admin/`

### Phase 11: Logout

79. **Logout** — Click "Log out". Verify redirect to login page.
80. **Session cleared** — Navigate to `/admin/`. Verify redirect to login (cookie cleared).

## What's NOT Tested (Current Limitations)

- **FK lookup popups** — ObjectId references show as plain text, no popup navigation
- **Nested document editing** — collection/complex type fields (e.g., `IngredientIds`) are read-only JSON, not editable
- **SAML SSO** — not configured in the MongoDB sample project
- **Custom bulk actions** — no sample entity defines `AdminActionList<ObjectId>` with custom actions
- **Per-collection read-only mode** — `readOnly: true` on `AddCollection<T>` is not demonstrated in the sample project (all user collections are editable)
