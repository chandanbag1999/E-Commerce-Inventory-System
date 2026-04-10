# 🔐 Authentication & Authorization Module

**Project:** EIVMS  
**Module:** Identity  
**Layer Coverage:** Domain → Application → Infrastructure → API  
**Pattern:** Clean Architecture + RBAC + JWT + Refresh Token  

---

## 1. Why This Module Exists

Har enterprise system ko do sawaalon ka jawab dena hota hai:

- **Authentication** — Kaun hai ye user? (Identity verify karna)
- **Authorization** — Is user ko kya karne ki permission hai? (Access control)

Real-world analogy:
- Login = Office building mein entry gate scan
- Role = Building ke andar konse floor access kar sakte ho
- Permission = Us floor ke specific room ka access

Is module ke bina koi bhi kisi bhi resource ko access kar sakta hai — jo ek e-commerce system mein catastrophic hoga.

---

## 2. Module Responsibilities

### Authentication
- User registration (email + password)
- Login with credential validation
- Password hashing (BCrypt with salt)
- JWT access token generation
- Refresh token generation + rotation
- Logout with token revocation

### Authorization
- Role-based access control (RBAC)
- Permission-based granular access
- Claims injection into JWT
- Policy enforcement via ASP.NET Core middleware

### Session Management
- Sliding refresh token window
- Token family tracking (refresh token rotation)
- Concurrent session detection
- Account lockout after failed attempts

---

## 3. Roles & Permissions Design

### Roles (Predefined)

| Role | Description |
|------|-------------|
| `SuperAdmin` | Full system access - can manage all users and system |
| `Admin` | System administration - user and settings management |
| `InventoryManager` | Warehouse and inventory management |
| `SalesManager` | Sales and order management |
| `Staff` | Operational tasks - inventory and orders (default for new registrations) |

### Permission Strings (Granular)
product:create      product:read      product:update      product:delete
inventory:manage    inventory:view     inventory:receive    inventory:dispatch
order:create        order:read        order:update      order:cancel    order:fulfill
user:manage         user:create      user:view        user:update     user:delete
role:manage
report:generate     report:view
settings:manage     dashboard:view

### Role → Permission Mapping

| Permission | SuperAdmin | Admin | InventoryManager | SalesManager | Staff |
|------------|-----------|-------|------------------|---------------|-------|
| product:create | ✅ | ✅ | ❌ | ✅ | ❌ |
| product:read | ✅ | ✅ | ✅ | ✅ | ✅ |
| product:update | ✅ | ✅ | ❌ | ✅ | ❌ |
| product:delete | ✅ | ✅ | ❌ | ❌ | ❌ |
| inventory:manage | ✅ | ✅ | ✅ | ❌ | ❌ |
| inventory:view | ✅ | ✅ | ✅ | ✅ | ✅ |
| inventory:receive | ✅ | ❌ | ✅ | ❌ | ✅ |
| inventory:dispatch | ✅ | ❌ | ✅ | ❌ | ✅ |
| order:create | ✅ | ✅ | ❌ | ✅ | ❌ |
| order:read | ✅ | ✅ | ✅ | ✅ | ✅ |
| order:update | ✅ | ✅ | ✅ | ✅ | ✅ |
| order:cancel | ✅ | ❌ | ❌ | ✅ | ❌ |
| order:fulfill | ✅ | ❌ | ❌ | ✅ | ❌ |
| user:manage | ✅ | ❌ | ❌ | ❌ | ❌ |
| user:create | ✅ | ✅ | ❌ | ❌ | ❌ |
| user:view | ✅ | ✅ | ❌ | ❌ | ❌ |
| user:update | ✅ | ✅ | ❌ | ❌ | ❌ |
| user:delete | ✅ | ❌ | ❌ | ❌ | ❌ |
| role:manage | ✅ | ❌ | ❌ | ❌ | ❌ |
| report:generate | ✅ | ✅ | ✅ | ✅ | ❌ |
| report:view | ✅ | ✅ | ✅ | ✅ | ❌ |
| settings:manage | ✅ | ✅ | ❌ | ❌ | ❌ |
| dashboard:view | ✅ | ✅ | ✅ | ✅ | ✅ |

> **Enterprise Rule:** Role = collection of permissions. User ko directly permissions assign nahi hoti — sirf Role assign hoti hai. Permissions us Role ke through aati hain.

---

## 4. Architecture — Clean Architecture Alignment
EIVMS.Domain
└── Entities/Identity/
    ├── User.cs
    ├── Role.cs
    ├── Permission.cs
    ├── UserRole.cs
    └── RefreshToken.cs
EIVMS.Application
├── Common/Interfaces/
│   ├── IJwtService.cs
│   └── IPasswordHasher.cs
└── Modules/Identity/
    ├── DTOs/
    │   ├── RegisterRequestDto.cs
    │   ├── LoginRequestDto.cs
    │   ├── AuthResponseDto.cs
    │   ├── RefreshTokenRequestDto.cs
    │   └── CurrentUserDto.cs
    ├── Interfaces/
    │   ├── IAuthService.cs
    │   └── IUserRepository.cs
    ├── Services/
    │   └── AuthService.cs
    └── Validators/
        ├── RegisterRequestValidator.cs
        └── LoginRequestValidator.cs
EIVMS.Infrastructure
├── Persistence/
│   ├── AppDbContext.cs (DbSets for Users, Roles, Permissions, RefreshTokens)
│   └── Configurations/
│       ├── UserConfiguration.cs
│       ├── RoleConfiguration.cs
│       ├── PermissionConfiguration.cs
│       └── RefreshTokenConfiguration.cs
├── Repositories/
│   └── UserRepository.cs
├── Services/
│   ├── JwtService.cs
│   └── PasswordHasher.cs
├── Seeders/
│   └── RolePermissionSeeder.cs
└── DependencyInjection.cs
EIVMS.API
└── Controllers/v1/
    └── AuthController.cs

---

## 5. Domain Layer — Entities

### User.cs
```csharp
public class User : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsEmailVerified { get; private set; } = false;
    public int FailedLoginAttempts { get; private set; } = 0;
    public DateTime? LockedUntil { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    // Domain behaviour methods
    public void IncrementFailedAttempts() { ... }
    public void LockAccount(DateTime until) { ... }
    public void ResetFailedAttempts() { ... }
    public void UpdateLastLogin() { ... }
}
```

### RefreshToken.cs
```csharp
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Token { get; private set; }           // Stored as hash
    public string TokenFamily { get; private set; }     // For rotation tracking
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }

    public User User { get; private set; }
}
```

### Role.cs
```csharp
public class Role : BaseEntity
{
    public string Name { get; private set; }           // "Admin", "Vendor"
    public string NormalizedName { get; private set; } // "ADMIN", "VENDOR"
    public string? Description { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
}
```

### Permission.cs
```csharp
public class Permission : BaseEntity
{
    public string Name { get; private set; }        // "product:create"
    public string Resource { get; private set; }   // "product"
    public string Action { get; private set; }     // "create"
    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
}
```

---

## 6. Application Layer — DTOs

### RegisterRequestDto.cs
```csharp
public class RegisterRequestDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
```

### LoginRequestDto.cs
```csharp
public class LoginRequestDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
```

### AuthResponseDto.cs
```csharp
public class AuthResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
    public DateTime RefreshTokenExpiresAt { get; set; }
    public CurrentUserDto User { get; set; }
}
```

### CurrentUserDto.cs
```csharp
public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public List<string> Permissions { get; set; }
    public string Role { get; set; }           // Primary role (first role)
    public DateTime? LastLoginAt { get; set; } // Last successful login timestamp
}
```

---

## 7. Application Layer — Interface Contracts

### IAuthService.cs
```csharp
public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
    Task<ApiResponse<CurrentUserDto>> GetCurrentUserAsync(Guid userId);
}
```

### IUserRepository.cs
```csharp
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithRolesAsync(Guid userId);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task AddUserRoleAsync(UserRole userRole);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
    Task RevokeTokenFamilyAsync(string tokenFamily);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task<Role?> GetRoleByNameAsync(string roleName);
}
```

> **Note:** `IJwtService` interface `Application/Common/Interfaces/` mein hai — kyunki yeh Application layer ko bhi chahiye.

---

## 8. Infrastructure Layer — JWT Service

### JwtService.cs (Key Logic)
```csharp
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    // Access token: configurable via appsettings (default 15 minutes)
    // Contains: UserId, Email, FullName, Roles, Permissions, JTI, IAT as claims
    public string GenerateAccessToken(User user, List<string> roles, List<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("fullName", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // Roles as claims
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Permissions as claims (for policy-based auth)
        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: GetAccessTokenExpiry(),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    // Refresh token: cryptographically random, stored as SHA-256 hash in DB
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public DateTime GetAccessTokenExpiry()
    {
        var expiryMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15");
        return DateTime.UtcNow.AddMinutes(expiryMinutes);
    }

    public DateTime GetRefreshTokenExpiry()
    {
        var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
        return DateTime.UtcNow.AddDays(expiryDays);
    }
}
```

---

## 9. Infrastructure Layer — Password Hasher

```csharp
public class PasswordHasher : IPasswordHasher
{
    // BCrypt with work factor 12 (enterprise standard)
    // Work factor 12 = ~300ms per hash — too slow for brute force, fine for real users
    public string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool VerifyPassword(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);
}
```

---

## 10. Complete Flow Diagrams

### Register Flow

POST /api/v1/auth/register
↓
FluentValidation (email format, password strength, confirm match)
↓
Check email duplicate → if exists: return 409 Conflict
↓
Hash password (BCrypt work factor 12)
↓
Create User entity
↓
Assign default Role = "Staff" (not "Customer" — Staff is the default for new registrations)
↓
Save to DB (transaction)
↓
Return AuthResponseDto (access + refresh token)


### Login Flow

POST /api/v1/auth/login
↓
Find user by email → if not found: return generic 401
↓
Check account locked → if locked: return 423 with unlock time
↓
Verify password (BCrypt.Verify)
↓
If fail: increment FailedLoginAttempts
→ if >= 5: lock account for 15 minutes
→ return 401 Unauthorized
↓
Reset failed attempts, update LastLoginAt
↓
Fetch user's roles + permissions
↓
Generate JWT access token (15 min expiry)
↓
Generate refresh token → hash → save to DB with IP/UserAgent
↓
Return AuthResponseDto


### Refresh Token Flow

POST /api/v1/auth/refresh
↓
Receive raw refresh token from client
↓
Hash incoming token → look up in DB
↓
Validate: exists? not expired? not revoked?
↓
If token is already used (revoked but has ReplacedByToken = null):
→ TOKEN REUSE DETECTED
→ Revoke entire token family (security breach response)
→ Return 401
↓
Mark current refresh token as revoked (reason: "Rotated")
↓
Generate NEW access token + NEW refresh token
↓
Save new refresh token to DB (same TokenFamily)
↓
Return new AuthResponseDto


### Logout Flow

POST /api/v1/auth/logout
↓
Hash incoming refresh token → find in DB
↓
Mark as revoked (reason: "LoggedOut")
↓
Optional: revoke all tokens in same family (logout all devices)
↓
Return 200 OK


---

## 11. API Layer — AuthController

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        dto.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        dto.UserAgent = Request.Headers["User-Agent"].ToString();

        var result = await _authService.LoginAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        var result = await _authService.LogoutAsync(refreshToken);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.GetCurrentUserAsync(userId);
        return Ok(result);
    }
}
```

---

## 12. Authorization — Policy-Based Setup

### Program.cs mein (API Layer)
```csharp
builder.Services.AddAuthorization(options =>
{
    // Permission-based policies
    options.AddPolicy("product:create", policy =>
        policy.RequireClaim("permission", "product:create"));

    options.AddPolicy("inventory:manage", policy =>
        policy.RequireClaim("permission", "inventory:manage"));

    // ya dynamic policy provider use karo (advanced)
});
```

### Controller Usage
```csharp
[HttpPost]
[Authorize(Policy = "product:create")]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
{
    // sirf wahi users access kar sakte hain jinke JWT mein "product:create" permission claim hai
}
```

---

## 13. Database Tables (EF Core)

### AppDbContext.cs DbSets
```csharp
public DbSet<User> Users => Set<User>();
public DbSet<Role> Roles => Set<Role>();
public DbSet<Permission> Permissions => Set<Permission>();
public DbSet<UserRole> UserRoles => Set<UserRole>();
public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
```

### Key Fluent Configurations

**UserConfiguration.cs**
```csharp
builder.HasIndex(u => u.Email).IsUnique();
builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
builder.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
```

**RefreshTokenConfiguration.cs**
```csharp
builder.HasIndex(r => r.Token);           // fast lookup
builder.HasIndex(r => r.TokenFamily);     // family revocation
builder.Property(r => r.Token).HasMaxLength(512).IsRequired();
builder.HasOne(r => r.User)
       .WithMany(u => u.RefreshTokens)
       .HasForeignKey(r => r.UserId)
       .OnDelete(DeleteBehavior.Cascade);
```

---

## 14. Seeder — Roles & Permissions

### RolePermissionSeeder.cs (Infrastructure/Seeders/)
```csharp
public static class RolePermissionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Roles.AnyAsync()) return;

        // Define all permissions
        var permissions = new Dictionary<string, Permission>
        {
            ["product:create"] = Permission.Create("product:create", "product", "create", "Create new products"),
            ["product:read"]   = Permission.Create("product:read",   "product", "read",   "View products"),
            ["product:update"] = Permission.Create("product:update", "product", "update", "Update products"),
            ["product:delete"] = Permission.Create("product:delete", "product", "delete", "Delete products"),
            ["inventory:manage"] = Permission.Create("inventory:manage", "inventory", "manage", "Full inventory management"),
            ["inventory:view"]   = Permission.Create("inventory:view",   "inventory", "view",   "View inventory"),
            ["inventory:receive"] = Permission.Create("inventory:receive", "inventory", "receive", "Receive stock"),
            ["inventory:dispatch"] = Permission.Create("inventory:dispatch", "inventory", "dispatch", "Dispatch stock"),
            ["order:create"] = Permission.Create("order:create", "order", "create", "Create orders"),
            ["order:read"]   = Permission.Create("order:read",   "order", "read",   "View orders"),
            ["order:update"] = Permission.Create("order:update", "order", "update", "Update orders"),
            ["order:cancel"] = Permission.Create("order:cancel", "order", "cancel", "Cancel orders"),
            ["order:fulfill"] = Permission.Create("order:fulfill", "order", "fulfill", "Fulfill orders"),
            ["user:manage"] = Permission.Create("user:manage", "user", "manage", "Manage users"),
            ["user:create"] = Permission.Create("user:create", "user", "create", "Create users"),
            ["user:view"]   = Permission.Create("user:view",   "user", "view",   "View users"),
            ["user:update"] = Permission.Create("user:update", "user", "update", "Update users"),
            ["user:delete"] = Permission.Create("user:delete", "user", "delete", "Delete users"),
            ["role:manage"] = Permission.Create("role:manage", "role", "manage", "Manage roles"),
            ["report:generate"] = Permission.Create("report:generate", "report", "generate", "Generate reports"),
            ["report:view"]     = Permission.Create("report:view",     "report", "view",     "View reports"),
            ["settings:manage"] = Permission.Create("settings:manage", "settings", "manage", "Manage system settings"),
            ["dashboard:view"] = Permission.Create("dashboard:view", "dashboard", "view", "View dashboard"),
        };

        await context.Permissions.AddRangeAsync(permissions.Values);

        // Create roles with specific permissions
        var superAdminRole = Role.Create("SuperAdmin", "Full system access - can manage all users and system");
        var superAdminPermissions = permissions.Keys.ToList(); // All permissions

        var adminRole = Role.Create("Admin", "System administration - user and settings management");
        var adminPermissions = new[] { "product:create", "product:read", "product:update", "product:delete",
            "inventory:manage", "inventory:view", "inventory:receive", "inventory:dispatch",
            "order:create", "order:read", "order:update", "order:cancel", "order:fulfill",
            "user:create", "user:view", "user:update", "report:generate", "report:view",
            "dashboard:view", "settings:manage" };

        var inventoryManagerRole = Role.Create("InventoryManager", "Warehouse and inventory management");
        var inventoryManagerPermissions = new[] { "product:read", "inventory:manage", "inventory:view",
            "inventory:receive", "inventory:dispatch", "order:read", "order:update", "report:generate", "report:view", "dashboard:view" };

        var salesManagerRole = Role.Create("SalesManager", "Sales and order management");
        var salesManagerPermissions = new[] { "product:create", "product:read", "product:update", "inventory:view",
            "order:create", "order:read", "order:update", "order:cancel", "order:fulfill", "report:generate", "report:view", "dashboard:view" };

        var staffRole = Role.Create("Staff", "Operational tasks - inventory and orders");
        var staffPermissions = new[] { "product:read", "inventory:view", "inventory:receive", "inventory:dispatch",
            "order:read", "order:update", "dashboard:view" };

        await context.Roles.AddRangeAsync(superAdminRole, adminRole, inventoryManagerRole, salesManagerRole, staffRole);
        await context.SaveChangesAsync();

        // Create RolePermission mappings
        var rolePermissions = new List<RolePermission>();
        foreach (var permKey in superAdminPermissions)
            rolePermissions.Add(RolePermission.Create(superAdminRole.Id, permissions[permKey].Id));

        foreach (var permKey in adminPermissions)
            rolePermissions.Add(RolePermission.Create(adminRole.Id, permissions[permKey].Id));

        foreach (var permKey in inventoryManagerPermissions)
            rolePermissions.Add(RolePermission.Create(inventoryManagerRole.Id, permissions[permKey].Id));

        foreach (var permKey in salesManagerPermissions)
            rolePermissions.Add(RolePermission.Create(salesManagerRole.Id, permissions[permKey].Id));

        foreach (var permKey in staffPermissions)
            rolePermissions.Add(RolePermission.Create(staffRole.Id, permissions[permKey].Id));

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }
}
```

---

## 15. appsettings.json — JWT Configuration

```json
{
  "Jwt": {
    "SecretKey": "your-very-long-secret-key-minimum-32-chars-enterprise",
    "Issuer": "EcommerceInventorySystem",
    "Audience": "EcommerceInventorySystemClient",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

> **Configuration Keys:** `AccessTokenExpiryMinutes` and `RefreshTokenExpiryDays` are configurable via appsettings — no hardcoding in service.

---

## 16. FluentValidation — Input Validation

### RegisterRequestValidator.cs
```csharp
public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("First name can only contain letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one digit")
            .Matches(@"[!@#$%^&*(),.?""':{}|<>]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}
```

### LoginRequestValidator.cs
```csharp
public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
```

> **Production Rule:** SecretKey kabhi bhi appsettings mein hardcode mat karo. Environment variable ya Azure Key Vault / AWS Secrets Manager use karo.

---

## 17. Security Best Practices Implemented

| Concern | Implementation |
|---------|---------------|
| Password storage | BCrypt with work factor 12 |
| Token storage | Refresh token SHA-256 hashed in DB |
| Token rotation | Every refresh generates new token, old revoked |
| Reuse detection | Revoked token reuse = entire family revoked |
| Brute force | 5 attempts → 15-minute account lock |
| Claims | UserId, Email, FullName, Roles, Permissions, JTI, IAT in JWT |
| Audit trail | IP + UserAgent stored with each refresh token |
| Expiry | Configurable via appsettings (Access: 15 min, Refresh: 7 days) |
| Concurrent sessions | Multiple refresh tokens per user supported |
| Validation | FluentValidation for Register and Login DTOs |

---

## 18. MVP vs Enterprise Scope

### ✅ MVP (Current Implementation)
- JWT access token with configurable expiry
- Refresh token with rotation
- Token reuse detection (entire family revoked)
- BCrypt password hashing with work factor 12
- Role-based access (SuperAdmin, Admin, InventoryManager, SalesManager, Staff)
- Permission claims in JWT
- FluentValidation (Register/Login)
- Account lockout after 5 failed attempts
- Default role for new registrations: "Staff"

### 🔮 Enterprise Extensions (Future Scope)
- Email verification on register
- Password reset via email OTP
- OAuth2 (Google/GitHub login)
- Multi-Factor Authentication (TOTP)
- Redis-based token blacklist (for instant revocation)
- SSO (Single Sign-On)
- Device tracking + suspicious activity alerts

---

## 18. Dependency Injection Registration

### Infrastructure/DependencyInjection.cs
```csharp
// Identity module registrations
services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddSingleton<IJwtService, JwtService>();
services.AddSingleton<IPasswordHasher, PasswordHasher>();

// JWT Bearer authentication
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!)),
            ClockSkew = TimeSpan.Zero  // Strict expiry, no buffer
        };
    });
```

---

## 19. Unit Test Structure
tests/EIVMS.UnitTests/
└── Modules/
    └── Identity/
        ├── AuthServiceTests.cs        (Register, Login, Refresh, Logout)
        ├── PasswordHasherTests.cs     (Hash + verify)
        └── UserRepositoryTests.cs     (Email lookup, duplicate check)

---

## 20. Industry Standard Notes

1. **Access Token is stateless** — server ke paas koi record nahi, sirf signature validate hoti hai. Isliye short-lived rakhte hain (15 min).

2. **Refresh Token is stateful** — DB mein stored hoti hai, isliye instant revoke possible hai.

3. **Token Family concept** — ek login session = ek family. Agar us family ka koi bhi revoked token dobara use kiya gaya, poori family revoke ho jaati hai. Yeh token theft ka response hai.

4. **Permissions in JWT** — Enterprise mein permissions sirf DB se check karna slow hota hai. JWT mein embed karke stateless authorization possible hai. Trade-off: 15-min window mein permission change reflect nahi hogi — acceptable for most systems.

5. **Never return "email not found" vs "wrong password" separately** — dono ke liye same generic `401 Unauthorized` return karo. Attackers ko valid emails enumerate karne ka moka nahi dena chahiye.