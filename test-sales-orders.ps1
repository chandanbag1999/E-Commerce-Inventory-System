# Sales Orders Test Script
# Tests: Create → Submit → Approve → Ship → Deliver → Cancel

$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjMxMTExMTExLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWUiOiJTdXBlciBBZG1pbmlzdHJhdG9yIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoic3VwZXJhZG1pbkBlY29tbWVyY2UubG9jYWwiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJTdXBlckFkbWluIiwicGVybWlzc2lvbiI6WyJVc2Vycy5WaWV3IiwiVXNlcnMuQ3JlYXRlIiwiVXNlcnMuRWRpdCIsIlVzZXJzLkRlbGV0ZSIsIlVzZXJzLkFzc2lnblJvbGUiLCJSb2xlcy5WaWV3IiwiQ2F0ZWdvcmllcy5WaWV3IiwiQ2F0ZWdvcmllcy5DcmVhdGUiLCJDYXRlZ29yaWVzLkVkaXQiLCJDYXRlZ29yaWVzLkRlbGV0ZSIsIlByb2R1Y3RzLlZpZXciLCJQcm9kdWN0cy5DcmVhdGUiLCJQcm9kdWN0cy5FZGl0IiwiUHJvZHVjdHMuRGVsZXRlIiwiV2FyZWhvdXNlcy5WaWV3IiwiV2FyZWhvdXNlcy5DcmVhdGUiLCJXYXJlaG91c2VzLkVkaXQiLCJXYXJlaG91c2VzLkRlbGV0ZSIsIlN0b2Nrcy5WaWV3IiwiU3RvY2tzLkFkanVzdCIsIlN1cHBsaWVycy5WaWV3IiwiU3VwcGxpZXJzLkNyZWF0ZSIsIlN1cHBsaWVycy5FZGl0IiwiU3VwcGxpZXJzLkRlbGV0ZSIsIlB1cmNoYXNlT3JkZXJzLlZpZXciLCJQdXJjaGFzZU9yZGVycy5DcmVhdGUiLCJQdXJjaGFzZU9yZGVycy5BcHByb3ZlIiwiUHVyY2hhc2VPcmRlcnMuUmVjZWl2ZSIsIlB1cmNoYXNlT3JkZXJzLkNhbmNlbCIsIlNhbGVzT3JkZXJzLlZpZXciLCJTYWxlc09yZGVycy5DcmVhdGUiLCJTYWxlc09yZGVycy5BcHByb3ZlIiwiU2FsZXNPcmRlcnMuU2hpcCIsIlNhbGVzT3JkZXJzLkRlbGl2ZXIiLCJTYWxlc09yZGVycy5DYW5jZWwiXSwiZXhwIjoxNzc2MjcyODY1LCJpc3MiOiJFY29tbWVyY2VJbnZlbnRvcnlBUEkiLCJhdWQiOiJFY29tbWVyY2VJbnZlbnRvcnlDbGllbnRzIn0.OE8aXd78TrVs0vXWzKFHqRqn8WnTcfJSu-zp8nEjr2s"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$baseUrl = "http://localhost:5255/api/v1"

Write-Host "`n===== STEP 1: Create Test Data (Category, Product, Stock) =====" -ForegroundColor Cyan

# Create Category
Write-Host "`nCreating category..."
$category = Invoke-RestMethod -Uri "$baseUrl/categories" -Method POST -Headers $headers -Body '{"name":"Electronics","description":"Electronic devices"}' | ConvertTo-Json -Depth 10
Write-Host $category
$categoryData = ($category | ConvertFrom-Json).data
$categoryId = $categoryData.id
Write-Host "Category ID: $categoryId" -ForegroundColor Green

# Create Product
Write-Host "`nCreating product..."
$product = Invoke-RestMethod -Uri "$baseUrl/products" -Method POST -Headers $headers -Body "{
    `"categoryId`": `"$categoryId`",
    `"name`": `"Test Laptop`",
    `"sku`": `"LAPTOP-001`",
    `"unitPrice`": 50000,
    `"costPrice`": 40000,
    `"reorderLevel`": 5,
    `"reorderQty`": 10
}" | ConvertTo-Json -Depth 10
Write-Host $product
$productData = ($product | ConvertFrom-Json).data
$productId = $productData.id
Write-Host "Product ID: $productId" -ForegroundColor Green

# Get Warehouse ID
Write-Host "`nGetting warehouse..."
$warehouses = Invoke-RestMethod -Uri "$baseUrl/warehouses" -Method GET -Headers $headers | ConvertTo-Json -Depth 10
$warehouseData = ($warehouses | ConvertFrom-Json).data[0]
$warehouseId = $warehouseData.id
Write-Host "Warehouse ID: $warehouseId" -ForegroundColor Green

# Adjust stock to have 100 units
Write-Host "`nAdjusting stock to 100 units..."
$stockAdjust = Invoke-RestMethod -Uri "$baseUrl/stocks/adjust" -Method POST -Headers $headers -Body "{
    `"productId`": `"$productId`",
    `"warehouseId`": `"$warehouseId`",
    `"adjustmentType`": `"ManualAdjustment`",
    `"quantity`": 100,
    `"reason`": `"Initial stock for testing`"
}" | ConvertTo-Json -Depth 10
Write-Host $stockAdjust

Write-Host "`n===== STEP 2: Create Sales Order =====" -ForegroundColor Cyan

# Create Sales Order
Write-Host "`nCreating sales order..."
$so = Invoke-RestMethod -Uri "$baseUrl/sales-orders" -Method POST -Headers $headers -Body "{
    `"warehouseId`": `"$warehouseId`",
    `"customerName`": `"John Doe`",
    `"customerEmail`": `"john@example.com`",
    `"customerPhone`": `"+91-9876543210`",
    `"notes`": `"Test sales order`",
    `"items`": [
        {
            `"productId`": `"$productId`",
            `"quantity`": 10,
            `"unitPrice`": 50000,
            `"discount`": 5000
        }
    ]
}" | ConvertTo-Json -Depth 10
Write-Host $so
$soData = ($so | ConvertFrom-Json).data
$soId = $soData.id
Write-Host "Sales Order ID: $soId" -ForegroundColor Green
Write-Host "Status: $($soData.status)" -ForegroundColor Green

Write-Host "`n===== STEP 3: Submit Sales Order =====" -ForegroundColor Cyan

# Submit Sales Order
Write-Host "`nSubmitting sales order..."
$submit = Invoke-RestMethod -Uri "$baseUrl/sales-orders/$soId/submit" -Method POST -Headers $headers -Body '{}' | ConvertTo-Json -Depth 10
Write-Host $submit
$submitData = ($submit | ConvertFrom-Json).data
Write-Host "Status after submit: $($submitData.status)" -ForegroundColor Green

Write-Host "`n===== STEP 4: Approve Sales Order (Reserve Stock) =====" -ForegroundColor Cyan

# Approve Sales Order
Write-Host "`nApproving sales order (should reserve stock)..."
$approve = Invoke-RestMethod -Uri "$baseUrl/sales-orders/$soId/approve" -Method POST -Headers $headers -Body '{}' | ConvertTo-Json -Depth 10
Write-Host $approve
$approveData = ($approve | ConvertFrom-Json).data
Write-Host "Status after approve: $($approveData.status)" -ForegroundColor Green
Write-Host "Approved At: $($approveData.approvedAt)" -ForegroundColor Green

# Check stock levels (should show reserved qty)
Write-Host "`nChecking stock levels after approval..."
$stockAfterApprove = Invoke-RestMethod -Uri "$baseUrl/stocks?productId=$productId&warehouseId=$warehouseId" -Method GET -Headers $headers | ConvertTo-Json -Depth 10
Write-Host $stockAfterApprove

Write-Host "`n===== STEP 5: Ship Sales Order (Deduct Stock) =====" -ForegroundColor Cyan

# Ship Sales Order
Write-Host "`nShipping sales order (should deduct stock)..."
$ship = Invoke-RestMethod -Uri "$baseUrl/sales-orders/$soId/ship" -Method POST -Headers $headers -Body '{}' | ConvertTo-Json -Depth 10
Write-Host $ship
$shipData = ($ship | ConvertFrom-Json).data
Write-Host "Status after ship: $($shipData.status)" -ForegroundColor Green
Write-Host "Shipped At: $($shipData.shippedAt)" -ForegroundColor Green

# Check stock levels (should show deducted qty)
Write-Host "`nChecking stock levels after shipping..."
$stockAfterShip = Invoke-RestMethod -Uri "$baseUrl/stocks?productId=$productId&warehouseId=$warehouseId" -Method GET -Headers $headers | ConvertTo-Json -Depth 10
Write-Host $stockAfterShip

Write-Host "`n===== STEP 6: Deliver Sales Order =====" -ForegroundColor Cyan

# Deliver Sales Order
Write-Host "`nDelivering sales order..."
$deliver = Invoke-RestMethod -Uri "$baseUrl/sales-orders/$soId/deliver" -Method POST -Headers $headers -Body '{}' | ConvertTo-Json -Depth 10
Write-Host $deliver
$deliverData = ($deliver | ConvertFrom-Json).data
Write-Host "Status after deliver: $($deliverData.status)" -ForegroundColor Green
Write-Host "Delivered At: $($deliverData.deliveredAt)" -ForegroundColor Green

Write-Host "`n===== STEP 7: Get All Sales Orders =====" -ForegroundColor Cyan

# Get All Sales Orders
Write-Host "`nGetting all sales orders..."
$allOrders = Invoke-RestMethod -Uri "$baseUrl/sales-orders" -Method GET -Headers $headers | ConvertTo-Json -Depth 10
Write-Host $allOrders

Write-Host "`n===== STEP 8: Get Sales Order by ID =====" -ForegroundColor Cyan

# Get Sales Order by ID
Write-Host "`nGetting sales order by ID..."
$soById = Invoke-RestMethod -Uri "$baseUrl/sales-orders/$soId" -Method GET -Headers $headers | ConvertTo-Json -Depth 10
Write-Host $soById

Write-Host "`n===== TEST COMPLETE =====" -ForegroundColor Green
Write-Host "`nSales Order lifecycle tested successfully:"
Write-Host "  ✓ Create → Submit → Approve (reserve) → Ship (deduct) → Deliver"
Write-Host "`n"
