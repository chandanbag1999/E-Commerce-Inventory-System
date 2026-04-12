# Frontend Auth, Home and 12 Endpoint Plan

## Scope

This document maps the current backend API surface against the current frontend needs for:

- Login
- Register
- Home page dashboard
- Notification center used from the home layout

Target frontend endpoint set:

1. `POST /api/v1/auth/login`
2. `POST /api/v1/auth/register`
3. `GET /api/v1/auth/me`
4. `POST /api/v1/auth/refresh`
5. `POST /api/v1/auth/logout`
6. `GET /api/v1/analytics/dashboard`
7. `GET /api/v1/orders?limit=5`
8. `GET /api/v1/notifications`
9. `PATCH /api/v1/notifications/{id}/read`
10. `PATCH /api/v1/notifications/read-all`
11. `DELETE /api/v1/notifications/{id}`
12. `DELETE /api/v1/notifications/clear-all`

## Current Backend Endpoints Relevant To Frontend

### Auth

- `POST /api/v1/auth/register`
- `POST /api/v1/auth/login`
- `POST /api/v1/auth/refresh`
- `POST /api/v1/auth/logout`
- `GET /api/v1/auth/me`

### Products

- `GET /api/v1/products`
- `GET /api/v1/products/{id}`
- `GET /api/v1/products/slug/{slug}`
- `POST /api/v1/products`
- `PUT /api/v1/products/{id}`
- `POST /api/v1/products/{id}/publish`
- `POST /api/v1/products/{id}/unpublish`
- `DELETE /api/v1/products/{id}`
- `POST /api/v1/products/{id}/feature`
- `POST /api/v1/products/{productId}/variants`
- `PUT /api/v1/products/variants/{variantId}`
- `DELETE /api/v1/products/variants/{variantId}`
- `POST /api/v1/products/{productId}/variants/{variantId}/default`
- `POST /api/v1/products/{productId}/images`
- `DELETE /api/v1/products/images/{mediaId}`
- `POST /api/v1/products/{productId}/images/{mediaId}/primary`
- `PUT /api/v1/products/{productId}/images/order`

### Categories

- `GET /api/v1/categories`
- `GET /api/v1/categories/{id}`
- `POST /api/v1/categories`
- `PUT /api/v1/categories/{id}`
- `DELETE /api/v1/categories/{id}`
- `POST /api/v1/categories/{id}/image`

### Orders

- `POST /api/v1/orders`
- `GET /api/v1/orders/{orderId}`
- `GET /api/v1/orders/number/{orderNumber}`
- `GET /api/v1/orders/my-orders`
- `POST /api/v1/orders/{orderId}/confirm-payment`
- `POST /api/v1/orders/{orderId}/cancel`
- `POST /api/v1/orders/{orderId}/return`
- `GET /api/v1/orders/summary`
- `GET /api/v1/orders/pricing/calculate`
- `GET /api/v1/orders`
- `PUT /api/v1/orders/{orderId}/status`
- `POST /api/v1/orders/{orderId}/ship`
- `POST /api/v1/orders/{orderId}/deliver`
- `POST /api/v1/orders/{orderId}/return/{returnItemId}/process`

### Inventory

- `GET /api/v1/inventory/items/{id}`
- `GET /api/v1/inventory/warehouse/{warehouseId}/items`
- `GET /api/v1/inventory/product/{productId}/items`
- `GET /api/v1/inventory/low-stock`
- `GET /api/v1/inventory/out-of-stock`
- `POST /api/v1/inventory/items`
- `POST /api/v1/inventory/items/{id}/adjust`
- `POST /api/v1/inventory/stock-in`
- `POST /api/v1/inventory/stock-out`
- `POST /api/v1/inventory/damaged`
- `GET /api/v1/inventory/items/{inventoryItemId}/movements`
- `POST /api/v1/inventory/reserve`
- `GET /api/v1/inventory/reservations/{code}`
- `POST /api/v1/inventory/reservations/{id}/confirm`
- `POST /api/v1/inventory/reservations/{id}/release`
- `POST /api/v1/inventory/reservations/process-expired`

### Alerts

- `GET /api/v1/alerts`
- `POST /api/v1/alerts/{id}/read`
- `POST /api/v1/alerts/{id}/resolve`
- `POST /api/v1/alerts/generate`

### Warehouses

- `GET /api/v1/warehouses`
- `GET /api/v1/warehouses/{id}`
- `GET /api/v1/warehouses/nearby`
- `POST /api/v1/warehouses`
- `PUT /api/v1/warehouses/{id}`
- `DELETE /api/v1/warehouses/{id}`

### Payments

- `POST /api/v1/payments/create`
- `GET /api/v1/payments/{paymentId}`
- `GET /api/v1/payments/order/{orderId}`
- `POST /api/v1/payments/webhook/{provider}`
- `POST /api/v1/payments/refund`
- `GET /api/v1/payments/{paymentId}/refunds`
- `POST /api/v1/payments/reconcile`

### User Profile And User Management

- `GET /api/v1/users/profile`
- `PUT /api/v1/users/profile`
- `POST /api/v1/users/profile/picture`
- `PUT /api/v1/users/password`
- `PUT /api/v1/users/notifications`
- `POST /api/v1/users/gdpr/export`
- `POST /api/v1/users/gdpr/delete`
- `GET /api/v1/users`
- `GET /api/v1/users/{id}/profile`
- `PUT /api/v1/users/{id}/suspend`
- `PUT /api/v1/users/{id}/activate`
- `DELETE /api/v1/users/{id}`

### Vendor And Addresses

- `POST /api/v1/vendors/apply`
- `GET /api/v1/vendors/my-application`
- `PUT /api/v1/vendors/applications/{id}/bank-details`
- `POST /api/v1/vendors/applications/{id}/submit`
- `GET /api/v1/vendors/applications`
- `GET /api/v1/vendors/applications/{id}`
- `POST /api/v1/vendors/applications/{id}/review`
- `GET /api/v1/addresses`
- `GET /api/v1/addresses/{id}`
- `POST /api/v1/addresses`
- `PUT /api/v1/addresses/{id}`
- `DELETE /api/v1/addresses/{id}`
- `PUT /api/v1/addresses/{id}/set-default`

## Frontend Contract Status For The 12 Target Endpoints

| # | Frontend target | Current backend status | Notes |
|---|---|---|---|
| 1 | `POST /auth/login` | Exists | Route is already present and usable. |
| 2 | `POST /auth/register` | Exists with contract gap | Backend register DTO does not accept frontend role selection. Backend also auto assigns `Staff`. |
| 3 | `GET /auth/me` | Exists with response mapping gap | Route exists, but frontend role values need normalization. |
| 4 | `POST /auth/refresh` | Exists | Route is already present and usable. |
| 5 | `POST /auth/logout` | Exists | Route is already present and usable. |
| 6 | `GET /analytics/dashboard` | Missing | No analytics controller or dashboard aggregate endpoint exists. |
| 7 | `GET /orders?limit=5` | Partial | `GET /api/v1/orders` exists, but it uses `PageNumber` and `PageSize`, not `limit`. |
| 8 | `GET /notifications` | Missing | No notifications controller exists. `AlertsController` is not a full replacement. |
| 9 | `PATCH /notifications/{id}/read` | Missing | Closest route is `POST /api/v1/alerts/{id}/read`. Route and semantics differ. |
| 10 | `PATCH /notifications/read-all` | Missing | No bulk read endpoint exists. |
| 11 | `DELETE /notifications/{id}` | Missing | No delete notification endpoint exists. |
| 12 | `DELETE /notifications/clear-all` | Missing | No clear all notifications endpoint exists. |

## Important Gaps Before Integration

### 1. Role naming mismatch

Frontend roles:

- `admin`
- `seller`
- `warehouse`
- `delivery`

Backend seeded roles:

- `SuperAdmin`
- `Admin`
- `InventoryManager`
- `SalesManager`
- `Staff`

Impact:

- Login success can still happen, but the frontend role-based routing and guards will not align cleanly.
- Register currently cannot create a user as `seller`, `warehouse`, or `delivery` because backend register flow always assigns `Staff`.

### 2. Permission naming mismatch

Frontend permission style uses plural resources such as:

- `orders:view`
- `products:create`
- `analytics:view`

Backend permission and policy style currently uses singular resources such as:

- `order:read`
- `product:create`
- `dashboard:view`

Impact:

- `useAuthStore` and permission checks on the frontend will need a mapping layer or the backend payload must be normalized.

### 3. Profile route mismatch

Frontend endpoint config expects:

- `/profile`
- `/profile/password`
- `/profile/avatar`

Backend currently exposes:

- `/api/v1/users/profile`
- `/api/v1/users/password`
- `/api/v1/users/profile/picture`

### 4. Notifications do not exist as a backend module

The current `AlertsController` only handles inventory alerts. The frontend notification center expects a general-purpose stream with types such as:

- `order`
- `stock`
- `delivery`
- `system`
- `alert`
- `payment`

### 5. Dashboard data is still static on the frontend

`DashboardPage.tsx` currently uses hardcoded:

- KPI cards
- revenue chart data
- weekly order trend
- category pie chart
- activity feed

So even after adding `GET /analytics/dashboard`, a concrete dashboard response contract must still be defined.

## Plan To Reach Full 12 Endpoint Support

### Phase 1. Freeze The Contract

1. Confirm the exact frontend request and response shape for all 12 endpoints.
2. Decide one role strategy:
   - map backend roles to frontend roles in API responses, or
   - change backend roles to match frontend names.
3. Decide one permission strategy:
   - map backend permissions to frontend permission names, or
   - standardize backend permission names to the frontend contract.
4. Decide whether `GET /api/v1/orders` will accept `limit` directly or the frontend will send `pageSize=5`.

### Phase 2. Finish Auth Contract

Current auth routes already exist, so the work here is mostly contract alignment.

Tasks:

1. Extend `RegisterRequestDto` if frontend role selection must be honored.
2. Update `AuthService.RegisterAsync` to assign the correct role instead of always assigning `Staff`.
3. Normalize the `CurrentUserDto.Role` value to a frontend-safe value such as `admin`, `seller`, `warehouse`, or `delivery`.
4. Normalize the permissions list if frontend permission checks will use backend values directly.
5. Verify `AuthController` responses already match frontend token storage needs:
   - `accessToken`
   - `refreshToken`
   - current user payload

Files likely affected:

- `src/EIVMS.API/Controllers/v1/AuthController.cs`
- `src/EIVMS.Application/Modules/Identity/DTOs/RegisterRequestDto.cs`
- `src/EIVMS.Application/Modules/Identity/Services/AuthService.cs`
- `src/EIVMS.Infrastructure/Seeders/RolePermissionSeeder.cs`

### Phase 3. Add Dashboard Analytics Endpoint

Add `GET /api/v1/analytics/dashboard`.

Recommended response sections:

- `stats`
- `revenueOverview`
- `salesByCategory`
- `ordersThisWeek`
- `recentActivity`

Implementation work:

1. Create `AnalyticsController` under `src/EIVMS.API/Controllers/v1`.
2. Add analytics application service and interface.
3. Create dashboard response DTOs.
4. Aggregate data from existing modules:
   - orders for totals, revenue, weekly trend
   - products or categories for category chart
   - inventory or alerts for low stock and activity items
5. Add authorization policy for dashboard access.
6. Add unit tests for aggregation and controller response.

### Phase 4. Align Recent Orders Endpoint For Home Page

Frontend home page needs the latest 5 rows.

Options:

1. Minimal option:
   - reuse `GET /api/v1/orders`
   - accept `pageSize=5`
   - keep sorting by `createdAt desc`
2. Contract-first option:
   - add support for `limit`
   - optionally accept `sortBy=createdAt&sortDirection=desc`

Implementation work:

1. Update `OrderFilterDto` if `limit` is required.
2. Update controller model binding if aliasing is needed.
3. Ensure repository paging returns latest first.
4. Verify the response contains fields needed by the dashboard table.

Files likely affected:

- `src/EIVMS.API/Controllers/v1/Orders/OrdersController.cs`
- `src/EIVMS.Application/Modules/Orders/DTOs/OrderDtos.cs`
- `src/EIVMS.Infrastructure/Repositories/Orders/OrderRepository.cs`

### Phase 5. Create Notification Module

Add these endpoints:

1. `GET /api/v1/notifications`
2. `PATCH /api/v1/notifications/{id}/read`
3. `PATCH /api/v1/notifications/read-all`
4. `DELETE /api/v1/notifications/{id}`
5. `DELETE /api/v1/notifications/clear-all`

Recommended notification fields:

- `id`
- `type`
- `title`
- `message`
- `read`
- `actionUrl`
- `createdAt`
- `priority`
- `userId`

Implementation work:

1. Create notification entity in domain.
2. Add EF configuration and migration.
3. Add repository and service layer.
4. Add controller and DTOs.
5. Add read, bulk read, delete, and clear-all operations scoped to current user.
6. Add authorization for authenticated users.
7. Seed or generate initial data if needed for development.

### Phase 6. Produce Notifications From Business Events

Notification endpoints alone are not enough. They also need data producers.

Create notifications from events such as:

- new order placed
- low stock alert generated
- delivery status changes
- payment success or refund
- system messages

Minimal first version:

1. Reuse existing inventory alert generation for `stock` notifications.
2. Create notification records inside order status changes.
3. Add a simple system notification seeder for admin announcements.

### Phase 7. Response Normalization Layer

For smooth frontend integration, keep one consistent API response strategy.

Decide whether to:

1. Keep the current envelope shape:
   - `success`
   - `message`
   - `data`
   - `errors`
   - `statusCode`
2. Or expose a frontend-specific thin adapter layer on the frontend.

Recommendation:

- Keep backend envelope as-is.
- Normalize role and permission values in backend DTOs for auth-related endpoints.
- Keep home/dashboard endpoints dedicated and frontend-shaped.

### Phase 8. Testing

Required tests before calling the 12 endpoints complete:

1. Auth controller tests:
   - register
   - login
   - refresh
   - logout
   - me
2. Analytics dashboard tests:
   - stats aggregation
   - chart datasets
   - activity feed ordering
3. Orders list tests:
   - latest 5 sorting
   - query parameter binding
4. Notification tests:
   - list only current user notifications
   - mark one read
   - mark all read
   - delete one
   - clear all

### Phase 9. Suggested Implementation Order

1. Auth contract alignment
2. Recent orders query alignment
3. Analytics dashboard endpoint
4. Notification module storage and CRUD endpoints
5. Notification producers from order and inventory events
6. Final integration testing with frontend

## Recommended Definition Of Done

The backend is ready for the current frontend login, register and home flow when all 12 conditions are true:

1. Login returns valid tokens plus normalized frontend role.
2. Register can create a user in a role compatible with frontend routing, or frontend role mapping is formally handled.
3. `GET /auth/me` restores the exact user contract needed by the frontend store.
4. Refresh rotation works.
5. Logout revokes refresh token.
6. Dashboard endpoint returns live KPI cards.
7. Dashboard endpoint returns live chart data.
8. Recent orders endpoint returns the latest 5 records in descending order.
9. Notifications list returns general-purpose notifications, not only inventory alerts.
10. Single notification read works.
11. Bulk notification read works.
12. Delete and clear-all notification flows work.

## Current Reality Summary

- Auth base is already implemented.
- Orders base is already implemented.
- Analytics dashboard is not implemented.
- Notification center backend is not implemented.
- Frontend role names do not match backend seeded roles.
- Frontend permission naming does not match backend permission naming.

Because of that, the backend is not yet ready for full current login, register and home page integration even though some core auth routes already exist.
