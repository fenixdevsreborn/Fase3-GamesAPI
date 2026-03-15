# Autenticação JWT — Games API confia na Users API

A Games API **não emite** tokens. Ela **valida** tokens JWT (RS256) emitidos pela **Users API**, usando o documento OIDC e a JWKS expostos pela Users API.

## Fluxo

1. O cliente autentica na **Users API** (POST `/auth/login`) e recebe um access token JWT (RS256).
2. O cliente envia esse token nas requisições à **Games API** no header `Authorization: Bearer <token>`.
3. A Games API obtém o documento de discovery em `{Authority}/.well-known/openid-configuration` e a JWKS em `{Authority}/.well-known/jwks.json`.
4. Com a JWKS, valida assinatura, issuer, audience e expiração do token e extrai `sub`, `role`, `scope`.

## Configuração

### Variáveis de ambiente (recomendado em produção)

- `Jwt__Authority` — URL base da Users API (ex.: `https://users-api.example.com`).
- `Jwt__Audience` — Audience esperada (ex.: `fcg-cloud-platform`).
- `Jwt__RequireHttpsMetadata` — `true` ou `false` (em dev local pode ser `false`).
- `Jwt__MetadataRequestTimeoutSeconds` — Timeout em segundos para requisições de metadata (padrão 10).

### appsettings (exemplos)

**Desenvolvimento local** (`appsettings.Development.json`):

```json
{
  "Jwt": {
    "Authority": "https://localhost:5001",
    "Audience": "fcg-cloud-platform",
    "RequireHttpsMetadata": false
  }
}
```

**Produção** (`appsettings.json` ou variáveis):

```json
{
  "Jwt": {
    "Authority": "https://users-api.seudominio.com",
    "Audience": "fcg-cloud-platform",
    "RequireHttpsMetadata": true,
    "MetadataRequestTimeoutSeconds": 10
  }
}
```

## Indisponibilidade da metadata/JWKS

- O middleware JWT Bearer usa o **ConfigurationManager** do Microsoft.IdentityModel, que faz cache da configuração e **revalida** periodicamente.
- Se a Users API estiver temporariamente indisponível, as requisições que precisam validar o token falham (401) até a metadata ser obtida de novo.
- Em **OnAuthenticationFailed** e **OnChallenge** está configurado log (Warning/Debug) para facilitar diagnóstico.
- Timeouts e retentativas: use `Jwt:MetadataRequestTimeoutSeconds` e, se necessário, um **BackchannelHttpHandler** customizado com retry (ex.: Polly).

## Estrutura (Authentication / Authorization)

- **Authentication**: `JwtOptions`, `JwtBearerExtensions`, `JwtBearerPostConfigureOptions`, `IBackchannelHttpHandlerFactory` (opcional, para testes), claim types e policies.
- **Authorization**: `AuthorizationExtensions`, `UserClaimsExtensions`, `OwnerAuthorization`, `CurrentUserAccessor` / `ICurrentUserAccessor`.
- **/me/library** usa apenas `User.GetUserId()` (claim `sub`); não aceita `userId` no body para operações do usuário logado.

## Testes de integração

- O projeto de integração usa um servidor OIDC **in-process** (`TestOidcServer`) baseado em **HttpListener**, que expõe discovery e JWKS e gera tokens para os testes.
- A Games API é configurada com `Jwt:Authority` apontando para esse servidor; não é necessário subir a Users API para rodar os testes.
- Exemplo: `GetGames_WithValidToken_Returns200` obtém um token do `TestOidcServer`, envia em `GET /games` e verifica 200.

## Pacotes NuGet

- **Fcg.Games.Api**: `Microsoft.AspNetCore.Authentication.JwtBearer` (validação via Authority + JWKS).

Nenhuma chave simétrica; a validação é feita apenas com as chaves públicas da JWKS da Users API.
