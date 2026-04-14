# 🚀 Enterprise Ecommerce Inventory System — MVP PRD
## Minimum Viable Product — Build → Deploy → Iterate

---

> **MVP Version:** 1.0.0  
> **Target:** Deploy kar, working system hath mein aaye, phir daily update  
> **Full PRD Version:** 1.0.0 (parent document)  
> **Stack:** ASP.NET Core 10 | PostgreSQL (Neon DB) | Clean Architecture  
> **Deploy Target:** https://www.monsterasp.net/  (free tier) following Visual Studio login through webdeploye publish progile download kaarke.

---

## 📌 MVP PHILOSOPHY

```
FULL PRD = 80 endpoints, 16 tables, 14 modules
MVP      = 52 endpoints, 14 tables, 10 modules
           ↑ Deploy this first. Works end-to-end.

Phase 2+ = Daily add karo:
           Notifications → Reports → Stock Transfer → Warehouse Zones
```

**Rule:** Agar feature bina kisi doosre feature ke kaam kar sakta hai — MVP mein hai.  
**Rule:** Agar feature sirf "nice to have" hai — Phase 2 mein.

---

## 📋 TABLE OF CONTENTS

1. [MVP vs Full PRD — What's Cut](#1-mvp-vs-full-prd--whats-cut)
2. [MVP Solution Structure](#2-mvp-solution-structure)
3. [MVP Database — 14 Tables](#3-mvp-database--14-tables)
4. [MVP Modules — 10 Modules](#4-mvp-modules--10-modules)
5. [Module 1 — Auth](#5-module-1--auth)
6. [Module 2 — Users](#6-module-2--users)
7. [Module 3 — Roles & Permissions (Seeded Only)](#7-module-3--roles--permissions-seeded-only)
8. [Module 4 — Categories](#8-module-4--categories)
9. [Module 5 — Products](#9-module-5--products)
10. [Module 6 — Warehouses](#10-module-6--warehouses)
11. [Module 7 — Stocks](#11-module-7--stocks)
12. [Module 8 — Suppliers](#12-module-8--suppliers)
13. [Module 9 — Purchase Orders](#13-module-9--purchase-orders)
14. [Module 10 — Sales Orders](#14-module-10--sales-orders)
15. [MVP API Endpoint Reference — 52 Endpoints](#15-mvp-api-endpoint-reference--52-endpoints)
16. [MVP Folder Structure](#16-mvp-folder-structure)
17. [MVP Database DDL](#17-mvp-database-ddl)
18. [Implementation Order (Day-by-Day Plan)](#18-implementation-order-day-by-day-plan)
19. [Deploy Checklist](#19-deploy-checklist)
20. [Post-MVP Daily Update Plan](#20-post-mvp-daily-update-plan)

---

## 1. MVP vs Full PRD — What's Cut

### ✅ MVP MEIN HAI (Build Now)

| Module | Reason |
|--------|--------|
| Auth (full) | Core — bina iske kuch nahi chalta |
| Users (basic CRUD + profile image) | Core |
| Roles (seeded, no dynamic CRUD) | 8 roles seed karo, UI baad mein |
| Categories (full) | Products ke liye zaroori |
| Products (full + images) | Core inventory |
| Warehouses (basic) | Stock ke liye zaroori |
| Stocks (view + manual adjust) | Core inventory feature |
| Suppliers (basic CRUD) | Purchase orders ke liye |
| Purchase Orders (full lifecycle) | Inbound stock management |
| Sales Orders (full lifecycle) | Outbound stock management |

### ❌ PHASE 2 MEIN HAI (Add Later)

| Feature | Why Cut from MVP | Add in Phase |
|---------|-----------------|--------------|
| In-App Notifications | Nice to have, email se kaam chalega | Phase 2 — Day 3-4 post deploy |
| Reports & Analytics | No data initially, add when data builds up | Phase 2 — Week 2 |
| Stock Transfer (warehouse-to-warehouse) | Add after basic stock works | Phase 2 — Day 5 |
| Warehouse Zones | Extra complexity, not blocking anything | Phase 3 |
| Customers Module | Sales orders mein walk-in customer kaam karega | Phase 2 — Day 2 |
| Dynamic Role CRUD | 8 roles kaafi hain, seeded via migration | Phase 2 — Week 2 |
| Supplier-Product linking | Basic supplier info kaafi | Phase 2 — Day 6 |
| Audit Logs | Add after core works | Phase 2 — Day 7 |
| Product Variants | Add when needed | Phase 3 |
| Advanced Reports/Export | Phase 3 | Phase 3 |

---

## 2. MVP Solution Structure

```
EcommerceInventorySystem/
├── EcommerceInventorySystem.sln
├── .gitignore
├── README.md
│
└── src/
    ├── EcommerceInventory.Domain/
    ├── EcommerceInventory.Application/
    ├── EcommerceInventory.Infrastructure/
    └── EcommerceInventory.API/
```

**Project References (same as full PRD):**
```
Domain        ← no references
Application   ← Domain
Infrastructure← Application + Domain
API           ← Application + Infrastructure
```

---

## 3. MVP Database — 14 Tables

### Tables in MVP

```
users                    ✅ MVP
roles                    ✅ MVP (seeded, no CRUD API)
permissions              ✅ MVP (seeded, no CRUD API)
role_permissions         ✅ MVP (seeded)
user_roles               ✅ MVP
refresh_tokens           ✅ MVP
password_reset_tokens    ✅ MVP
email_verification_tokens✅ MVP
categories               ✅ MVP
products                 ✅ MVP
product_images           ✅ MVP
warehouses               ✅ MVP
stocks                   ✅ MVP
stock_movements          ✅ MVP (write-only from domain, no separate API)
suppliers                ✅ MVP
purchase_orders          ✅ MVP
purchase_order_items     ✅ MVP
sales_orders             ✅ MVP
sales_order_items        ✅ MVP
```

### Tables CUT from MVP

```
warehouse_images         ❌ Phase 2
warehouse_zones          ❌ Phase 3
customers                ❌ Phase 2 (use walk-in/default customer for now)
supplier_products        ❌ Phase 2
notifications            ❌ Phase 2
audit_logs               ❌ Phase 2
```

---

## 4. MVP Modules — 10 Modules

```
┌─────────────────────────────────────────────────────────────┐
│                    MVP SYSTEM MAP                           │
│                                                             │
│  [Auth] ──────────────── Login, Register, Token Refresh     │
│     │                                                       │
│  [Users] ─────────────── CRUD + Profile Image               │
│     │                                                       │
│  [Roles/Permissions] ─── Seeded Only (8 roles, 40 perms)    │
│     │                                                       │
│  [Categories] ─────────── Tree structure, CRUD + image      │
│     │                                                       │
│  [Products] ───────────── CRUD + multi-image upload         │
│     │                                                       │
│  [Warehouses] ─────────── CRUD (no zones, no images MVP)    │
│     │                                                       │
│  [Stocks] ─────────────── View levels + manual adjust       │
│     │                                                       │
│  [Suppliers] ──────────── Basic CRUD                        │
│     │                                                       │
│  [Purchase Orders] ─────── Draft→Submit→Approve→Receive     │
│     │                  (Receive = auto stock increment)     │
│  [Sales Orders] ────────── Draft→Submit→Approve→Ship→Deliver│
│                         (Approve = reserve, Ship = deduct)  │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Module 1 — Auth

### Endpoints (9)

| # | Endpoint | Method | Auth | Description |
|---|----------|--------|------|-------------|
| 1 | `/auth/register` | POST | None | Register user |
| 2 | `/auth/login` | POST | None | Login → token pair |
| 3 | `/auth/refresh-token` | POST | None | Rotate tokens |
| 4 | `/auth/logout` | POST | Bearer | Revoke refresh |
| 5 | `/auth/verify-email` | POST | None | Verify email |
| 6 | `/auth/forgot-password` | POST | None | Send reset link |
| 7 | `/auth/reset-password` | POST | None | New password |
| 8 | `/auth/me` | GET | Bearer | Current user |
| 9 | `/auth/change-password` | POST | Bearer | Change password |

### Register — Request/Response

```json
// POST /auth/register
// Request
{
  "fullName": "Chandan Kumar",
  "email": "chandan@example.com",
  "password": "Test@1234",
  "phone": "9876543210"
}

// Response 201
{
  "success": true,
  "message": "Registration successful. Please check your email to verify your account.",
  "data": {
    "id": "uuid",
    "fullName": "Chandan Kumar",
    "email": "chandan@example.com",
    "status": "Active",
    "isEmailVerified": false,
    "createdAt": "2025-04-13T10:00:00Z"
  }
}
```

### Login — Request/Response

```json
// POST /auth/login
// Request
{
  "email": "chandan@example.com",
  "password": "Test@1234",
  "deviceInfo": "Chrome/Windows"
}

// Response 200
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiJ9...",
    "refreshToken": "abc123randombase64...",
    "expiresIn": 900,
    "tokenType": "Bearer",
    "user": {
      "id": "uuid",
      "fullName": "Chandan Kumar",
      "email": "chandan@example.com",
      "roles": ["InventoryManager"],
      "permissions": ["Products.View", "Products.Create", ...],
      "isEmailVerified": true
    }
  }
}
```

### Pseudo-Code: Login Handler

```
LoginCommandHandler.Handle():
────────────────────────────────────────────
1. Find user by email (include roles → rolePermissions → permission)
   IF not found → throw UnauthorizedException("Invalid credentials")
   
2. Check user.Status
   Inactive/Suspended → throw UnauthorizedException("Account disabled")
   IsDeleted          → throw UnauthorizedException("Invalid credentials")

3. BCrypt.Verify(password, user.PasswordHash)
   IF false → throw UnauthorizedException("Invalid credentials")

4. Extract flat lists
   roles       = user.UserRoles.Select(ur => ur.Role.Name)
   permissions = user.UserRoles
                     .SelectMany(ur => ur.Role.RolePermissions)
                     .Select(rp => rp.Permission.Name)
                     .Distinct()

5. tokenService.GenerateAccessToken(user, roles, permissions)
   → JWT with claims: userId, email, role(s), permission(s)
   → Expires: DateTime.UtcNow + 15 min

6. tokenService.GenerateRefreshToken(deviceInfo)
   → 64 random bytes → Base64 string
   → Expires: DateTime.UtcNow + 7 days
   → Save to DB: refresh_tokens table

7. user.RecordLogin() → updates last_login_at

8. SaveChangesAsync()

9. Return LoginResponseDto
```

### Pseudo-Code: Refresh Token Handler

```
RefreshTokenCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { accessToken (expired OK), refreshToken }

1. tokenService.GetPrincipalFromExpiredToken(accessToken)
   → Validates signature + issuer + audience, skips lifetime check
   IF null → throw UnauthorizedException("Invalid access token")

2. userId = principal.FindFirst(ClaimTypes.NameIdentifier)

3. storedToken = DB lookup WHERE token = refreshToken AND user_id = userId
   IF null → throw UnauthorizedException("Invalid refresh token")

4. IF storedToken.IsRevoked:
     ── REUSE ATTACK: revoke ALL tokens for this user
     await RevokeAllUserRefreshTokensAsync(userId)
     throw UnauthorizedException("Security alert: session terminated")

   IF storedToken.ExpiresAt < UtcNow:
     throw UnauthorizedException("Refresh token expired. Please login again.")

5. Revoke old token (rotation)
   storedToken.IsRevoked  = true
   storedToken.RevokedAt  = UtcNow
   storedToken.ReplacedBy = newToken.Token

6. Re-fetch user with fresh roles/permissions (role may have changed)

7. Generate new access + refresh token pair

8. Save new refresh token to DB

9. Return new { accessToken, refreshToken, expiresIn: 900 }
```

---

## 6. Module 2 — Users

### Endpoints (8)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 10 | `/users` | GET | Users.View | Paginated list |
| 11 | `/users/{id}` | GET | Users.View | By ID |
| 12 | `/users` | POST | Users.Create | Create user |
| 13 | `/users/{id}` | PUT | Users.Edit | Update profile |
| 14 | `/users/{id}` | DELETE | Users.Delete | Soft delete |
| 15 | `/users/{id}/activate` | PATCH | Users.Edit | Activate |
| 16 | `/users/{id}/deactivate` | PATCH | Users.Edit | Deactivate |
| 17 | `/users/{id}/profile-image` | POST | Users.Edit | Upload photo |
| 18 | `/users/{id}/assign-role` | POST | Users.AssignRole | Assign role |
| 19 | `/users/{id}/revoke-role/{roleId}` | DELETE | Users.AssignRole | Revoke role |

### Create User — Request/Response

```json
// POST /users
// Request
{
  "fullName": "Rahul Sharma",
  "email": "rahul@company.com",
  "password": "Secure@123",
  "phone": "9876543210",
  "roleId": "uuid-of-inventorymanager-role"
}

// Response 201
{
  "success": true,
  "data": {
    "id": "uuid",
    "fullName": "Rahul Sharma",
    "email": "rahul@company.com",
    "phone": "9876543210",
    "status": "Active",
    "isEmailVerified": false,
    "roles": ["InventoryManager"],
    "createdAt": "2025-04-13T10:00:00Z"
  }
}
```

### Pseudo-Code: Upload Profile Image

```
UploadProfileImageCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { userId, file: IFormFile }

1. Load user by userId
   IF null → throw NotFoundException("User not found")

2. Authorization check:
   IF currentUser.UserId != userId AND NOT currentUser.HasPermission("Users.Edit"):
     throw UnauthorizedException("Can only update your own profile image")

3. Validate file:
   IF file.Length == 0 → throw ValidationException("File is empty")
   IF file.ContentType NOT IN [image/jpeg, image/png, image/webp]:
     throw ValidationException("Only JPEG, PNG, WebP allowed")
   IF file.Length > 5MB → throw ValidationException("Max 5MB")

4. Upload to Cloudinary (folder: "profiles")
   result = await cloudinaryService.UploadImageAsync(file, "profiles")

5. Delete old image from Cloudinary (if exists)
   IF user.CloudinaryProfileId != null:
     await cloudinaryService.DeleteImageAsync(user.CloudinaryProfileId)

6. Update user entity
   user.SetProfileImage(result.SecureUrl, result.PublicId)

7. SaveChangesAsync()

8. Return { imageUrl: result.SecureUrl }
```

---

## 7. Module 3 — Roles & Permissions (Seeded Only)

### MVP Approach

> **MVP mein Roles ka dynamic CRUD API nahi banao.** 8 roles aur 40+ permissions EF Core seed data se insert karo. Phase 2 mein full Roles CRUD API add karna.

**Why?** Role management is rarely needed at launch. It adds complexity (circular permission assignment risks, hierarchy validation). Seed karo, move on.

### Seed via EF Core Migration

```csharp
// Infrastructure/Persistence/Seed/RolePermissionSeed.cs
public static class RolePermissionSeed
{
    public static void Seed(ModelBuilder builder)
    {
        // ── Roles
        var superAdminId       = Guid.Parse("11111111-0000-0000-0000-000000000001");
        var adminId            = Guid.Parse("11111111-0000-0000-0000-000000000002");
        var inventoryManagerId = Guid.Parse("11111111-0000-0000-0000-000000000003");
        var warehouseManagerId = Guid.Parse("11111111-0000-0000-0000-000000000004");
        var purchaseManagerId  = Guid.Parse("11111111-0000-0000-0000-000000000005");
        var salesManagerId     = Guid.Parse("11111111-0000-0000-0000-000000000006");
        var accountantId       = Guid.Parse("11111111-0000-0000-0000-000000000007");
        var viewerId           = Guid.Parse("11111111-0000-0000-0000-000000000008");

        builder.Entity<Role>().HasData(
            new Role { Id = superAdminId,       Name = "SuperAdmin",        HierarchyLevel = 1,  IsSystemRole = true },
            new Role { Id = adminId,            Name = "Admin",             HierarchyLevel = 2,  IsSystemRole = true },
            new Role { Id = inventoryManagerId, Name = "InventoryManager",  HierarchyLevel = 3,  IsSystemRole = false },
            new Role { Id = warehouseManagerId, Name = "WarehouseManager",  HierarchyLevel = 4,  IsSystemRole = false },
            new Role { Id = purchaseManagerId,  Name = "PurchaseManager",   HierarchyLevel = 5,  IsSystemRole = false },
            new Role { Id = salesManagerId,     Name = "SalesManager",      HierarchyLevel = 6,  IsSystemRole = false },
            new Role { Id = accountantId,       Name = "Accountant",        HierarchyLevel = 7,  IsSystemRole = false },
            new Role { Id = viewerId,           Name = "Viewer",            HierarchyLevel = 10, IsSystemRole = false }
        );

        // ── Permissions (40+ entries — same as full PRD seed data)
        // define all Guid IDs, then HasData(permissions array)
        // then assign to roles via RolePermission HasData()
        // (See full seed code below)
    }
}
```

### One Read-Only Endpoint for MVP

```
GET /roles         → Returns seeded roles list (for dropdown in frontend)
GET /permissions   → Returns all permissions (for display only)
```

---

## 8. Module 4 — Categories

### Endpoints (6)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 20 | `/categories` | GET | Categories.View | All (tree) |
| 21 | `/categories/{id}` | GET | Categories.View | By ID |
| 22 | `/categories` | POST | Categories.Create | Create |
| 23 | `/categories/{id}` | PUT | Categories.Edit | Update |
| 24 | `/categories/{id}` | DELETE | Categories.Delete | Soft delete |
| 25 | `/categories/{id}/image` | POST | Categories.Edit | Upload image |

### Create Category — Request/Response

```json
// POST /categories
// Request
{
  "name": "Electronics",
  "description": "All electronic products",
  "parentId": null,
  "imageFile": "[multipart/form-data]"
}

// Response 201
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Electronics",
    "slug": "electronics-a1b2c3d4",
    "description": "All electronic products",
    "imageUrl": "https://res.cloudinary.com/...",
    "parentId": null,
    "isActive": true,
    "children": []
  }
}
```

### Pseudo-Code: Create Category

```
CreateCategoryCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { name, description?, parentId?, imageFile? }

1. VALIDATION (FluentValidation):
   - name: NotEmpty, MaxLength(150)
   - parentId: IF provided, must exist in DB

2. IF parentId != null:
     parent = await uow.Categories.GetByIdAsync(parentId)
     IF null → throw NotFoundException("Parent category not found")

3. Slug generation:
   slug = name.Trim().ToLower().Replace(" ", "-") + "-" + NewGuid()[..8]
   ── Check uniqueness
   IF await uow.Categories.Query().AnyAsync(c => c.Slug == slug):
     slug += "-" + NewGuid()[..4]  // retry with extra suffix

4. Cloudinary upload (if imageFile provided):
   result = await cloudinaryService.UploadImageAsync(imageFile, "categories")
   imageUrl    = result.SecureUrl
   cloudinaryId = result.PublicId

5. Create entity:
   category = new Category
   {
     Name         = name.Trim(),
     Slug         = slug,
     Description  = description,
     ParentId     = parentId,
     ImageUrl     = imageUrl,
     CloudinaryId = cloudinaryId,
     IsActive     = true
   }

6. await uow.Categories.AddAsync(category)
   await uow.SaveChangesAsync()

7. Return CategoryDto
```

### Pseudo-Code: Get Categories as Tree

```
GetAllCategoriesQueryHandler.Handle():
────────────────────────────────────────────
1. Load ALL categories (flat list, no filter)
   categories = await uow.Categories.Query()
                    .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                    .ToListAsync()

2. Build tree in memory (O(n), no recursive SQL):
   lookup = categories.ToDictionary(c => c.Id, c => mapper.Map<CategoryDto>(c))
   roots  = new List<CategoryDto>()

   foreach category in categories:
     dto = lookup[category.Id]
     IF category.ParentId == null:
       roots.Add(dto)
     ELSE IF lookup.ContainsKey(category.ParentId):
       lookup[category.ParentId].Children.Add(dto)

3. Return roots  // hierarchical tree
```

---

## 9. Module 5 — Products

### Endpoints (8)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 26 | `/products` | GET | Products.View | Paginated list |
| 27 | `/products/{id}` | GET | Products.View | By ID (with images + stock) |
| 28 | `/products/sku/{sku}` | GET | Products.View | By SKU |
| 29 | `/products` | POST | Products.Create | Create + images |
| 30 | `/products/{id}` | PUT | Products.Edit | Update |
| 31 | `/products/{id}` | DELETE | Products.Delete | Soft delete |
| 32 | `/products/{id}/images` | POST | Products.Edit | Upload images |
| 33 | `/products/{id}/images/{imageId}` | DELETE | Products.Edit | Delete image |

### Create Product — Request

```
POST /products
Content-Type: multipart/form-data

Fields:
  categoryId:    uuid
  name:          string (required)
  sku:           string (required, unique)
  description:   string (optional)
  unitPrice:     decimal (required, >= 0)
  costPrice:     decimal (required, >= 0)
  reorderLevel:  int (default: 0)
  reorderQty:    int (default: 0)
  barcode:       string (optional)
  weightKg:      decimal (optional)
  images:        List<IFormFile> (optional, max 10, max 5MB each)
```

### Pseudo-Code: Create Product

```
CreateProductCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { categoryId, name, sku, description?, unitPrice,
         costPrice, reorderLevel, reorderQty, barcode?, weightKg?,
         images?: List<IFormFile> }

1. VALIDATION:
   - name:       NotEmpty, MaxLength(200)
   - sku:        NotEmpty, MaxLength(100)
   - unitPrice:  GreaterThanOrEqual(0)
   - costPrice:  GreaterThanOrEqual(0)
   - images:     each Max(5MB), image types only

2. SKU uniqueness check:
   IF await uow.Products.Query().AnyAsync(p => p.Sku == sku.ToUpper()):
     throw DomainException("SKU already exists")

3. Category exists:
   category = await uow.Categories.GetByIdAsync(categoryId)
   IF null → throw NotFoundException("Category not found")

4. Create Product entity (via factory):
   product = Product.Create(categoryId, name, sku, unitPrice,
               costPrice, description, reorderLevel, reorderQty,
               createdBy: currentUserId)

5. Upload images to Cloudinary in PARALLEL:
   IF images != null AND images.Any():
     tasks = images.Select(img => cloudinaryService.UploadImageAsync(img, "products"))
     results = await Task.WhenAll(tasks)

     FOR i = 0 to results.Length - 1:
       productImage = new ProductImage
       {
         ProductId    = product.Id,
         CloudinaryId = results[i].PublicId,
         ImageUrl     = results[i].SecureUrl,
         IsPrimary    = (i == 0),    // first = primary
         DisplayOrder = i
       }
       product.Images.Add(productImage)

6. await uow.Products.AddAsync(product)
   await uow.SaveChangesAsync()

7. Return ProductDto (with images list)
```

### Pseudo-Code: Get Products (Paginated + Filtered)

```
GetAllProductsQueryHandler.Handle():
────────────────────────────────────────────
INPUT: { pageNumber=1, pageSize=20, searchTerm?,
         categoryId?, status?, minPrice?, maxPrice?,
         sortBy="Name", sortDesc=false }

1. Base query:
   query = uow.Products.Query()
              .Include(p => p.Category)
              .Include(p => p.Images.Where(i => i.IsPrimary))

2. Filters:
   IF searchTerm != null:
     query = query.Where(p =>
       EF.Functions.ILike(p.Name, $"%{searchTerm}%") ||
       EF.Functions.ILike(p.Sku, $"%{searchTerm}%"))

   IF categoryId != null:
     query = query.Where(p => p.CategoryId == categoryId)

   IF status != null:
     query = query.Where(p => p.Status == status)

   IF minPrice != null:
     query = query.Where(p => p.UnitPrice >= minPrice)

   IF maxPrice != null:
     query = query.Where(p => p.UnitPrice <= maxPrice)

3. Sort:
   query = (sortBy, sortDesc) switch
   {
     ("Price",     false) => query.OrderBy(p => p.UnitPrice),
     ("Price",     true)  => query.OrderByDescending(p => p.UnitPrice),
     ("CreatedAt", false) => query.OrderBy(p => p.CreatedAt),
     ("CreatedAt", true)  => query.OrderByDescending(p => p.CreatedAt),
     ("Sku",       _)     => query.OrderBy(p => p.Sku),
     _                    => sortDesc
                              ? query.OrderByDescending(p => p.Name)
                              : query.OrderBy(p => p.Name)
   }

4. Count + paginate:
   total = await query.CountAsync()
   items = await query.Skip((pageNumber-1)*pageSize).Take(pageSize).ToListAsync()

5. Map → return PagedResult<ProductDto>
```

---

## 10. Module 6 — Warehouses

### Endpoints (5)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 34 | `/warehouses` | GET | Warehouses.View | All warehouses |
| 35 | `/warehouses/{id}` | GET | Warehouses.View | By ID |
| 36 | `/warehouses` | POST | Warehouses.Create | Create |
| 37 | `/warehouses/{id}` | PUT | Warehouses.Edit | Update |
| 38 | `/warehouses/{id}` | DELETE | Warehouses.Delete | Soft delete |

> **Phase 2:** `/warehouses/{id}/images`, `/warehouses/{id}/zones`, `/warehouses/{id}/stock-summary`

### Create Warehouse — Request/Response

```json
// POST /warehouses
// Request
{
  "name": "Main Warehouse",
  "code": "WH-01",
  "managerId": "uuid (optional)",
  "phone": "9876543210",
  "address": {
    "street": "12 Industrial Area",
    "city": "Kolkata",
    "state": "West Bengal",
    "pincode": "700001",
    "country": "India"
  }
}

// Response 201
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "Main Warehouse",
    "code": "WH-01",
    "address": { ... },
    "isActive": true
  }
}
```

---

## 11. Module 7 — Stocks

### Endpoints (4)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 39 | `/stocks/product/{productId}` | GET | Stocks.View | Stock by product |
| 40 | `/stocks/warehouse/{warehouseId}` | GET | Stocks.View | Stock by warehouse |
| 41 | `/stocks/low-stock-alerts` | GET | Stocks.View | Low stock list |
| 42 | `/stocks/adjust` | POST | Stocks.Adjust | Manual adjust |

> **Phase 2:** `/stocks/transfer`, `/stocks/movements` (paginated history)

### Pseudo-Code: Manual Stock Adjustment

```
AdjustStockCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { productId, warehouseId, adjustmentType: "Add"|"Remove",
         quantity, reason }

VALIDATION:
  - quantity:  GreaterThan(0)
  - reason:    NotEmpty, MaxLength(500)
  - adjustmentType: must be "Add" or "Remove"

BEGIN TRANSACTION

1. Find stock entry:
   stock = await uow.Stocks.Query()
               .FirstOrDefaultAsync(s => s.ProductId == productId
                                     && s.WarehouseId == warehouseId)

   IF stock == null:
     ── Validate product + warehouse exist
     product   = await uow.Products.GetByIdAsync(productId)
     warehouse = await uow.Warehouses.GetByIdAsync(warehouseId)
     IF product == null   → throw NotFoundException("Product not found")
     IF warehouse == null → throw NotFoundException("Warehouse not found")

     ── Auto-create stock entry at 0
     stock = Stock.Create(productId, warehouseId, initialQty: 0)
     await uow.Stocks.AddAsync(stock)
     await uow.SaveChangesAsync()  // need stock.Id for movement

2. Perform adjustment using domain method:
   movement = adjustmentType switch
   {
     "Add"    => stock.AddStock(qty, "ManualAdjustmentAdd",
                   null, "Manual", reason, currentUserId),
     "Remove" => stock.RemoveStock(qty, "ManualAdjustmentRemove",
                   null, "Manual", reason, currentUserId)
     ─ stock.RemoveStock() throws BusinessRuleViolationException
       if available < requested
   }

3. Check low stock AFTER adjustment:
   product = await uow.Products.GetByIdAsync(productId)
   IF stock.IsLowStock(product.ReorderLevel):
     ── Email alert to InventoryManagers (async, don't await)
     _ = Task.Run(() => emailService.SendLowStockAlertAsync(
           managerEmail, product.Name,
           stock.AvailableQty, product.ReorderLevel))

4. Save stock movement:
   await uow.StockMovements.AddAsync(movement)
   await uow.SaveChangesAsync()

COMMIT TRANSACTION

5. Return StockDto { productId, warehouseId, quantity, availableQty,
                     reservedQty, lastMovementType, updatedAt }
```

### Low Stock Alerts — Pseudo-Code

```
GetLowStockAlertsQueryHandler.Handle():
────────────────────────────────────────────
1. Join stocks + products where available_qty <= reorder_level

   lowStockItems = await uow.Stocks.Query()
       .Include(s => s.Product)
       .Include(s => s.Warehouse)
       .Where(s => s.Product.ReorderLevel > 0
               && (s.Quantity - s.ReservedQty) <= s.Product.ReorderLevel)
       .OrderBy(s => (s.Quantity - s.ReservedQty))  // worst first
       .ToListAsync()

2. Map and return list of LowStockAlertDto:
   {
     productId, productName, sku, warehouseName,
     currentQty, availableQty, reorderLevel, reorderQty,
     deficit: reorderLevel - availableQty
   }
```

---

## 12. Module 8 — Suppliers

### Endpoints (5)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 43 | `/suppliers` | GET | Suppliers.View | Paginated list |
| 44 | `/suppliers/{id}` | GET | Suppliers.View | By ID |
| 45 | `/suppliers` | POST | Suppliers.Create | Create |
| 46 | `/suppliers/{id}` | PUT | Suppliers.Edit | Update |
| 47 | `/suppliers/{id}` | DELETE | Suppliers.Delete | Soft delete |

> **Phase 2:** `/suppliers/{id}/products` (linking), supplier performance report

### Create Supplier — Request/Response

```json
// POST /suppliers
// Request
{
  "name": "ABC Electronics Ltd",
  "contactName": "Amit Verma",
  "email": "amit@abcelectronics.com",
  "phone": "9876543210",
  "gstNumber": "29ABCDE1234F1Z5",
  "address": {
    "street": "45 Commerce Road",
    "city": "Mumbai",
    "state": "Maharashtra",
    "pincode": "400001",
    "country": "India"
  }
}

// Response 201
{
  "success": true,
  "data": {
    "id": "uuid",
    "name": "ABC Electronics Ltd",
    "contactName": "Amit Verma",
    "email": "amit@abcelectronics.com",
    "isActive": true,
    "createdAt": "2025-04-13T10:00:00Z"
  }
}
```

---

## 13. Module 9 — Purchase Orders

### Status Flow

```
[Draft] ──Submit──► [Submitted] ──Approve──► [Approved] ──Receive──► [Received]
   │                    │                        │
 Cancel               Reject                  Cancel
   │                    │                        │
   ▼                    ▼                        ▼
[Cancelled]         [Rejected]             [Cancelled]

On RECEIVE → stock_movements inserted, stocks.quantity increased
```

### Endpoints (9)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 48 | `/purchase-orders` | GET | PurchaseOrders.View | Paginated list |
| 49 | `/purchase-orders/{id}` | GET | PurchaseOrders.View | By ID + items |
| 50 | `/purchase-orders` | POST | PurchaseOrders.Create | Create draft |
| 51 | `/purchase-orders/{id}/items` | POST | PurchaseOrders.Create | Add item |
| 52 | `/purchase-orders/{id}/items/{iid}` | DELETE | PurchaseOrders.Create | Remove item |
| 53 | `/purchase-orders/{id}/submit` | POST | PurchaseOrders.Create | Submit |
| 54 | `/purchase-orders/{id}/approve` | POST | PurchaseOrders.Approve | Approve |
| 55 | `/purchase-orders/{id}/reject` | POST | PurchaseOrders.Approve | Reject |
| 56 | `/purchase-orders/{id}/receive` | POST | PurchaseOrders.Receive | Receive → stock++ |
| 57 | `/purchase-orders/{id}/cancel` | POST | PurchaseOrders.Cancel | Cancel |

### Create Purchase Order — Request/Response

```json
// POST /purchase-orders
// Request
{
  "supplierId": "uuid",
  "warehouseId": "uuid",
  "expectedDeliveryAt": "2025-05-01T00:00:00Z",
  "notes": "Urgent order",
  "items": [
    { "productId": "uuid", "quantityOrdered": 100, "unitCost": 450.00 },
    { "productId": "uuid", "quantityOrdered": 50,  "unitCost": 1200.00 }
  ]
}

// Response 201
{
  "success": true,
  "data": {
    "id": "uuid",
    "poNumber": "PO-202504-00001",
    "status": "Draft",
    "totalAmount": 105000.00,
    "supplierName": "ABC Electronics Ltd",
    "warehouseName": "Main Warehouse",
    "items": [
      { "productName": "...", "quantityOrdered": 100, "unitCost": 450, "totalCost": 45000 }
    ]
  }
}
```

### Pseudo-Code: Create Purchase Order

```
CreatePurchaseOrderCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { supplierId, warehouseId, expectedDeliveryAt?,
         notes?, items: [{ productId, quantityOrdered, unitCost }] }

VALIDATION:
  - supplierId:   NotEmpty, must exist
  - warehouseId:  NotEmpty, must exist
  - items:        NotEmpty (at least 1 item)
  - each qty:     GreaterThan(0)
  - each unitCost: GreaterThanOrEqual(0)

1. Validate supplier + warehouse exist

2. Validate ALL product IDs exist (batch query)
   productIds   = items.Select(i => i.ProductId).ToList()
   validProducts = await uow.Products.Query()
                      .Where(p => productIds.Contains(p.Id))
                      .Select(p => p.Id)
                      .ToListAsync()
   IF validProducts.Count != productIds.Count:
     missing = productIds.Except(validProducts)
     throw NotFoundException($"Products not found: {string.Join(", ", missing)}")

3. Generate PO number:
   lastPo   = await uow.PurchaseOrders.Query()
                  .OrderByDescending(p => p.CreatedAt)
                  .Select(p => p.PoNumber)
                  .FirstOrDefaultAsync()
   sequence = ExtractSequence(lastPo) + 1
   poNumber = $"PO-{UtcNow:yyyyMM}-{sequence:D5}"
   // Example: PO-202504-00001

4. Create PurchaseOrder entity:
   po = PurchaseOrder.Create(poNumber, supplierId, warehouseId,
                              currentUserId, notes)
   po.ExpectedDeliveryAt = expectedDeliveryAt

5. Add items via domain method:
   foreach item in items:
     po.AddItem(item.ProductId, item.QuantityOrdered, item.UnitCost)
     // AddItem() also recalculates po.TotalAmount

6. await uow.PurchaseOrders.AddAsync(po)
   await uow.SaveChangesAsync()

7. Return PurchaseOrderDto
```

### Pseudo-Code: Receive Purchase Order (Critical)

```
ReceivePurchaseOrderCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { purchaseOrderId, items: [{ itemId, quantityReceived }] }

1. Load PO with items:
   po = await uow.PurchaseOrders.Query()
           .Include(p => p.Items)
           .FirstOrDefaultAsync(p => p.Id == purchaseOrderId)
   IF null → throw NotFoundException
   IF po.Status != OrderStatus.Approved:
     throw BusinessRuleViolationException("Only Approved orders can be received")

BEGIN TRANSACTION

2. Process each received item:
   foreach receivedItem in command.Items:

     poItem = po.Items.FirstOrDefault(i => i.Id == receivedItem.ItemId)
     IF poItem == null → throw NotFoundException($"Item {receivedItem.ItemId} not in this PO")

     IF receivedItem.QuantityReceived <= 0:
       throw DomainException("Received qty must be > 0")
     IF receivedItem.QuantityReceived > poItem.QuantityOrdered:
       throw BusinessRuleViolationException("Cannot receive more than ordered")

     poItem.QuantityReceived = receivedItem.QuantityReceived

     ── Find or create stock for (product, warehouse):
     stock = await FindOrCreateStockAsync(poItem.ProductId, po.WarehouseId)

     ── Domain method increments stock + creates movement record:
     movement = stock.AddStock(
       qty:           receivedItem.QuantityReceived,
       movementType:  "PurchaseReceived",
       referenceId:   po.Id,
       referenceType: "PurchaseOrder",
       notes:         $"Received via {po.PoNumber}",
       performedBy:   currentUserId
     )
     await uow.StockMovements.AddAsync(movement)

3. Mark PO as Received:
   po.MarkReceived()  // sets Status = Received, ReceivedAt = UtcNow

4. await uow.SaveChangesAsync()

COMMIT TRANSACTION

5. Send email: "PO {poNumber} has been received"
6. Return updated PurchaseOrderDto
```

---

## 14. Module 10 — Sales Orders

### Status Flow

```
[Draft] ──Submit──► [Submitted] ──Approve──► [Approved] ──Ship──► [Shipped] ──Deliver──► [Delivered]
   │                    │                        │
 Cancel               Cancel                  Cancel
   │                    │                        │
   ▼                    ▼                        ▼
[Cancelled]         [Cancelled]             [Cancelled]

On APPROVE → stock.Reserve(qty)       ← AvailableQty decreases
On SHIP    → stock.ReleaseReservation + RemoveStock  ← Actual deduction
On CANCEL (after Approve) → stock.ReleaseReservation ← Unreserve
```

### Endpoints (9)

| # | Endpoint | Method | Permission | Description |
|---|----------|--------|-----------|-------------|
| 58 | `/sales-orders` | GET | SalesOrders.View | Paginated list |
| 59 | `/sales-orders/{id}` | GET | SalesOrders.View | By ID + items |
| 60 | `/sales-orders` | POST | SalesOrders.Create | Create draft |
| 61 | `/sales-orders/{id}/items` | POST | SalesOrders.Create | Add item |
| 62 | `/sales-orders/{id}/items/{iid}` | DELETE | SalesOrders.Create | Remove item |
| 63 | `/sales-orders/{id}/submit` | POST | SalesOrders.Create | Submit |
| 64 | `/sales-orders/{id}/approve` | POST | SalesOrders.Approve | Approve + reserve |
| 65 | `/sales-orders/{id}/ship` | POST | SalesOrders.Ship | Ship + deduct stock |
| 66 | `/sales-orders/{id}/deliver` | POST | SalesOrders.Deliver | Mark delivered |
| 67 | `/sales-orders/{id}/cancel` | POST | SalesOrders.Cancel | Cancel + release |

### Pseudo-Code: Approve Sales Order (Reserve Stock)

```
ApproveSalesOrderCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { salesOrderId }

1. Load SO with items:
   so = await LoadWithItems(salesOrderId)
   IF so.Status != OrderStatus.Submitted:
     throw BusinessRuleViolationException("Only Submitted orders can be approved")

2. 4-eye rule:
   IF so.CreatedBy == currentUserId:
     throw BusinessRuleViolationException(
       "You cannot approve an order you created (4-eye principle)")

BEGIN TRANSACTION

3. PRE-CHECK: Validate stock for ALL items BEFORE any reservation
   foreach item in so.Items:
     stock = await FindStock(item.ProductId, so.WarehouseId)
     IF stock == null OR stock.AvailableQty < item.Quantity:
       product = await GetProduct(item.ProductId)
       throw BusinessRuleViolationException(
         $"Insufficient stock for '{product.Name}'. " +
         $"Available: {stock?.AvailableQty ?? 0}, Required: {item.Quantity}")
   ── If ANY item fails, throw before making ANY changes

4. Reserve stock for ALL items:
   foreach item in so.Items:
     stock = await FindStock(item.ProductId, so.WarehouseId)
     stock.Reserve(item.Quantity)
     // stock.AvailableQty = stock.Quantity - stock.ReservedQty (auto-computed)

5. Approve order:
   so.Approve(currentUserId)

6. await uow.SaveChangesAsync()

COMMIT TRANSACTION

7. Email: "Your order {so.SoNumber} has been approved"
8. Return updated SalesOrderDto
```

### Pseudo-Code: Ship Sales Order (Deduct Stock)

```
ShipSalesOrderCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { salesOrderId }

1. Load SO with items
   IF so.Status != OrderStatus.Approved:
     throw BusinessRuleViolationException("Only Approved orders can be shipped")

BEGIN TRANSACTION

2. For each item:
   stock = await FindStock(item.ProductId, so.WarehouseId)

   ── Step A: Release reservation
   stock.ReleaseReservation(item.Quantity)

   ── Step B: Actually deduct stock (RemoveStock only checks Quantity, not AvailableQty
   ──         because reservation was already released above)
   movement = stock.RemoveStock(
     qty:           item.Quantity,
     movementType:  "SaleDispatched",
     referenceId:   so.Id,
     referenceType: "SalesOrder",
     notes:         $"Shipped via {so.SoNumber}",
     performedBy:   currentUserId
   )
   await uow.StockMovements.AddAsync(movement)

   ── Step C: Low stock check after deduction
   product = await GetProduct(item.ProductId)
   IF stock.IsLowStock(product.ReorderLevel):
     _ = Task.Run(() => SendLowStockEmailAlert(product, stock))

3. Mark SO as Shipped:
   so.Ship()  // Status = Shipped, ShippedAt = UtcNow

4. await uow.SaveChangesAsync()

COMMIT TRANSACTION

5. Email notification
6. Return updated SalesOrderDto
```

### Pseudo-Code: Cancel Sales Order

```
CancelSalesOrderCommandHandler.Handle():
────────────────────────────────────────────
INPUT: { salesOrderId, reason }

1. Load SO
2. IF Status is Shipped or Delivered:
     throw BusinessRuleViolationException("Cannot cancel shipped/delivered orders")

BEGIN TRANSACTION

3. IF Status == Approved (stock was reserved):
   ── Release all reservations
   foreach item in so.Items:
     stock = await FindStock(item.ProductId, so.WarehouseId)
     IF stock != null:
       stock.ReleaseReservation(item.Quantity)

4. so.Cancel()

5. await uow.SaveChangesAsync()

COMMIT TRANSACTION

6. Return updated SalesOrderDto
```

---

## 15. MVP API Endpoint Reference — 52 Endpoints

```
BASE URL: /api/v1

═══════════════════════════════════════════════════════
AUTH (9 endpoints) — no auth unless noted
═══════════════════════════════════════════════════════
POST   /auth/register                         Register
POST   /auth/login                            Login → token pair
POST   /auth/refresh-token                    Refresh tokens
POST   /auth/logout                      [🔒] Logout
POST   /auth/verify-email                     Verify email
POST   /auth/forgot-password                  Send reset link
POST   /auth/reset-password                   Reset password
GET    /auth/me                          [🔒] Current user
POST   /auth/change-password             [🔒] Change password

═══════════════════════════════════════════════════════
USERS (10 endpoints) [Users.*]
═══════════════════════════════════════════════════════
GET    /users                            [View]       All users (paginated)
GET    /users/{id}                       [View]       By ID
POST   /users                            [Create]     Create user
PUT    /users/{id}                       [Edit]       Update
DELETE /users/{id}                       [Delete]     Soft delete
PATCH  /users/{id}/activate              [Edit]       Activate
PATCH  /users/{id}/deactivate            [Edit]       Deactivate
POST   /users/{id}/profile-image         [Edit]       Upload photo
POST   /users/{id}/assign-role           [AssignRole] Assign role
DELETE /users/{id}/revoke-role/{roleId}  [AssignRole] Revoke role

═══════════════════════════════════════════════════════
ROLES (2 endpoints — read only in MVP) [Roles.View]
═══════════════════════════════════════════════════════
GET    /roles                            All seeded roles
GET    /permissions                      All permissions

═══════════════════════════════════════════════════════
CATEGORIES (6 endpoints) [Categories.*]
═══════════════════════════════════════════════════════
GET    /categories                       [View]   All (tree)
GET    /categories/{id}                  [View]   By ID
POST   /categories                       [Create] Create
PUT    /categories/{id}                  [Edit]   Update
DELETE /categories/{id}                  [Delete] Soft delete
POST   /categories/{id}/image            [Edit]   Upload image

═══════════════════════════════════════════════════════
PRODUCTS (8 endpoints) [Products.*]
═══════════════════════════════════════════════════════
GET    /products                         [View]   Paginated list
GET    /products/{id}                    [View]   By ID + stock + images
GET    /products/sku/{sku}               [View]   By SKU
POST   /products                         [Create] Create + images
PUT    /products/{id}                    [Edit]   Update
DELETE /products/{id}                    [Delete] Soft delete
POST   /products/{id}/images             [Edit]   Upload images
DELETE /products/{id}/images/{imageId}   [Edit]   Delete image

═══════════════════════════════════════════════════════
WAREHOUSES (5 endpoints) [Warehouses.*]
═══════════════════════════════════════════════════════
GET    /warehouses                       [View]   All
GET    /warehouses/{id}                  [View]   By ID
POST   /warehouses                       [Create] Create
PUT    /warehouses/{id}                  [Edit]   Update
DELETE /warehouses/{id}                  [Delete] Soft delete

═══════════════════════════════════════════════════════
STOCKS (4 endpoints) [Stocks.*]
═══════════════════════════════════════════════════════
GET    /stocks/product/{productId}       [View]   By product
GET    /stocks/warehouse/{warehouseId}   [View]   By warehouse
GET    /stocks/low-stock-alerts          [View]   Low stock list
POST   /stocks/adjust                    [Adjust] Manual adjust

═══════════════════════════════════════════════════════
SUPPLIERS (5 endpoints) [Suppliers.*]
═══════════════════════════════════════════════════════
GET    /suppliers                        [View]   Paginated
GET    /suppliers/{id}                   [View]   By ID
POST   /suppliers                        [Create] Create
PUT    /suppliers/{id}                   [Edit]   Update
DELETE /suppliers/{id}                   [Delete] Soft delete

═══════════════════════════════════════════════════════
PURCHASE ORDERS (10 endpoints) [PurchaseOrders.*]
═══════════════════════════════════════════════════════
GET    /purchase-orders                  [View]    Paginated
GET    /purchase-orders/{id}             [View]    By ID + items
POST   /purchase-orders                  [Create]  Create draft
POST   /purchase-orders/{id}/items       [Create]  Add item
DELETE /purchase-orders/{id}/items/{iid} [Create]  Remove item
POST   /purchase-orders/{id}/submit      [Create]  Submit
POST   /purchase-orders/{id}/approve     [Approve] Approve
POST   /purchase-orders/{id}/reject      [Approve] Reject
POST   /purchase-orders/{id}/receive     [Receive] Receive → stock++
POST   /purchase-orders/{id}/cancel      [Cancel]  Cancel

═══════════════════════════════════════════════════════
SALES ORDERS (10 endpoints) [SalesOrders.*]
═══════════════════════════════════════════════════════
GET    /sales-orders                     [View]    Paginated
GET    /sales-orders/{id}                [View]    By ID + items
POST   /sales-orders                     [Create]  Create draft
POST   /sales-orders/{id}/items          [Create]  Add item
DELETE /sales-orders/{id}/items/{iid}    [Create]  Remove item
POST   /sales-orders/{id}/submit         [Create]  Submit
POST   /sales-orders/{id}/approve        [Approve] Approve + reserve stock
POST   /sales-orders/{id}/ship           [Ship]    Ship + deduct stock
POST   /sales-orders/{id}/deliver        [Deliver] Mark delivered
POST   /sales-orders/{id}/cancel         [Cancel]  Cancel + release stock

─────────────────────────────────────────────────────
TOTAL MVP ENDPOINTS: 69
(9 auth + 10 users + 2 roles + 6 categories + 8 products +
 5 warehouses + 4 stocks + 5 suppliers + 10 po + 10 so)
═════════════════════════════════════════════════════
```

---

## 16. MVP Folder Structure

```
EcommerceInventorySystem/
├── EcommerceInventorySystem.sln
├── .gitignore
│
└── src/
    │
    ├── EcommerceInventory.Domain/
    │   ├── Common/
    │   │   ├── BaseEntity.cs
    │   │   ├── AuditableEntity.cs
    │   │   └── ISoftDelete.cs
    │   ├── Entities/
    │   │   ├── User.cs
    │   │   ├── Role.cs
    │   │   ├── Permission.cs
    │   │   ├── RolePermission.cs
    │   │   ├── UserRole.cs
    │   │   ├── RefreshToken.cs
    │   │   ├── PasswordResetToken.cs
    │   │   ├── EmailVerificationToken.cs
    │   │   ├── Category.cs
    │   │   ├── Product.cs
    │   │   ├── ProductImage.cs
    │   │   ├── Warehouse.cs
    │   │   ├── Stock.cs
    │   │   ├── StockMovement.cs
    │   │   ├── Supplier.cs
    │   │   ├── PurchaseOrder.cs
    │   │   ├── PurchaseOrderItem.cs
    │   │   ├── SalesOrder.cs
    │   │   └── SalesOrderItem.cs
    │   ├── Enums/
    │   │   ├── OrderStatus.cs
    │   │   ├── StockMovementType.cs
    │   │   ├── UserStatus.cs
    │   │   └── ProductStatus.cs
    │   ├── ValueObjects/
    │   │   └── Address.cs
    │   └── Exceptions/
    │       ├── DomainException.cs
    │       ├── NotFoundException.cs
    │       ├── BusinessRuleViolationException.cs
    │       └── UnauthorizedException.cs
    │
    ├── EcommerceInventory.Application/
    │   ├── Common/
    │   │   ├── Interfaces/
    │   │   │   ├── IApplicationDbContext.cs
    │   │   │   ├── IUnitOfWork.cs
    │   │   │   ├── IRepository.cs
    │   │   │   ├── ITokenService.cs
    │   │   │   ├── IEmailService.cs
    │   │   │   ├── ICloudinaryService.cs
    │   │   │   ├── ICurrentUserService.cs
    │   │   │   └── IDateTimeService.cs
    │   │   ├── Behaviours/
    │   │   │   ├── ValidationBehaviour.cs
    │   │   │   └── LoggingBehaviour.cs
    │   │   └── Models/
    │   │       ├── Result.cs
    │   │       ├── ApiResponse.cs
    │   │       └── PagedResult.cs
    │   │
    │   ├── Features/
    │   │   ├── Auth/
    │   │   │   ├── Commands/
    │   │   │   │   ├── Register/
    │   │   │   │   │   ├── RegisterCommand.cs
    │   │   │   │   │   ├── RegisterCommandHandler.cs
    │   │   │   │   │   └── RegisterCommandValidator.cs
    │   │   │   │   ├── Login/
    │   │   │   │   │   ├── LoginCommand.cs
    │   │   │   │   │   ├── LoginCommandHandler.cs
    │   │   │   │   │   └── LoginCommandValidator.cs
    │   │   │   │   ├── RefreshToken/
    │   │   │   │   │   ├── RefreshTokenCommand.cs
    │   │   │   │   │   └── RefreshTokenCommandHandler.cs
    │   │   │   │   ├── Logout/
    │   │   │   │   │   ├── LogoutCommand.cs
    │   │   │   │   │   └── LogoutCommandHandler.cs
    │   │   │   │   ├── ForgotPassword/
    │   │   │   │   │   ├── ForgotPasswordCommand.cs
    │   │   │   │   │   ├── ForgotPasswordCommandHandler.cs
    │   │   │   │   │   └── ForgotPasswordValidator.cs
    │   │   │   │   ├── ResetPassword/
    │   │   │   │   │   ├── ResetPasswordCommand.cs
    │   │   │   │   │   ├── ResetPasswordCommandHandler.cs
    │   │   │   │   │   └── ResetPasswordValidator.cs
    │   │   │   │   ├── VerifyEmail/
    │   │   │   │   │   ├── VerifyEmailCommand.cs
    │   │   │   │   │   └── VerifyEmailCommandHandler.cs
    │   │   │   │   └── ChangePassword/
    │   │   │   │       ├── ChangePasswordCommand.cs
    │   │   │   │       ├── ChangePasswordCommandHandler.cs
    │   │   │   │       └── ChangePasswordValidator.cs
    │   │   │   └── DTOs/
    │   │   │       ├── LoginResponseDto.cs
    │   │   │       └── TokenDto.cs
    │   │   │
    │   │   ├── Users/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateUser/
    │   │   │   │   ├── UpdateUser/
    │   │   │   │   ├── DeleteUser/
    │   │   │   │   ├── ActivateUser/
    │   │   │   │   ├── DeactivateUser/
    │   │   │   │   ├── UploadProfileImage/
    │   │   │   │   ├── AssignRole/
    │   │   │   │   └── RevokeRole/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetAllUsers/
    │   │   │   │   └── GetUserById/
    │   │   │   └── DTOs/
    │   │   │       ├── UserDto.cs
    │   │   │       └── UserListDto.cs
    │   │   │
    │   │   ├── Roles/
    │   │   │   └── Queries/
    │   │   │       └── GetAllRoles/
    │   │   │           ├── GetAllRolesQuery.cs
    │   │   │           └── GetAllRolesQueryHandler.cs
    │   │   │
    │   │   ├── Categories/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateCategory/
    │   │   │   │   ├── UpdateCategory/
    │   │   │   │   ├── DeleteCategory/
    │   │   │   │   └── UploadCategoryImage/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetAllCategories/
    │   │   │   │   └── GetCategoryById/
    │   │   │   └── DTOs/
    │   │   │       └── CategoryDto.cs
    │   │   │
    │   │   ├── Products/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateProduct/
    │   │   │   │   ├── UpdateProduct/
    │   │   │   │   ├── DeleteProduct/
    │   │   │   │   └── UploadProductImages/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetAllProducts/
    │   │   │   │   ├── GetProductById/
    │   │   │   │   └── GetProductBySku/
    │   │   │   └── DTOs/
    │   │   │       └── ProductDto.cs
    │   │   │
    │   │   ├── Warehouses/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateWarehouse/
    │   │   │   │   ├── UpdateWarehouse/
    │   │   │   │   └── DeleteWarehouse/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetAllWarehouses/
    │   │   │   │   └── GetWarehouseById/
    │   │   │   └── DTOs/
    │   │   │       └── WarehouseDto.cs
    │   │   │
    │   │   ├── Stocks/
    │   │   │   ├── Commands/
    │   │   │   │   └── AdjustStock/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetStockByProduct/
    │   │   │   │   ├── GetStockByWarehouse/
    │   │   │   │   └── GetLowStockAlerts/
    │   │   │   └── DTOs/
    │   │   │       └── StockDto.cs
    │   │   │
    │   │   ├── Suppliers/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreateSupplier/
    │   │   │   │   ├── UpdateSupplier/
    │   │   │   │   └── DeleteSupplier/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetAllSuppliers/
    │   │   │   │   └── GetSupplierById/
    │   │   │   └── DTOs/
    │   │   │       └── SupplierDto.cs
    │   │   │
    │   │   ├── PurchaseOrders/
    │   │   │   ├── Commands/
    │   │   │   │   ├── CreatePurchaseOrder/
    │   │   │   │   ├── AddPurchaseOrderItem/
    │   │   │   │   ├── RemovePurchaseOrderItem/
    │   │   │   │   ├── SubmitPurchaseOrder/
    │   │   │   │   ├── ApprovePurchaseOrder/
    │   │   │   │   ├── RejectPurchaseOrder/
    │   │   │   │   ├── ReceivePurchaseOrder/
    │   │   │   │   └── CancelPurchaseOrder/
    │   │   │   ├── Queries/
    │   │   │   │   ├── GetAllPurchaseOrders/
    │   │   │   │   └── GetPurchaseOrderById/
    │   │   │   └── DTOs/
    │   │   │       └── PurchaseOrderDto.cs
    │   │   │
    │   │   └── SalesOrders/
    │   │       ├── Commands/
    │   │       │   ├── CreateSalesOrder/
    │   │       │   ├── AddSalesOrderItem/
    │   │       │   ├── RemoveSalesOrderItem/
    │   │       │   ├── SubmitSalesOrder/
    │   │       │   ├── ApproveSalesOrder/
    │   │       │   ├── ShipSalesOrder/
    │   │       │   ├── DeliverSalesOrder/
    │   │       │   └── CancelSalesOrder/
    │   │       ├── Queries/
    │   │       │   ├── GetAllSalesOrders/
    │   │       │   └── GetSalesOrderById/
    │   │       └── DTOs/
    │   │           └── SalesOrderDto.cs
    │   │
    │   └── DependencyInjection.cs
    │
    ├── EcommerceInventory.Infrastructure/
    │   ├── Persistence/
    │   │   ├── AppDbContext.cs
    │   │   ├── Configurations/
    │   │   │   ├── UserConfiguration.cs
    │   │   │   ├── RoleConfiguration.cs
    │   │   │   ├── CategoryConfiguration.cs
    │   │   │   ├── ProductConfiguration.cs
    │   │   │   ├── WarehouseConfiguration.cs
    │   │   │   ├── StockConfiguration.cs
    │   │   │   ├── SupplierConfiguration.cs
    │   │   │   ├── PurchaseOrderConfiguration.cs
    │   │   │   └── SalesOrderConfiguration.cs
    │   │   ├── Seed/
    │   │   │   └── RolePermissionSeed.cs
    │   │   ├── Repositories/
    │   │   │   └── GenericRepository.cs
    │   │   ├── UnitOfWork.cs
    │   │   └── Migrations/
    │   │
    │   ├── Services/
    │   │   ├── TokenService.cs
    │   │   ├── EmailService.cs
    │   │   ├── CloudinaryService.cs
    │   │   └── DateTimeService.cs
    │   │
    │   ├── Identity/
    │   │   └── CurrentUserService.cs
    │   │
    │   └── DependencyInjection.cs
    │
    └── EcommerceInventory.API/
        ├── Program.cs
        ├── appsettings.json
        ├── appsettings.Development.json
        │
        ├── Controllers/
        │   ├── BaseApiController.cs
        │   ├── AuthController.cs
        │   ├── UsersController.cs
        │   ├── RolesController.cs
        │   ├── CategoriesController.cs
        │   ├── ProductsController.cs
        │   ├── WarehousesController.cs
        │   ├── StocksController.cs
        │   ├── SuppliersController.cs
        │   ├── PurchaseOrdersController.cs
        │   └── SalesOrdersController.cs
        │
        ├── Middleware/
        │   ├── GlobalExceptionHandlingMiddleware.cs
        │   └── CorrelationIdMiddleware.cs
        │
        ├── Extensions/
        │   ├── ServiceExtensions.cs
        │   └── SwaggerExtensions.cs
        │
        └── Authorization/
            ├── PermissionRequirement.cs
            ├── PermissionAuthorizationHandler.cs
            └── HasPermissionAttribute.cs
```

---

## 17. MVP Database DDL

```sql
-- ═══════════════════════════════════════════════════════
-- NEON DB SETUP — MVP TABLES ONLY (19 tables)
-- ═══════════════════════════════════════════════════════

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- USERS
CREATE TABLE users (
    id                    UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    full_name             VARCHAR(150) NOT NULL,
    email                 VARCHAR(200) NOT NULL UNIQUE,
    password_hash         VARCHAR(500) NOT NULL,
    phone                 VARCHAR(20),
    profile_image_url     VARCHAR(500),
    cloudinary_profile_id VARCHAR(200),
    status                VARCHAR(20) NOT NULL DEFAULT 'Active',
    is_email_verified     BOOLEAN NOT NULL DEFAULT FALSE,
    last_login_at         TIMESTAMPTZ,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at            TIMESTAMPTZ
);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_status ON users(status) WHERE deleted_at IS NULL;

-- ROLES
CREATE TABLE roles (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name            VARCHAR(100) NOT NULL UNIQUE,
    description     VARCHAR(300),
    hierarchy_level INT NOT NULL DEFAULT 100,
    is_system_role  BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- PERMISSIONS
CREATE TABLE permissions (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name        VARCHAR(150) NOT NULL UNIQUE,
    module      VARCHAR(100) NOT NULL,
    description VARCHAR(300),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- ROLE_PERMISSIONS
CREATE TABLE role_permissions (
    role_id       UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

-- USER_ROLES
CREATE TABLE user_roles (
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id     UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    assigned_by UUID REFERENCES users(id),
    PRIMARY KEY (user_id, role_id)
);

-- REFRESH_TOKENS
CREATE TABLE refresh_tokens (
    id          UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token       VARCHAR(500) NOT NULL UNIQUE,
    expires_at  TIMESTAMPTZ NOT NULL,
    is_revoked  BOOLEAN NOT NULL DEFAULT FALSE,
    revoked_at  TIMESTAMPTZ,
    replaced_by VARCHAR(500),
    device_info VARCHAR(300),
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_refresh_tokens_token   ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);

-- PASSWORD_RESET_TOKENS
CREATE TABLE password_reset_tokens (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(500) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    is_used    BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- EMAIL_VERIFICATION_TOKENS
CREATE TABLE email_verification_tokens (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id    UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(500) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    is_used    BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- CATEGORIES
CREATE TABLE categories (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name         VARCHAR(150) NOT NULL,
    slug         VARCHAR(200) NOT NULL UNIQUE,
    description  TEXT,
    image_url    VARCHAR(500),
    cloudinary_id VARCHAR(200),
    parent_id    UUID REFERENCES categories(id) ON DELETE SET NULL,
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order   INT NOT NULL DEFAULT 0,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at   TIMESTAMPTZ
);
CREATE INDEX idx_categories_parent ON categories(parent_id);
CREATE INDEX idx_categories_slug   ON categories(slug);

-- PRODUCTS
CREATE TABLE products (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    category_id   UUID NOT NULL REFERENCES categories(id),
    name          VARCHAR(200) NOT NULL,
    slug          VARCHAR(250) NOT NULL UNIQUE,
    description   TEXT,
    sku           VARCHAR(100) NOT NULL UNIQUE,
    barcode       VARCHAR(100),
    unit_price    NUMERIC(18,2) NOT NULL DEFAULT 0,
    cost_price    NUMERIC(18,2) NOT NULL DEFAULT 0,
    reorder_level INT NOT NULL DEFAULT 0,
    reorder_qty   INT NOT NULL DEFAULT 0,
    status        VARCHAR(20) NOT NULL DEFAULT 'Active',
    weight_kg     NUMERIC(10,3),
    created_by    UUID REFERENCES users(id),
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at    TIMESTAMPTZ
);
CREATE INDEX idx_products_sku      ON products(sku);
CREATE INDEX idx_products_category ON products(category_id);
CREATE INDEX idx_products_status   ON products(status) WHERE deleted_at IS NULL;

-- PRODUCT_IMAGES
CREATE TABLE product_images (
    id            UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id    UUID NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    cloudinary_id VARCHAR(200) NOT NULL,
    image_url     VARCHAR(500) NOT NULL,
    is_primary    BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INT NOT NULL DEFAULT 0,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_product_images_product ON product_images(product_id);

-- WAREHOUSES
CREATE TABLE warehouses (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name         VARCHAR(150) NOT NULL,
    code         VARCHAR(20) NOT NULL UNIQUE,
    address_json JSONB,
    manager_id   UUID REFERENCES users(id),
    phone        VARCHAR(20),
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at   TIMESTAMPTZ
);

-- STOCKS
CREATE TABLE stocks (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    product_id      UUID NOT NULL REFERENCES products(id),
    warehouse_id    UUID NOT NULL REFERENCES warehouses(id),
    quantity        INT NOT NULL DEFAULT 0,
    reserved_qty    INT NOT NULL DEFAULT 0,
    last_counted_at TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (product_id, warehouse_id)
);
CREATE INDEX idx_stocks_product   ON stocks(product_id);
CREATE INDEX idx_stocks_warehouse ON stocks(warehouse_id);

-- STOCK_MOVEMENTS
CREATE TABLE stock_movements (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    stock_id        UUID NOT NULL REFERENCES stocks(id),
    movement_type   VARCHAR(50) NOT NULL,
    quantity        INT NOT NULL,
    quantity_before INT NOT NULL,
    quantity_after  INT NOT NULL,
    reference_id    UUID,
    reference_type  VARCHAR(50),
    notes           TEXT,
    performed_by    UUID REFERENCES users(id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_stock_movements_stock ON stock_movements(stock_id);
CREATE INDEX idx_stock_movements_ref   ON stock_movements(reference_id);

-- SUPPLIERS
CREATE TABLE suppliers (
    id           UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name         VARCHAR(200) NOT NULL,
    contact_name VARCHAR(150),
    email        VARCHAR(200),
    phone        VARCHAR(20),
    address_json JSONB,
    gst_number   VARCHAR(50),
    is_active    BOOLEAN NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at   TIMESTAMPTZ
);

-- PURCHASE_ORDERS
CREATE TABLE purchase_orders (
    id                  UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    po_number           VARCHAR(50) NOT NULL UNIQUE,
    supplier_id         UUID NOT NULL REFERENCES suppliers(id),
    warehouse_id        UUID NOT NULL REFERENCES warehouses(id),
    status              VARCHAR(30) NOT NULL DEFAULT 'Draft',
    total_amount        NUMERIC(18,2) NOT NULL DEFAULT 0,
    notes               TEXT,
    created_by          UUID NOT NULL REFERENCES users(id),
    approved_by         UUID REFERENCES users(id),
    approved_at         TIMESTAMPTZ,
    expected_delivery_at TIMESTAMPTZ,
    received_at         TIMESTAMPTZ,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_po_status   ON purchase_orders(status);
CREATE INDEX idx_po_supplier ON purchase_orders(supplier_id);

-- PURCHASE_ORDER_ITEMS
CREATE TABLE purchase_order_items (
    id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    purchase_order_id UUID NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    product_id        UUID NOT NULL REFERENCES products(id),
    quantity_ordered  INT NOT NULL,
    quantity_received INT NOT NULL DEFAULT 0,
    unit_cost         NUMERIC(18,2) NOT NULL
);

-- SALES_ORDERS
CREATE TABLE sales_orders (
    id                    UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    so_number             VARCHAR(50) NOT NULL UNIQUE,
    customer_name         VARCHAR(200) NOT NULL DEFAULT 'Walk-in Customer',
    customer_email        VARCHAR(200),
    customer_phone        VARCHAR(20),
    warehouse_id          UUID NOT NULL REFERENCES warehouses(id),
    status                VARCHAR(30) NOT NULL DEFAULT 'Draft',
    subtotal              NUMERIC(18,2) NOT NULL DEFAULT 0,
    total_amount          NUMERIC(18,2) NOT NULL DEFAULT 0,
    notes                 TEXT,
    shipping_address_json JSONB,
    created_by            UUID NOT NULL REFERENCES users(id),
    approved_by           UUID REFERENCES users(id),
    approved_at           TIMESTAMPTZ,
    shipped_at            TIMESTAMPTZ,
    delivered_at          TIMESTAMPTZ,
    created_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at            TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_so_status ON sales_orders(status);

-- SALES_ORDER_ITEMS
CREATE TABLE sales_order_items (
    id             UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    sales_order_id UUID NOT NULL REFERENCES sales_orders(id) ON DELETE CASCADE,
    product_id     UUID NOT NULL REFERENCES products(id),
    quantity       INT NOT NULL,
    unit_price     NUMERIC(18,2) NOT NULL,
    discount       NUMERIC(18,2) NOT NULL DEFAULT 0
);

-- ═══════════════════════════════════════════════════════
-- NOTE ON SALES_ORDERS:
-- MVP mein customers table nahi hai.
-- Instead: sales_orders mein customer_name, customer_email, customer_phone
-- directly store karo (denormalized).
-- Phase 2 mein proper Customers table + migration add karo.
-- ═══════════════════════════════════════════════════════
```

---

## 18. Implementation Order (Day-by-Day Plan)

```
════════════════════════════════════════════════════════
TOTAL ESTIMATED TIME: 12-15 focused days
════════════════════════════════════════════════════════

DAY 1: Project Setup + Domain Layer
─────────────────────────────────────
□ Create solution + 4 projects
□ Set up project references
□ Install all NuGet packages
□ Write ALL Domain entities (19 classes)
□ Write all Enums, Exceptions, ValueObjects
□ Domain mein koi external dependency nahi — pure C# sirf

DAY 2: Infrastructure — DB + EF Core
─────────────────────────────────────
□ AppDbContext.cs (all DbSets)
□ All IEntityTypeConfiguration files (19 configurations)
□ RolePermissionSeed.cs (8 roles + 40 permissions + assignments)
□ GenericRepository.cs
□ UnitOfWork.cs
□ Add Neon DB connection string to appsettings.json
□ First migration: dotnet ef migrations add InitialCreate
□ dotnet ef database update → verify tables in Neon dashboard

DAY 3: Infrastructure — Services
─────────────────────────────────────
□ TokenService.cs (JWT access + refresh)
□ EmailService.cs (Gmail SMTP MailKit)
□ CloudinaryService.cs
□ CurrentUserService.cs
□ DateTimeService.cs
□ DependencyInjection.cs (Infrastructure)

DAY 4: Application Layer Setup + Auth Module
─────────────────────────────────────
□ All Common interfaces (IRepository, IUnitOfWork, etc.)
□ Result.cs, ApiResponse.cs, PagedResult.cs
□ ValidationBehaviour + LoggingBehaviour
□ MappingProfile.cs (AutoMapper)
□ DependencyInjection.cs (Application)
□ ALL Auth Commands + Handlers + Validators
   (Register, Login, RefreshToken, Logout,
    VerifyEmail, ForgotPassword, ResetPassword, ChangePassword)

DAY 5: API Layer + Auth Controller
─────────────────────────────────────
□ Program.cs (full setup)
□ BaseApiController.cs
□ AuthController.cs
□ GlobalExceptionHandlingMiddleware.cs
□ CorrelationIdMiddleware.cs
□ Authorization (PermissionRequirement + Handler + Attribute)
□ SwaggerExtensions.cs (JWT in Swagger)
□ ServiceExtensions.cs
□ TEST: Postman/Swagger — Register → Login → Refresh → Logout ✓

DAY 6: Users Module
─────────────────────────────────────
□ All User Commands + Handlers + Validators + DTOs
□ All User Queries + Handlers
□ UsersController.cs
□ TEST: Create user, assign role, upload profile image ✓

DAY 7: Categories + Products
─────────────────────────────────────
□ Category Commands + Queries + DTOs + Controller
□ Product Commands + Queries + DTOs + Controller
□ TEST: Create category → Create product with images ✓

DAY 8: Warehouses + Stocks
─────────────────────────────────────
□ Warehouse Commands + Queries + DTOs + Controller
□ Stock Queries + AdjustStock Command + Controller
□ Low stock alert logic
□ TEST: Create warehouse → Adjust stock → See low stock alerts ✓

DAY 9: Suppliers
─────────────────────────────────────
□ Supplier Commands + Queries + DTOs + Controller
□ TEST: CRUD suppliers ✓

DAY 10: Purchase Orders
─────────────────────────────────────
□ ALL Purchase Order Commands (8 commands)
□ Purchase Order Queries + DTOs
□ PurchaseOrdersController.cs
□ TEST: Create PO → Add items → Submit → Approve → Receive → Check stock ✓
        (THIS IS THE CORE FLOW — test everything)

DAY 11: Sales Orders
─────────────────────────────────────
□ ALL Sales Order Commands (8 commands)
□ Sales Order Queries + DTOs
□ SalesOrdersController.cs
□ TEST: Create SO → Submit → Approve (check reservation) →
        Ship (check deduction) → Deliver ✓
□ TEST: Cancel after approve → check reservation released ✓

DAY 12: Integration Testing + Bug Fix
─────────────────────────────────────
□ Full end-to-end test in Postman:
  1. Register SuperAdmin user
  2. Login → get token
  3. Create category → Create product
  4. Create warehouse
  5. Create supplier
  6. Create PO → receive → stock goes up
  7. Create SO → approve (reserved) → ship (stock goes down)
  8. Check low stock alerts
□ Fix all bugs found
□ Test all 69 endpoints at least once

DAY 13: Polish + Docs
─────────────────────────────────────
□ appsettings.json final config
□ Swagger descriptions for all endpoints
□ README.md with setup steps
□ .gitignore cleanup
□ Remove console.log debug code
□ Verify Serilog file logging works

DAY 14: Deploy
─────────────────────────────────────
□ See Deploy Checklist (Section 19)
□ Deploy to Railway or Render
□ Test all endpoints on live URL
□ Update Swagger baseUrl
```

---

## 19. Deploy Checklist

### Option A: Railway (Recommended — Easier)

```
RAILWAY SETUP:
──────────────
1. railway.app pe account banao (GitHub se login)
2. New Project → Deploy from GitHub repo
3. Add environment variables (from appsettings):
   - ConnectionStrings__DefaultConnection  = Neon DB connection string
   - JwtSettings__SecretKey               = your secret key
   - JwtSettings__Issuer                  = EcommerceInventoryAPI
   - JwtSettings__Audience                = EcommerceInventoryClients
   - JwtSettings__AccessTokenExpMinutes   = 15
   - JwtSettings__RefreshTokenExpDays     = 7
   - EmailSettings__Host                  = smtp.gmail.com
   - EmailSettings__Port                  = 587
   - EmailSettings__Username              = your@gmail.com
   - EmailSettings__AppPassword           = your-app-password
   - EmailSettings__SenderEmail           = your@gmail.com
   - EmailSettings__SenderName            = EcommerceInventory
   - CloudinarySettings__CloudName        = your-cloud-name
   - CloudinarySettings__ApiKey           = your-api-key
   - CloudinarySettings__ApiSecret        = your-api-secret
   - AppSettings__BaseUrl                 = https://your-railway-url.railway.app
   - ASPNETCORE_ENVIRONMENT               = Production
4. Railway auto-detects .NET project + Dockerfile
   (ya manually Dockerfile daal — see below)
5. Deploy → get URL → test Swagger
```

### Dockerfile (Railway/Render ke liye)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/EcommerceInventory.API/EcommerceInventory.API.csproj", "EcommerceInventory.API/"]
COPY ["src/EcommerceInventory.Application/EcommerceInventory.Application.csproj", "EcommerceInventory.Application/"]
COPY ["src/EcommerceInventory.Infrastructure/EcommerceInventory.Infrastructure.csproj", "EcommerceInventory.Infrastructure/"]
COPY ["src/EcommerceInventory.Domain/EcommerceInventory.Domain.csproj", "EcommerceInventory.Domain/"]
RUN dotnet restore "EcommerceInventory.API/EcommerceInventory.API.csproj"
COPY src/ .
WORKDIR "/src/EcommerceInventory.API"
RUN dotnet build -c Release -o /app/build
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EcommerceInventory.API.dll"]
```

### Pre-Deploy Checks

```
□ appsettings.json mein NO hardcoded secrets (use env vars)
□ Neon DB connection string mein SSL Mode=Require
□ Program.cs mein auto-migration on startup:
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();  ← creates tables + seeds data
    }
□ Swagger only in Development:
    if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
    ── Railway pe Swagger off rahega, which is fine
□ CORS configured properly for your frontend URL
□ Serilog file sink — ensure /logs/ directory writable in container
□ Test 1 endpoint after deploy to confirm DB connected
```

---

## 20. Post-MVP Daily Update Plan

### Phase 2 (Add after MVP is live)

```
DEPLOY DAY → System is LIVE ✓

POST-DEPLOY — Daily Updates (1-2 features per day):
────────────────────────────────────────────────────

DAY +1:  Add Customers module
         → customers table migration
         → Link sales_orders.customer_id (FK to customers)
         → 5 endpoints: CRUD + order history
         → Test

DAY +2:  Add Stock Transfer
         → POST /stocks/transfer
         → TransferStockCommand + Handler
         → 2 movements: TransferOut + TransferIn linked by transferId
         → Test

DAY +3:  Add In-App Notifications
         → notifications table migration
         → On PO receive → notify PurchaseManagers
         → On SO ship → notify SalesManagers
         → On low stock → notify InventoryManagers
         → 5 notification endpoints
         → Test

DAY +4:  Add Roles CRUD API
         → GET/POST/PUT/DELETE /roles
         → PUT /roles/{id}/permissions
         → Already seeded, now just expose management APIs
         → Test

DAY +5:  Add Stock Movements History
         → GET /stock-movements (paginated, filtered)
         → GET /stock-movements/product/{id}
         → GET /stock-movements/warehouse/{id}
         → Test

DAY +6:  Add Supplier-Product Linking
         → supplier_products table migration
         → GET/POST/DELETE /suppliers/{id}/products
         → Test

DAY +7:  Add Audit Logs
         → audit_logs table migration
         → Log all Create/Update/Delete/Login actions
         → GET /audit-logs (SuperAdmin only)
         → Test

WEEK 2: Add Reports module
         → GET /reports/inventory (snapshot)
         → GET /reports/inventory/valuation
         → GET /reports/low-stock
         → GET /reports/sales (by date range)
         → GET /reports/purchases (by date range)
         → GET /reports/top-products

WEEK 3: Warehouse Zones, Product Variants,
         Warehouse Images, Advanced Filtering
```

---

## 📊 MVP Summary

```
┌─────────────────────────────────────────────────────────┐
│                  MVP AT A GLANCE                        │
├─────────────────────────────────────────────────────────┤
│  Projects        │  4 (Domain, App, Infra, API)         │
│  DB Tables       │  19 tables                           │
│  API Endpoints   │  69 endpoints                        │
│  Modules         │  10 (Auth→Users→Roles→Cat→Prod→      │
│                  │      Warehouse→Stock→Supplier→PO→SO) │
│  Auth            │  Dual JWT (15min + 7-day rotation)   │
│  Roles           │  8 seeded roles, 40 permissions      │
│  Images          │  Cloudinary (profiles+products+cats) │
│  Email           │  Gmail SMTP (verify+reset+alerts)    │
│  Deploy Target   │  Railway / Render (free tier)        │
│  Est. Build Time │  12-14 focused days                  │
├─────────────────────────────────────────────────────────┤
│  WHAT WORKS END-TO-END IN MVP:                         │
│  ✓ Register → Login → Manage users with roles          │
│  ✓ Create categories → products with images            │
│  ✓ Create warehouses → manage stock                    │
│  ✓ Create suppliers → raise purchase orders            │
│  ✓ Receive PO → stock automatically goes up            │
│  ✓ Create sales order → approve (reserve) → ship       │
│    (stock automatically goes down)                     │
│  ✓ See low stock alerts                                │
│  ✓ All RBAC permissions enforced                       │
│  ✓ Email notifications for key events                  │
└─────────────────────────────────────────────────────────┘
```

---

*MVP PRD v1.0.0 — Derived from Full PRD v1.0.0*  
*Build → Deploy → Iterate. Start with this. Add daily.*
