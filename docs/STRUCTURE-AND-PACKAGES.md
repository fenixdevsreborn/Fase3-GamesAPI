# Fase3-GamesAPI — Estrutura e pacotes

Autenticação JWT (Authority + JWKS da Users API): **[docs/JWT-AUTHENTICATION.md](JWT-AUTHENTICATION.md)**.

## Árvore final de pastas (principais)

```
Fase3-GamesAPI/
├── src/
│   ├── Fcg.Games.Api/
│   │   ├── Authentication/
│   │   │   ├── FcgClaimTypes.cs
│   │   │   ├── FcgRoles.cs
│   │   │   ├── FcgScopes.cs
│   │   │   ├── FcgPolicies.cs
│   │   │   ├── JwtOptions.cs                  # Authority, Audience, RequireHttpsMetadata, MetadataRequestTimeoutSeconds
│   │   │   ├── JwtBearerExtensions.cs         # AddFcgJwtBearer (Authority + JWKS)
│   │   │   ├── JwtBearerPostConfigureOptions.cs
│   │   │   └── IBackchannelHttpHandlerFactory.cs  # Opcional (testes)
│   │   ├── Authorization/
│   │   │   ├── AuthorizationExtensions.cs      # AddFcgAuthorization, RequireScope
│   │   │   ├── UserClaimsExtensions.cs         # GetUserId, GetRole, IsAdmin, HasScope
│   │   │   ├── OwnerAuthorization.cs           # CanAccessResource (biblioteca)
│   │   │   ├── ICurrentUserAccessor.cs
│   │   │   └── CurrentUserAccessor.cs
│   │   ├── Observability/
│   │   │   ├── ObservabilityOptions.cs
│   │   │   ├── ObservabilityContext.cs
│   │   │   ├── IObservabilityContextAccessor.cs
│   │   │   ├── ObservabilityContextAccessor.cs
│   │   │   ├── FcgLogPropertyNames.cs
│   │   │   ├── FcgMetricNames.cs               # HTTP + games + library + recommendations + exceptions
│   │   │   ├── FcgMeters.cs
│   │   │   ├── CorrelationIdMiddleware.cs
│   │   │   ├── HttpMetricsMiddleware.cs
│   │   │   ├── ExceptionObservabilityMiddleware.cs
│   │   │   ├── ObservabilityDelegatingHandler.cs  # Propaga X-Correlation-ID e traceparent em HttpClient
│   │   │   ├── ObservabilityServiceCollectionExtensions.cs
│   │   │   └── ObservabilityApplicationBuilderExtensions.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs  # AddGamesApiAuth, AddGamesApiObservability
│   │   ├── OpenApi/
│   │   │   └── BearerSecuritySchemeTransformer.cs
│   │   ├── Controllers/
│   │   │   ├── GamesController.cs
│   │   │   ├── MeLibraryController.cs
│   │   │   └── InternalLibraryController.cs
│   │   ├── Program.cs
│   │   └── Program.IntegrationTests.cs
│   ├── Fcg.Games.Application/
│   ├── Fcg.Games.Domain/
│   ├── Fcg.Games.Infrastructure/
│   │   ├── DelegatingHandlers/
│   │   │   └── ForwardAuthorizationHandler.cs
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs  # AddGamesInfrastructure(+ configurePaymentsHttpClient)
│   └── Fcg.Games.Contracts/
├── tests/
│   ├── Fcg.Games.UnitTests/
│   │   ├── Authorization/
│   │   │   └── UserClaimsExtensionsTests.cs
│   │   ├── Observability/
│   │   │   └── FcgMetersTests.cs
│   │   └── Services/
│   └── Fcg.Games.IntegrationTests/   # WebAppFixture, TestOidcServer (HttpListener), GamesIntegrationTests
└── docs/
    ├── JWT-AUTHENTICATION.md         # Como a Games API confia na Users API (Authority, JWKS)
    └── STRUCTURE-AND-PACKAGES.md
```

## Comandos NuGet necessários

**Fcg.Games.Api:**

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3
dotnet add package Microsoft.AspNetCore.OpenApi --version 10.0.3
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 10.0.0
dotnet add package Microsoft.OpenApi --version 2.0.0
dotnet add package Scalar.AspNetCore --version 2.13.6
```

**Fcg.Games.Infrastructure:**

```bash
dotnet add package Microsoft.AspNetCore.Http.Abstractions --version 2.2.0
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.0
dotnet add package Microsoft.Extensions.Http --version 10.0.0
dotnet add package Microsoft.Extensions.Http.Polly --version 10.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add package Polly.Extensions.Http --version 3.0.0
```

**Testes:**

```bash
# UnitTests
dotnet add package Moq --version 4.20.72
dotnet add package xunit --version 2.9.3
dotnet add package Microsoft.NET.Test.Sdk --version 17.14.1

# IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.3
dotnet add package xunit --version 2.9.3
```

Nenhum pacote do Fase3-Shared é necessário. JWT é validado com a mesma chave/issuer/audience da Users API (config `Jwt:SigningKey` etc.).
