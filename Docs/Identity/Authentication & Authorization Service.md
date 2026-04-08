# 🔐 Authentication & Authorization — Enterprise Implementation Guide
### `InventoryManagement` Clean Architecture · ASP.NET Core 8 · Neon DB (PostgreSQL) · All Free Packages

---

## 📌 TABLE OF CONTENTS

1. [Architecture Overview — Auth Fit](#1-architecture-overview)
2. [Why This Design? Decision Rationale](#2-why-this-design)
3. [NuGet Packages — All Free](#3-nuget-packages)
4. [Domain Layer — Entities & Interfaces](#4-domain-layer)
5. [Application Layer — Commands, Queries, DTOs, Validators](#5-application-layer)
6. [Infrastructure Layer — Services & EF Configurations](#6-infrastructure-layer)
7. [API Layer — Controller, Middleware, DI Extensions](#7-api-layer)
8. [Complete Flow Walkthroughs](#8-complete-flows)
9. [Database Schema (Neon PostgreSQL)](#9-database-schema)
10. [Security Hardening Checklist](#10-security-hardening)
11. [Error Handling Strategy](#11-error-handling)
12. [Logging with Serilog](#12-logging)
13. [DI Registration — Clean Program.cs](#13-di-registration)
14. [Step-by-Step Implementation Order](#14-implementation-order)
15. [Future Enterprise Extensions](#15-future-scope)
16. [Final File Map](#16-final-file-map)

---

## 1. ARCHITECTURE OVERVIEW

### Where Auth lives in your existing project

```
InventoryManagement/
│
├── InventoryManagement.Domain/
│   ├── Entities/
│   │   ├── User.cs                        ← NEW
│   │   ├── Role.cs                        ← NEW
│   │   ├── Permission.cs                  ← NEW
│   │   ├── UserRole.cs                    ← NEW
│   │   ├── RolePermission.cs              ← NEW
│   │   └── RefreshToken.cs                ← NEW (critical)
│   ├── Enums/
│   │   └── DefaultRoles.cs                ← NEW
│   ├── Exceptions/
│   │   ├── UnauthorizedException.cs       ← NEW
│   │   └── AccountLockedException.cs      ← NEW
│   └── Interfaces/
│       ├── IUserRepository.cs             ← NEW
│       ├── IRefreshTokenRepository.cs     ← NEW
│       ├── ITokenService.cs               ← NEW
│       └── IPasswordService.cs            ← NEW
│
├── InventoryManagement.Application/
│   ├── Features/
│   │   └── Auth/                          ← NEW FEATURE FOLDER
│   │       ├── Commands/
│   │       │   ├── RegisterUserCommand.cs ← NEW
│   │       │   ├── LoginUserCommand.cs    ← NEW
│   │       │   ├── RefreshTokenCommand.cs ← NEW
│   │       │   └── LogoutCommand.cs       ← NEW
│   │       └── Queries/
│   │           └── GetCurrentUserQuery.cs ← NEW
│   ├── DTOs/
│   │   ├── RegisterDto.cs                 ← NEW
│   │   ├── LoginDto.cs                    ← NEW
│   │   ├── AuthResponseDto.cs             ← NEW
│   │   └── CurrentUserDto.cs              ← NEW
│   ├── Interfaces/
│   │   └── IAuthService.cs                ← NEW (already exists as placeholder)
│   ├── Validators/
│   │   ├── RegisterDtoValidator.cs        ← NEW
│   │   └── LoginDtoValidator.cs           ← NEW
│   └── Mappings/
│       └── AuthMappingProfile.cs          ← NEW
│
├── InventoryManagement.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs                ← MODIFY (add DbSets)
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs       ← NEW
│   │   │   ├── RoleConfiguration.cs       ← NEW
│   │   │   ├── RefreshTokenConfiguration.cs ← NEW
│   │   │   └── DataSeedConfiguration.cs   ← NEW (seed roles/permissions)
│   │   └── Repositories/
│   │       ├── UserRepository.cs          ← NEW
│   │       └── RefreshTokenRepository.cs  ← NEW
│   └── Services/
│       ├── AuthService.cs                 ← NEW
│       ├── TokenService.cs                ← NEW
│       └── PasswordService.cs             ← NEW
│
└── InventoryManagement.API/
    ├── Controllers/
    │   └── AuthController.cs              ← NEW
    ├── Middleware/
    │   └── GlobalExceptionHandler.cs      ← MODIFY (add auth exceptions)
    └── Extensions/
        └── AuthServiceExtensions.cs       ← NEW (JWT + policy DI)
```

---

## 2. WHY THIS DESIGN?

### 2.1 Why NOT ASP.NET Core Identity?

| Reason | Explanation |
|---|---|
| **Overhead** | Identity brings 15+ tables, UserManager, RoleManager — overkill for your scope |
| **Flexibility** | Custom User entity = full control over fields, lockout logic, permissions |
| **Learning value** | Building from scratch = you understand every line in interviews |
| **Portfolio signal** | Interviewers see custom auth = senior understanding |

> ✅ **Decision: Custom JWT auth with manual RBAC** — correct for enterprise portfolio project.

---

### 2.2 Why JWT + Refresh Token (not just JWT)?

```
Problem with JWT-only:
  Long expiry (1 day) = if stolen, attacker has 24 hours of access.
  Short expiry (15 min) = user must re-login every 15 min. Bad UX.

Solution: JWT (15 min) + Refresh Token (7 days)
  → Access token stolen? Useless in 15 min.
  → User stays logged in via silent refresh.
  → Refresh token stolen? Token rotation detects reuse → revoke all sessions.
```

---

### 2.3 Why separate `ITokenService` and `IAuthService`?

- `IAuthService` → Business logic (register, login, logout)
- `ITokenService` → Pure token mechanics (generate, validate, parse)
- **Separation of Concerns** → Tomorrow if you change JWT to OAuth, only `TokenService` changes

---

### 2.4 Why store Refresh Tokens in DB (not in-memory)?

- DB = token survives server restart
- DB = can revoke specific token (logout one device)
- DB = audit trail (who logged in from where)
- In-memory = lost on restart, can't revoke

---

### 2.5 Why BCrypt with work factor 12?

```
MD5/SHA1 → cracked in milliseconds (GPU attacks)
BCrypt work factor 12 → ~300ms per hash

300ms for legitimate user = unnoticeable
300ms per attempt for attacker with 1000 GPUs = still very slow
```

---

### 2.6 Why Permission claims IN the JWT (not just roles)?

```
Role-only approach:
  [Authorize(Roles = "Admin")] → works but too coarse
  Admin can do EVERYTHING = security risk

Permission approach:
  JWT contains: permissions: ["product:create", "inventory:manage"]
  [Authorize(Policy = "CanCreateProduct")] → fine-grained
  → Role changes don't break existing code
  → Permissions are self-documenting
```

---

## 3. NUGET PACKAGES (All Free / Open Source)

### 3.1 Domain Project — No packages needed
Domain has zero dependencies by design.

### 3.2 Application Project
```xml
<!-- FluentValidation — input validation rules -->
<PackageReference Include="FluentValidation" Version="11.11.0" />

<!-- MediatR — CQRS command/query dispatcher (already in project) -->
<PackageReference Include="MediatR" Version="12.4.1" />

<!-- AutoMapper — DTO mapping (already in project) -->
<PackageReference Include="AutoMapper" Version="13.0.1" />
```

### 3.3 Infrastructure Project
```xml
<!-- JWT Bearer token generation and validation -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.16" />

<!-- JWT token parsing/creation -->
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.12.1" />

<!-- BCrypt password hashing — FREE, industry standard -->
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />

<!-- EF Core PostgreSQL — already in project -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.11" />
```

### 3.4 API Project
```xml
<!-- FluentValidation ASP.NET Core integration (auto model validation) -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />

<!-- Serilog — structured logging — FREE -->
<PackageReference Include="Serilog.AspNetCore" Version="8.0.3" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />

<!-- Swagger — already in project -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
```

> 💡 **All packages above are 100% free and open source. Zero cost.**

---

## 4. DOMAIN LAYER

### 📁 `InventoryManagement.Domain/Entities/`

#### `User.cs`
```csharp
namespace InventoryManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Account control
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; } = false; // future: email verification
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }          // null = not locked

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Computed helper — no DB column
    public string FullName => $"{FirstName} {LastName}";
    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
}
```

---

#### `Role.cs`
```csharp
namespace InventoryManagement.Domain.Entities;

public class Role
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;         // "Admin", "Vendor"
    public string Description { get; set; } = string.Empty;
    public bool IsSystem { get; set; } = false;               // system roles can't be deleted

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

---

#### `Permission.cs`
```csharp
namespace InventoryManagement.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;        // "product:create"
    public string Resource { get; set; } = string.Empty;    // "product"
    public string Action { get; set; } = string.Empty;      // "create"
    public string Description { get; set; } = string.Empty;

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
```

---

#### `UserRole.cs` (Junction)
```csharp
namespace InventoryManagement.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
```

---

#### `RolePermission.cs` (Junction)
```csharp
namespace InventoryManagement.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
```

---

#### `RefreshToken.cs` ← Most important entity
```csharp
namespace InventoryManagement.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }

    // The actual token value — crypto-random, NOT a JWT
    public string Token { get; set; } = string.Empty;

    // Links this refresh token to a specific access token's 'jti' claim
    // Useful for tracking which session this belongs to
    public string JwtId { get; set; } = string.Empty;

    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByIp { get; set; } = string.Empty;

    // Revocation audit trail
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }   // token that replaced this one (rotation)
    public string? ReasonRevoked { get; set; }      // "Logout" | "Rotation" | "Suspicious"

    // Computed — no DB columns needed
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation
    public User User { get; set; } = null!;
}
```

---

### 📁 `InventoryManagement.Domain/Enums/`

#### `DefaultRoles.cs`
```csharp
namespace InventoryManagement.Domain.Enums;

// String constants — not an enum — so they can be used in attributes
public static class DefaultRoles
{
    public const string Admin    = "Admin";
    public const string Vendor   = "Vendor";
    public const string Staff    = "Staff";
    public const string Customer = "Customer";
}

public static class Permissions
{
    // Products
    public const string ProductCreate = "product:create";
    public const string ProductRead   = "product:read";
    public const string ProductUpdate = "product:update";
    public const string ProductDelete = "product:delete";

    // Inventory
    public const string InventoryManage = "inventory:manage";
    public const string InventoryView   = "inventory:view";

    // Orders
    public const string OrderCreate = "order:create";
    public const string OrderRead   = "order:read";
    public const string OrderUpdate = "order:update";
    public const string OrderCancel = "order:cancel";

    // Users (admin only)
    public const string UserManage = "user:manage";

    // Reports
    public const string ReportView = "report:view";
}
```

---

### 📁 `InventoryManagement.Domain/Exceptions/`

#### `UnauthorizedException.cs`
```csharp
namespace InventoryManagement.Domain.Exceptions;

// Thrown when credentials invalid, token invalid, or token expired
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
```

#### `AccountLockedException.cs`
```csharp
namespace InventoryManagement.Domain.Exceptions;

public class AccountLockedException : Exception
{
    public DateTime LockoutEnd { get; }
    public AccountLockedException(DateTime lockoutEnd)
        : base($"Account is locked until {lockoutEnd:HH:mm:ss} UTC.")
    {
        LockoutEnd = lockoutEnd;
    }
}
```

---

### 📁 `InventoryManagement.Domain/Interfaces/`

#### `IUserRepository.cs`
```csharp
namespace InventoryManagement.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    // Include roles + permissions (for token generation)
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}
```

#### `IRefreshTokenRepository.cs`
```csharp
namespace InventoryManagement.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default);

    // Revoke all active tokens for user (password change, admin action)
    Task RevokeAllActiveTokensAsync(Guid userId, string reason, CancellationToken ct = default);

    // Cleanup — call via background job
    Task DeleteExpiredTokensAsync(CancellationToken ct = default);
}
```

#### `ITokenService.cs`
```csharp
namespace InventoryManagement.Domain.Interfaces;

public interface ITokenService
{
    // Generates signed JWT access token with user's claims
    string GenerateAccessToken(User user, IList<string> roles, IList<string> permissions);

    // Generates cryptographically random refresh token
    RefreshToken GenerateRefreshToken(Guid userId, string ipAddress);

    // Extracts claims from EXPIRED token (used in /refresh-token endpoint)
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
}
```

#### `IPasswordService.cs`
```csharp
namespace InventoryManagement.Domain.Interfaces;

public interface IPasswordService
{
    string HashPassword(string plainTextPassword);
    bool VerifyPassword(string plainTextPassword, string hashedPassword);
}
```

---

## 5. APPLICATION LAYER

### 📁 `InventoryManagement.Application/DTOs/`

#### `RegisterDto.cs`
```csharp
namespace InventoryManagement.Application.DTOs;

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

#### `LoginDto.cs`
```csharp
namespace InventoryManagement.Application.DTOs;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

#### `AuthResponseDto.cs`
```csharp
namespace InventoryManagement.Application.DTOs;

// This is what the API returns on login/register/refresh
public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; }
    public CurrentUserDto User { get; set; } = null!;
}
```

#### `CurrentUserDto.cs`
```csharp
namespace InventoryManagement.Application.DTOs;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
}
```

---

### 📁 `InventoryManagement.Application/Interfaces/`

#### `IAuthService.cs`
```csharp
namespace InventoryManagement.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
```

---

### 📁 `InventoryManagement.Application/Validators/`

#### `RegisterDtoValidator.cs`
```csharp
namespace InventoryManagement.Application.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please provide a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
```

#### `LoginDtoValidator.cs`
```csharp
namespace InventoryManagement.Application.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please provide a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
```

---

### 📁 `InventoryManagement.Application/Features/Auth/Commands/`

#### `LoginUserCommand.cs`
```csharp
namespace InventoryManagement.Application.Features.Auth.Commands;

// Command: carries input data
public record LoginUserCommand(LoginDto Dto, string IpAddress) : IRequest<AuthResponseDto>;

// Handler: calls IAuthService
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public LoginUserCommandHandler(IAuthService authService)
        => _authService = authService;

    public async Task<AuthResponseDto> Handle(LoginUserCommand request, CancellationToken ct)
        => await _authService.LoginAsync(request.Dto, request.IpAddress, ct);
}
```

#### `RegisterUserCommand.cs`
```csharp
namespace InventoryManagement.Application.Features.Auth.Commands;

public record RegisterUserCommand(RegisterDto Dto) : IRequest<AuthResponseDto>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public RegisterUserCommandHandler(IAuthService authService)
        => _authService = authService;

    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken ct)
        => await _authService.RegisterAsync(request.Dto, ct);
}
```

#### `RefreshTokenCommand.cs`
```csharp
namespace InventoryManagement.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken, string IpAddress) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
        => _authService = authService;

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken ct)
        => await _authService.RefreshTokenAsync(request.RefreshToken, request.IpAddress, ct);
}
```

#### `LogoutCommand.cs`
```csharp
namespace InventoryManagement.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken, string IpAddress) : IRequest;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
        => _authService = authService;

    public async Task Handle(LogoutCommand request, CancellationToken ct)
        => await _authService.LogoutAsync(request.RefreshToken, request.IpAddress, ct);
}
```

---

### 📁 `InventoryManagement.Application/Features/Auth/Queries/`

#### `GetCurrentUserQuery.cs`
```csharp
namespace InventoryManagement.Application.Features.Auth.Queries;

public record GetCurrentUserQuery(Guid UserId) : IRequest<CurrentUserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IAuthService _authService;

    public GetCurrentUserQueryHandler(IAuthService authService)
        => _authService = authService;

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken ct)
        => await _authService.GetCurrentUserAsync(request.UserId, ct);
}
```

---

### 📁 `InventoryManagement.Application/Mappings/`

#### `AuthMappingProfile.cs`
```csharp
namespace InventoryManagement.Application.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, CurrentUserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.UserRoles.Select(ur => ur.Role.Name).ToList()))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src =>
                src.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToList()));
    }
}
```

---

## 6. INFRASTRUCTURE LAYER

### 📁 `InventoryManagement.Infrastructure/Services/`

#### `PasswordService.cs`
```csharp
namespace InventoryManagement.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    // Work factor 12 = ~300ms per hash = good balance of security vs speed
    // Work factor 10 = ~100ms (acceptable for MVP, increase to 12 for prod)
    private const int WorkFactor = 12;

    public string HashPassword(string plainTextPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);

    public bool VerifyPassword(string plainTextPassword, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(plainTextPassword, hashedPassword);
}
```

---

#### `TokenService.cs`
```csharp
namespace InventoryManagement.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtOptions)
        => _jwtSettings = jwtOptions.Value;

    public string GenerateAccessToken(User user, IList<string> roles, IList<string> permissions)
    {
        var jwtId = Guid.NewGuid().ToString(); // unique ID for this token

        var claims = new List<Claim>
        {
            // Standard claims (industry standard names)
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, jwtId),             // token unique ID
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        // Add roles
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Add fine-grained permissions
        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        // HS256 = fine for monolith. Upgrade to RS256 when moving to microservices.
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(Guid userId, string ipAddress)
    {
        // 64 crypto-random bytes → Base64 string (NOT a JWT)
        // Why not JWT? Refresh tokens should be opaque — no info should leak
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
    {
        // Used only in /refresh-token endpoint to read userId from EXPIRED token
        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,         // ← Explicitly ignore expiry here
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret))
        };

        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(
                accessToken, tokenValidationParams, out var validatedToken);

            // Ensure it's actually a JWT with correct algorithm
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
```

---

#### `AuthService.cs` ← Core business logic
```csharp
namespace InventoryManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthService> logger)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
        _jwtSettings = jwtOptions.Value;
        _logger = logger;
    }

    // ─────────────────────────────────────────
    // REGISTER
    // ─────────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, CancellationToken ct = default)
    {
        // 1. Check duplicate email
        if (await _userRepo.EmailExistsAsync(dto.Email, ct))
            throw new InvalidOperationException("An account with this email already exists.");

        // 2. Create user with hashed password
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = _passwordService.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user, ct);

        // Note: Role assignment happens via seed data or admin panel
        // For registration, no role is assigned by default (or assign "Customer")
        // This is a business decision — your team decides

        _logger.LogInformation("New user registered: {Email}", user.Email);

        // For registration, return minimal response (no token yet — require login)
        // OR generate tokens immediately — depends on your UX requirement
        // This impl generates tokens immediately (common pattern)
        return await GenerateAuthResponse(user, ct);
    }

    // ─────────────────────────────────────────
    // LOGIN
    // ─────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, CancellationToken ct = default)
    {
        // 1. Find user with roles and permissions eagerly loaded
        var user = await _userRepo.GetByEmailWithRolesAsync(dto.Email.ToLower().Trim(), ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        // 2. Check if account is active
        if (!user.IsActive)
            throw new UnauthorizedException("This account has been deactivated.");

        // 3. Check lockout
        if (user.IsLockedOut)
            throw new AccountLockedException(user.LockoutEnd!.Value);

        // 4. Verify password
        if (!_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            // Lock after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                _logger.LogWarning("Account locked due to failed attempts: {Email}", user.Email);
            }

            await _userRepo.UpdateAsync(user, ct);

            // SECURITY: Do NOT say "password wrong" or "email not found"
            // Always use same message to prevent user enumeration
            throw new UnauthorizedException("Invalid email or password.");
        }

        // 5. Reset failed attempts on success
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user, ct);

        _logger.LogInformation("User logged in: {Email} from IP: {Ip}", user.Email, ipAddress);

        return await GenerateAuthResponseWithIp(user, ipAddress, ct);
    }

    // ─────────────────────────────────────────
    // REFRESH TOKEN (with Rotation + Theft Detection)
    // ─────────────────────────────────────────
    public async Task<AuthResponseDto> RefreshTokenAsync(
        string token, string ipAddress, CancellationToken ct = default)
    {
        var storedToken = await _refreshTokenRepo.GetByTokenAsync(token, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        // ⚠️ REUSE DETECTION — most important security check
        if (storedToken.IsRevoked)
        {
            // Token was already used/revoked = potential theft
            // Revoke all tokens in this user's active sessions
            await _refreshTokenRepo.RevokeAllActiveTokensAsync(
                storedToken.UserId,
                "Potential token theft — reuse of revoked token detected",
                ct);

            _logger.LogWarning(
                "SECURITY ALERT: Revoked refresh token reuse detected for UserId={UserId} from IP={Ip}",
                storedToken.UserId, ipAddress);

            throw new UnauthorizedException(
                "Security alert: Suspicious activity detected. Please log in again.");
        }

        // Check expiry
        if (storedToken.IsExpired)
            throw new UnauthorizedException("Refresh token has expired. Please log in again.");

        // ── TOKEN ROTATION ──
        // 1. Revoke old token
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReasonRevoked = "Rotation";

        // 2. Create new refresh token
        var newRefreshToken = _tokenService.GenerateRefreshToken(storedToken.UserId, ipAddress);
        storedToken.ReplacedByToken = newRefreshToken.Token; // audit chain

        await _refreshTokenRepo.UpdateAsync(storedToken, ct);
        await _refreshTokenRepo.AddAsync(newRefreshToken, ct);

        // 3. Load user with updated roles
        var user = await _userRepo.GetByEmailWithRolesAsync(storedToken.User.Email, ct)
            ?? throw new UnauthorizedException("User not found.");

        var roles = GetRoles(user);
        var permissions = GetPermissions(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles, permissions);

        _logger.LogInformation("Refresh token rotated for UserId={UserId}", user.Id);

        return BuildAuthResponse(newAccessToken, newRefreshToken.Token, user, roles, permissions);
    }

    // ─────────────────────────────────────────
    // LOGOUT
    // ─────────────────────────────────────────
    public async Task LogoutAsync(string token, string ipAddress, CancellationToken ct = default)
    {
        var storedToken = await _refreshTokenRepo.GetByTokenAsync(token, ct);

        // Silently ignore if token not found or already revoked
        if (storedToken == null || !storedToken.IsActive) return;

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = ipAddress;
        storedToken.ReasonRevoked = "Logout";

        await _refreshTokenRepo.UpdateAsync(storedToken, ct);

        _logger.LogInformation("User logged out: UserId={UserId}", storedToken.UserId);
    }

    // ─────────────────────────────────────────
    // GET CURRENT USER
    // ─────────────────────────────────────────
    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new UnauthorizedException("User not found.");

        return _mapper.Map<CurrentUserDto>(user);
    }

    // ─────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────
    private static IList<string> GetRoles(User user)
        => user.UserRoles.Select(ur => ur.Role.Name).ToList();

    private static IList<string> GetPermissions(User user)
        => user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();

    private async Task<AuthResponseDto> GenerateAuthResponse(User user, CancellationToken ct)
    {
        var roles = GetRoles(user);
        var permissions = GetPermissions(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, "unknown");
        await _refreshTokenRepo.AddAsync(refreshToken, ct);
        return BuildAuthResponse(accessToken, refreshToken.Token, user, roles, permissions);
    }

    private async Task<AuthResponseDto> GenerateAuthResponseWithIp(
        User user, string ip, CancellationToken ct)
    {
        var roles = GetRoles(user);
        var permissions = GetPermissions(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id, ip);
        await _refreshTokenRepo.AddAsync(refreshToken, ct);
        return BuildAuthResponse(accessToken, refreshToken.Token, user, roles, permissions);
    }

    private AuthResponseDto BuildAuthResponse(
        string accessToken, string refreshToken,
        User user, IList<string> roles, IList<string> permissions)
    {
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            User = new CurrentUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Roles = roles,
                Permissions = permissions
            }
        };
    }
}
```

---

### 📁 `InventoryManagement.Infrastructure/Persistence/`

#### `AppDbContext.cs` — Add DbSets
```csharp
// Add these to your existing AppDbContext:

public DbSet<User> Users => Set<User>();
public DbSet<Role> Roles => Set<Role>();
public DbSet<Permission> Permissions => Set<Permission>();
public DbSet<UserRole> UserRoles => Set<UserRole>();
public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

---

### 📁 `InventoryManagement.Infrastructure/Persistence/Configurations/`

#### `UserConfiguration.cs`
```csharp
namespace InventoryManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.PasswordHash).IsRequired();

        // Ignore computed property — no DB column
        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.IsLockedOut);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### `RefreshTokenConfiguration.cs`
```csharp
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.CreatedByIp).HasMaxLength(50);
        builder.Property(rt => rt.RevokedByIp).HasMaxLength(50);
        builder.Property(rt => rt.ReasonRevoked).HasMaxLength(200);
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(500);

        // Ignore computed properties
        builder.Ignore(rt => rt.IsActive);
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsRevoked);
    }
}
```

#### `DataSeedConfiguration.cs` ← Seeds roles + permissions
```csharp
public class DataSeedConfiguration : IEntityTypeConfiguration<Role>
{
    // Seed roles
    private static readonly Guid AdminRoleId  = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid VendorRoleId = Guid.Parse("11111111-0000-0000-0000-000000000002");
    private static readonly Guid StaffRoleId  = Guid.Parse("11111111-0000-0000-0000-000000000003");
    private static readonly Guid CustomerRoleId = Guid.Parse("11111111-0000-0000-0000-000000000004");

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Name).IsUnique();

        builder.HasData(
            new Role { Id = AdminRoleId,    Name = "Admin",    Description = "Full system access", IsSystem = true },
            new Role { Id = VendorRoleId,   Name = "Vendor",   Description = "Manage products and inventory", IsSystem = true },
            new Role { Id = StaffRoleId,    Name = "Staff",    Description = "View and process orders", IsSystem = true },
            new Role { Id = CustomerRoleId, Name = "Customer", Description = "Browse and order products", IsSystem = true }
        );
    }
}
```

---

### 📁 `InventoryManagement.Infrastructure/Persistence/Repositories/`

#### `UserRepository.cs`
```csharp
namespace InventoryManagement.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken ct = default)
        => await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await _context.Users.AnyAsync(u => u.Email == email.ToLower(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }
}
```

#### `RefreshTokenRepository.cs`
```csharp
namespace InventoryManagement.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context) => _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(ct);
    }

    public async Task RevokeAllActiveTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReasonRevoked = reason;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteExpiredTokensAsync(CancellationToken ct = default)
    {
        // Delete tokens expired more than 2 days ago
        var cutoff = DateTime.UtcNow.AddDays(-2);
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate < cutoff)
            .ToListAsync(ct);

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(ct);
    }
}
```

---

## 7. API LAYER

### 📁 `InventoryManagement.API/Controllers/`

#### `AuthController.cs`
```csharp
namespace InventoryManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    // Helper to get client IP
    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    /// <summary>Register a new user account</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterUserCommand(dto), ct);
        return CreatedAtAction(nameof(GetCurrentUser), result);
    }

    /// <summary>Login with email and password</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginUserCommand(dto, GetIpAddress()), ct);
        return Ok(result);
    }

    /// <summary>Get a new access token using refresh token</summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestDto request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(request.RefreshToken, GetIpAddress()), ct);
        return Ok(result);
    }

    /// <summary>Logout and revoke refresh token</summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto request, CancellationToken ct)
    {
        await _mediator.Send(new LogoutCommand(request.RefreshToken, GetIpAddress()), ct);
        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>Get the currently authenticated user's info</summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new UnauthorizedException("User ID not found in token."));

        var result = await _mediator.Send(new GetCurrentUserQuery(userId), ct);
        return Ok(result);
    }
}
```

---

### 📁 `InventoryManagement.API/Extensions/`

#### `AuthServiceExtensions.cs` ← Clean DI registration
```csharp
namespace InventoryManagement.API.Extensions;

public static class AuthServiceExtensions
{
    public static IServiceCollection AddAuthServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Bind JwtSettings from appsettings.json
        var jwtSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);
        var jwtSettings = jwtSection.Get<JwtSettings>()!;

        // 2. Register JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero  // No grace period — 15 min means 15 min
            };

            // Return proper 401 response (not redirect)
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync(
                        JsonSerializer.Serialize(new
                        {
                            statusCode = 401,
                            message = "Unauthorized. Please provide a valid access token."
                        }));
                }
            };
        });

        // 3. Policy-based authorization using permission claims
        services.AddAuthorization(options =>
        {
            // Product permissions
            options.AddPolicy("CanCreateProduct",
                p => p.RequireClaim("permission", Permissions.ProductCreate));
            options.AddPolicy("CanUpdateProduct",
                p => p.RequireClaim("permission", Permissions.ProductUpdate));
            options.AddPolicy("CanDeleteProduct",
                p => p.RequireClaim("permission", Permissions.ProductDelete));

            // Inventory permissions
            options.AddPolicy("CanManageInventory",
                p => p.RequireClaim("permission", Permissions.InventoryManage));
            options.AddPolicy("CanViewInventory",
                p => p.RequireClaim("permission", Permissions.InventoryView));

            // Order permissions
            options.AddPolicy("CanCreateOrder",
                p => p.RequireClaim("permission", Permissions.OrderCreate));

            // Admin-only
            options.AddPolicy("AdminOnly",
                p => p.RequireRole(DefaultRoles.Admin));

            // Vendor or Admin
            options.AddPolicy("VendorOrAdmin",
                p => p.RequireRole(DefaultRoles.Vendor, DefaultRoles.Admin));
        });

        // 4. Register service implementations
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}
```

---

### `appsettings.json` — Auth Configuration
```json
{
  "JwtSettings": {
    "Secret": "YOUR-SUPER-SECRET-KEY-MINIMUM-32-CHARACTERS-LONG",
    "Issuer": "InventoryManagement",
    "Audience": "InventoryManagement-Client",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### `JwtSettings.cs` (Options class)
```csharp
// Place in Infrastructure/Services/ or a shared Settings folder
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; set; } = 15;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}
```

---

### `Program.cs` — Clean entry point
```csharp
var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Your existing registrations
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);

// Fluent Validation
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

// ← Auth module (single line, clean)
builder.Services.AddAuthServices(builder.Configuration);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<GlobalExceptionHandler>();   // ← must be first

app.UseAuthentication();   // ← must be before UseAuthorization
app.UseAuthorization();

app.MapControllers();
app.Run();
```

---

## 8. COMPLETE FLOWS

### 🟢 REGISTER FLOW
```
POST /api/auth/register
{ firstName, lastName, email, password }
        │
        ▼
[FluentValidation Middleware]
  ✗ Invalid? → 400 Bad Request with field-level errors
  ✓ Valid → continue
        │
        ▼
[RegisterUserCommand → MediatR → RegisterUserCommandHandler]
        │
        ▼
[AuthService.RegisterAsync()]
  ├─ EmailExistsAsync() → DUPLICATE? → throw 409 Conflict
  ├─ BCrypt.HashPassword(password, workFactor=12)
  ├─ Create User entity
  ├─ UserRepository.AddAsync() → INSERT into users table
  ├─ TokenService.GenerateAccessToken() → JWT (15 min)
  │    Claims: sub, email, jti, iat, roles[], permissions[]
  ├─ TokenService.GenerateRefreshToken() → 64-byte crypto random
  ├─ RefreshTokenRepository.AddAsync() → INSERT into refresh_tokens
  └─ Return AuthResponseDto
        │
        ▼
201 Created → { accessToken, refreshToken, expiresIn, user: {...} }
```

---

### 🔵 LOGIN FLOW
```
POST /api/auth/login
{ email, password }
        │
        ▼
[FluentValidation] → 400 if invalid
        │
        ▼
[AuthService.LoginAsync(dto, ipAddress)]
  ├─ GetByEmailWithRolesAsync() 
  │    → NOT FOUND? → throw 401 (generic message — prevents user enumeration)
  ├─ IsActive check → false? → 401
  ├─ IsLockedOut check → locked? → 423 Account Locked
  ├─ BCrypt.Verify(inputPassword, storedHash)
  │    → MISMATCH?
  │         FailedLoginAttempts++
  │         >= 5? → LockoutEnd = now + 15 min
  │         SaveChanges()
  │         throw 401 (same generic message)
  │    → MATCH?
  │         FailedLoginAttempts = 0
  │         LockoutEnd = null
  │         SaveChanges()
  ├─ GetRoles() → ["Vendor"]
  ├─ GetPermissions() → ["product:create", "inventory:manage", ...]
  ├─ GenerateAccessToken() with all claims
  ├─ GenerateRefreshToken() with IP address
  ├─ SaveRefreshToken to DB
  └─ Return AuthResponseDto
        │
        ▼
200 OK → { accessToken, refreshToken, accessTokenExpiry, user }
```

---

### 🔁 REFRESH TOKEN FLOW
```
POST /api/auth/refresh-token
{ refreshToken: "abc123..." }
        │
        ▼
[AuthService.RefreshTokenAsync(token, ipAddress)]
        │
        ▼
RefreshTokenRepository.GetByTokenAsync()
  → NOT FOUND? → 401 "Invalid refresh token"
        │
        ▼
storedToken.IsRevoked?
  → YES (already revoked = THEFT SCENARIO)
       RevokeAllActiveTokensAsync(userId) → all sessions force-logged-out
       Log SECURITY WARNING
       → 401 "Suspicious activity. Please login again."
        │
        ▼
storedToken.IsExpired?
  → YES → 401 "Refresh token expired. Please login again."
        │
        ▼
TOKEN ROTATION:
  storedToken.RevokedAt = now
  storedToken.ReasonRevoked = "Rotation"
  newRefreshToken = GenerateRefreshToken(userId, ip)
  storedToken.ReplacedByToken = newRefreshToken.Token  ← audit chain
  UPDATE storedToken
  INSERT newRefreshToken
        │
        ▼
Load user with latest roles/permissions from DB
Generate new AccessToken with fresh claims
        │
        ▼
200 OK → { newAccessToken, newRefreshToken, expiry, user }
```

---

### 🔴 LOGOUT FLOW
```
POST /api/auth/logout  [Requires: Bearer token]
{ refreshToken: "abc123..." }
        │
        ▼
[AuthService.LogoutAsync(token, ipAddress)]
  ├─ GetByTokenAsync()
  ├─ Null or already revoked? → silently return OK (idempotent)
  ├─ storedToken.RevokedAt = now
  ├─ storedToken.ReasonRevoked = "Logout"
  ├─ storedToken.RevokedByIp = clientIp
  └─ SaveChanges()
        │
        ▼
200 OK → { message: "Logged out successfully." }

NOTE: Access token still valid for remaining ~15 min
      This is by design — acceptable for monolith
      For immediate invalidation: add Redis blacklist (future scope)
```

---

### 🔒 PROTECTED ENDPOINT FLOW
```
GET /api/products  [Requires: Bearer token + "product:read" permission]
Authorization: Bearer eyJhbGci...
        │
        ▼
[JwtBearerMiddleware]
  ├─ Extract token from Authorization header
  ├─ Validate signature against secret key
  ├─ Validate expiry (ClockSkew = 0)
  ├─ Validate issuer + audience
  └─ Populate HttpContext.User with claims
        │
        ▼
[Authorization Middleware]
  [Authorize(Policy = "CanViewProducts")]
  ├─ Check "permission" claim contains "product:read"
  └─ DENY → 403 Forbidden | ALLOW → continue
        │
        ▼
Controller action executes
```

---

## 9. DATABASE SCHEMA (Neon PostgreSQL)

```sql
-- users table
CREATE TABLE users (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name            VARCHAR(100) NOT NULL,
    last_name             VARCHAR(100) NOT NULL,
    email                 VARCHAR(256) NOT NULL UNIQUE,
    password_hash         TEXT NOT NULL,
    is_active             BOOLEAN DEFAULT TRUE,
    is_email_verified     BOOLEAN DEFAULT FALSE,
    failed_login_attempts INTEGER DEFAULT 0,
    lockout_end           TIMESTAMPTZ NULL,
    created_at            TIMESTAMPTZ DEFAULT NOW(),
    updated_at            TIMESTAMPTZ NULL
);

-- roles table
CREATE TABLE roles (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_system   BOOLEAN DEFAULT FALSE
);

-- permissions table
CREATE TABLE permissions (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(200) NOT NULL UNIQUE,   -- "product:create"
    resource    VARCHAR(100) NOT NULL,           -- "product"
    action      VARCHAR(100) NOT NULL,           -- "create"
    description TEXT
);

-- user_roles junction
CREATE TABLE user_roles (
    user_id     UUID REFERENCES users(id) ON DELETE CASCADE,
    role_id     UUID REFERENCES roles(id) ON DELETE CASCADE,
    assigned_at TIMESTAMPTZ DEFAULT NOW(),
    PRIMARY KEY (user_id, role_id)
);

-- role_permissions junction
CREATE TABLE role_permissions (
    role_id       UUID REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

-- refresh_tokens table — critical audit table
CREATE TABLE refresh_tokens (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id          UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token            VARCHAR(500) NOT NULL UNIQUE,
    jwt_id           VARCHAR(200),
    expiry_date      TIMESTAMPTZ NOT NULL,
    created_at       TIMESTAMPTZ DEFAULT NOW(),
    created_by_ip    VARCHAR(50),
    revoked_at       TIMESTAMPTZ NULL,
    revoked_by_ip    VARCHAR(50) NULL,
    replaced_by_token VARCHAR(500) NULL,         -- audit chain
    reason_revoked   VARCHAR(200) NULL
);

-- Performance indexes
CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token   ON refresh_tokens(token);
CREATE INDEX idx_users_email            ON users(email);
```

---

## 10. SECURITY HARDENING CHECKLIST

```
PASSWORD SECURITY
✅ BCrypt with work factor 12
✅ Minimum 8 chars + uppercase + number + special char (FluentValidation)
✅ Account lockout: 5 failed attempts → 15 min lock
✅ Generic error messages (prevents user enumeration)

TOKEN SECURITY
✅ Access token: 15 min expiry, ClockSkew = 0
✅ Refresh token: 7 days, crypto-random 64 bytes
✅ Refresh token rotation on every use
✅ Reuse detection → revoke entire session family
✅ JWT contains: sub, email, jti, iat, roles[], permissions[]
✅ Secret stored in environment variable / user-secrets
✅ Never log token values

API SECURITY
✅ FluentValidation on all inputs
✅ Global exception handler (no stack traces in responses)
✅ HTTPS enforced (Render does this automatically)
✅ Authorization header validation via middleware
✅ IP address logged on all auth events

THINGS TO NOT DO
❌ Never store plain text passwords
❌ Never use MD5/SHA1/SHA256 for passwords
❌ Never put JWT secret in source code
❌ Never return stack traces to client
❌ Never say "email not found" vs "wrong password" — same message always
❌ Never store refresh tokens in localStorage (use HTTP-Only cookies in SPA)
```

---

## 11. ERROR HANDLING

### Update `GlobalExceptionHandler.cs`
```csharp
namespace InventoryManagement.API.Middleware;

public class GlobalExceptionHandler : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            UnauthorizedException     => (401, ex.Message),
            AccountLockedException    => (423, ex.Message),
            InvalidOperationException => (409, ex.Message),
            ValidationException ve    => (400, string.Join("; ",
                ve.Errors.Select(e => e.ErrorMessage))),
            KeyNotFoundException      => (404, "Resource not found."),
            _                         => (500, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            statusCode,
            message,
            timestamp = DateTime.UtcNow
        });
    }
}
```

---

## 12. LOGGING WITH SERILOG

### What to log in AuthService:
```
✅ INFO  → User registered: {Email}
✅ INFO  → User logged in: {Email} from IP: {Ip}
✅ INFO  → Refresh token rotated for UserId: {UserId}
✅ INFO  → User logged out: UserId: {UserId}
✅ WARN  → Account locked: {Email} (too many failed attempts)
✅ WARN  → Failed login attempt: {Email} from IP: {Ip}
✅ WARN  → SECURITY ALERT: Token reuse detected UserId: {UserId} IP: {Ip}
✅ ERROR → Exception (auto-captured by GlobalExceptionHandler)

❌ NEVER log: passwords, tokens, password hashes, PII beyond email
```

---

## 13. DI REGISTRATION SUMMARY

```csharp
// In AuthServiceExtensions.cs (already shown above), all of these are registered:

services.AddScoped<IAuthService, AuthService>();
services.AddScoped<ITokenService, TokenService>();
services.AddScoped<IPasswordService, PasswordService>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// Why Scoped?
// Scoped = one instance per HTTP request
// Correct for DB repositories (each request gets its own DbContext)
// Correct for services that depend on repositories
```

---

## 14. STEP-BY-STEP IMPLEMENTATION ORDER

```
STEP 1 — Domain (no dependencies, start here)
  ✦ Create entities: User, Role, Permission, UserRole, RolePermission, RefreshToken
  ✦ Create enums: DefaultRoles, Permissions constants
  ✦ Create exceptions: UnauthorizedException, AccountLockedException
  ✦ Create interfaces: IUserRepository, IRefreshTokenRepository, ITokenService, IPasswordService

STEP 2 — Application DTOs + Validators
  ✦ Create: RegisterDto, LoginDto, AuthResponseDto, CurrentUserDto
  ✦ Create: RegisterDtoValidator, LoginDtoValidator
  ✦ Create: IAuthService interface

STEP 3 — Application Commands + Queries
  ✦ Create: LoginUserCommand + Handler
  ✦ Create: RegisterUserCommand + Handler
  ✦ Create: RefreshTokenCommand + Handler
  ✦ Create: LogoutCommand + Handler
  ✦ Create: GetCurrentUserQuery + Handler
  ✦ Create: AuthMappingProfile

STEP 4 — Infrastructure Services
  ✦ Create: JwtSettings.cs options class
  ✦ Create: PasswordService.cs (BCrypt)
  ✦ Create: TokenService.cs (JWT generation)
  ✦ Create: AuthService.cs (business logic)
  ✦ Install packages: BCrypt.Net-Next, JwtBearer, IdentityModel

STEP 5 — Infrastructure Persistence
  ✦ Modify: AppDbContext.cs (add DbSets)
  ✦ Create: EF Configurations (UserConfiguration, RefreshTokenConfiguration)
  ✦ Create: DataSeedConfiguration (roles + permissions)
  ✦ Create: UserRepository.cs
  ✦ Create: RefreshTokenRepository.cs
  ✦ Run: dotnet ef migrations add AddAuthTables
  ✦ Run: dotnet ef database update

STEP 6 — API Layer
  ✦ Create: AuthController.cs
  ✦ Create: AuthServiceExtensions.cs
  ✦ Modify: GlobalExceptionHandler.cs (add auth exceptions)
  ✦ Modify: Program.cs (add auth services)
  ✦ Test all endpoints with Swagger

STEP 7 — Test + Validate
  ✦ Test register → check DB for user + hashed password
  ✦ Test login → check JWT claims in jwt.io
  ✦ Test refresh → verify old token revoked, new token issued
  ✦ Test logout → verify token revoked in DB
  ✦ Test lockout → 5 wrong passwords → check LockoutEnd in DB
  ✦ Test reuse detection → use revoked token → verify all tokens revoked
```

---

## 15. FUTURE ENTERPRISE EXTENSIONS

### When your portfolio project grows, add these:

| Feature | Trigger | How |
|---|---|---|
| **Redis Token Blacklist** | Need instant access token revocation | StackExchange.Redis (free) — store `jti` claim on logout |
| **Email Verification** | Production launch | Add `EmailVerificationToken` entity + SMTP via MailKit (free) |
| **Password Reset Flow** | Users forget passwords | `PasswordResetToken` entity + email link |
| **HTTP-Only Cookie for Refresh** | SPA frontend security | `Response.Cookies.Append()` with `HttpOnly = true, SameSite = Strict` |
| **Rate Limiting** | Prevent brute force at API level | ASP.NET Core built-in `AddRateLimiter()` — free, .NET 7+ |
| **OAuth (Google Login)** | Social login requirement | `Microsoft.AspNetCore.Authentication.Google` — free |
| **RS256 Signing** | Moving to microservices | Azure Key Vault (free tier) + asymmetric keys |
| **Refresh Token Family Tree** | Detect sophisticated token theft | Track `ReplacedByToken` chain — already in schema |
| **Audit Log Table** | Compliance / enterprise audit | Separate `AuthAuditLog` entity, append-only |
| **MFA / TOTP** | High security apps | `Otp.NET` (free) — Google Authenticator compatible |

---

## 16. FINAL FILE MAP

```
InventoryManagement.Domain/
  Entities/User.cs
  Entities/Role.cs
  Entities/Permission.cs
  Entities/UserRole.cs
  Entities/RolePermission.cs
  Entities/RefreshToken.cs
  Enums/DefaultRoles.cs             ← also contains Permissions constants
  Exceptions/UnauthorizedException.cs
  Exceptions/AccountLockedException.cs
  Interfaces/IUserRepository.cs
  Interfaces/IRefreshTokenRepository.cs
  Interfaces/ITokenService.cs
  Interfaces/IPasswordService.cs

InventoryManagement.Application/
  Features/Auth/Commands/RegisterUserCommand.cs
  Features/Auth/Commands/LoginUserCommand.cs
  Features/Auth/Commands/RefreshTokenCommand.cs
  Features/Auth/Commands/LogoutCommand.cs
  Features/Auth/Queries/GetCurrentUserQuery.cs
  DTOs/RegisterDto.cs
  DTOs/LoginDto.cs
  DTOs/AuthResponseDto.cs
  DTOs/CurrentUserDto.cs
  Interfaces/IAuthService.cs
  Validators/RegisterDtoValidator.cs
  Validators/LoginDtoValidator.cs
  Mappings/AuthMappingProfile.cs

InventoryManagement.Infrastructure/
  Persistence/AppDbContext.cs        ← MODIFY (add DbSets)
  Persistence/Configurations/UserConfiguration.cs
  Persistence/Configurations/RoleConfiguration.cs
  Persistence/Configurations/RefreshTokenConfiguration.cs
  Persistence/Configurations/DataSeedConfiguration.cs
  Persistence/Repositories/UserRepository.cs
  Persistence/Repositories/RefreshTokenRepository.cs
  Services/AuthService.cs
  Services/TokenService.cs
  Services/PasswordService.cs
  Services/JwtSettings.cs            ← options class

InventoryManagement.API/
  Controllers/AuthController.cs
  Extensions/AuthServiceExtensions.cs
  Middleware/GlobalExceptionHandler.cs  ← MODIFY
  Program.cs                            ← MODIFY

Config:
  appsettings.json                      ← add JwtSettings section
```

---

> **Architecture verdict:** Teri existing architecture **100% correct** hai enterprise monolith ke liye.
> Clean Architecture + CQRS + Feature folders — yahi pattern senior developers use karte hain.
> Auth module bilkul isi pattern mein fit hota hai — koi restructuring needed nahi.
>
> **All packages used: 100% free and open source.**
> `BCrypt.Net-Next` · `JwtBearer` · `FluentValidation` · `MediatR` · `AutoMapper` · `Serilog`