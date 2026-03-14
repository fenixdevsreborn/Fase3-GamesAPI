# FCG Games API

API de catálogo de jogos, biblioteca do usuário e compra para a FCG Cloud Platform. Valida JWT emitido pela **Users API**.

## Decisões

- **Tags:** armazenadas como JSON no PostgreSQL (coluna `tags`).
- **Paginação:** `PageNumber` (1-based), `PageSize` (1–100).
- **Filtros:** `Genre`, `MinPrice`, `MaxPrice`, `Studio`, `IsPublished`.
- **Ordenação:** `SortBy` (Title, Price, CreatedAt), `SortDesc`.
- **JWT:** mesmo Issuer/Audience/SigningKey da Users API; claims `sub`, `role`, `scope` usadas para autorização.
- **Autorização:** `[Authorize(Roles = "admin")]` para criar/atualizar/excluir/preço/publicação; `[Authorize]` para catálogo, biblioteca e compra.
- **Biblioteca:** um registro por (UserId, GameId); duplicidade tratada como upsert (atualiza status/notes).
- **Soft delete em jogos:** `DeletedAt`; jogos já comprados continuam acessíveis na biblioteca mesmo se despublicados.
- **Payments API:** contrato `IPaymentsApiClient`; implementação com HttpClient (stub quando `UseRealApi: false`).
- **Recomendações:** exclui jogos já possuídos; ordena por quantidade de compras e depois por data; fallback para usuário sem histórico.

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL (ou `UseInMemoryDatabase`/ambiente `Testing` para testes)
- JWT: usar a mesma chave (`Jwt:SigningKey`) da Users API

## Pacotes

```bash
# Api
dotnet add src/Fcg.Games.Api package Microsoft.AspNetCore.OpenApi --version 10.0.3
dotnet add src/Fcg.Games.Api package Microsoft.OpenApi --version 2.0.0
dotnet add src/Fcg.Games.Api package Scalar.AspNetCore --version 2.13.6
dotnet add src/Fcg.Games.Api package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# Infrastructure
dotnet add src/Fcg.Games.Infrastructure package Microsoft.EntityFrameworkCore --version 10.0.0
dotnet add src/Fcg.Games.Infrastructure package Microsoft.EntityFrameworkCore.InMemory --version 10.0.0
dotnet add src/Fcg.Games.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3
dotnet add src/Fcg.Games.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add src/Fcg.Games.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# Testes
dotnet add tests/Fcg.Games.UnitTests package Moq --version 4.20.72
dotnet add tests/Fcg.Games.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing --version 10.0.3
```

## Migrations

```bash
dotnet ef migrations add InitialCreate --project src/Fcg.Games.Infrastructure --startup-project src/Fcg.Games.Api
dotnet ef database update --project src/Fcg.Games.Infrastructure --startup-project src/Fcg.Games.Api
```

## Configuração

- **ConnectionStrings:GamesDb** – PostgreSQL
- **Jwt:SigningKey** – mesma chave da Users API (mín. 32 caracteres)
- **Jwt:Issuer**, **Jwt:Audience** – iguais à Users API
- **PaymentsApi:BaseAddress**, **UseRealApi**, **StubCheckoutUrl**
- **InternalApi:ApiKey** – chave para `POST /internal/library/add-from-payment` (uso apenas pela Payments API; rede restrita)

## Executar

```bash
dotnet run --project src/Fcg.Games.Api
```

- Documentação: **/scalar**
- OpenAPI: **/openapi/v1.json**

## Testes

```bash
dotnet build tests/Fcg.Games.UnitTests/Fcg.Games.UnitTests.csproj
dotnet test tests/Fcg.Games.UnitTests/Fcg.Games.UnitTests.csproj --no-build
dotnet build tests/Fcg.Games.IntegrationTests/Fcg.Games.IntegrationTests.csproj
dotnet test tests/Fcg.Games.IntegrationTests/Fcg.Games.IntegrationTests.csproj --no-build
```

---

## Endpoints e exemplos

Obtenha o token na Users API: `POST /auth/login` com `email` e `password`. Use no header: `Authorization: Bearer <token>`.

### Catálogo (autenticado)

- **GET /games** – lista paginada  
  Query: `pageNumber`, `pageSize`, `genre`, `minPrice`, `maxPrice`, `studio`, `isPublished`, `sortBy`, `sortDesc`
- **GET /games/{id}** – detalhe
- **GET /games/search?q=** – busca
- **GET /games/recommendations?count=** – recomendações

### Admin (role admin)

- **POST /games** – criar jogo  
  Body: `title`, `description`, `genre`, `studio`, `price`, `coverUrl`, `tags[]`, `isPublished`, `isActive`
- **PUT /games/{id}** – atualizar
- **PATCH /games/{id}/price** – body `{ "price": 29.90 }`
- **PATCH /games/{id}/publication** – body `{ "isPublished": true }`
- **DELETE /games/{id}** – soft delete

### Biblioteca (autenticado, sempre do usuário do JWT)

- **GET /me/library** – lista da biblioteca  
  Query: `pageNumber`, `pageSize`, `status`
- **GET /me/library/{id}** – um item
- **POST /me/library** – body `{ "gameId": "...", "status": "Wishlist", "notes": "..." }`  
  Status: Owned, Wishlist, Favorite, Hidden, Archived
- **PUT /me/library/{id}** – body `{ "status": "...", "notes": "..." }`
- **DELETE /me/library/{id}** – remover

### Endpoint interno (não usar com JWT)

- **POST /internal/library/add-from-payment** – adiciona jogo à biblioteca de um usuário após pagamento aprovado.  
  **Proteção:** apenas header `X-Api-Key` (não usa JWT).  
  **Body:** `{ "userId": "guid", "gameId": "guid" }`.  
  **Uso:** chamado pela Payments API após confirmação de pagamento.  
  **Segurança:** use API key forte (`InternalApi:ApiKey`), restrinja o tráfego à rede interna/VPC e não exponha este endpoint à internet.

### Compra (autenticado, userId do JWT)

- **POST /games/{id}/purchase** – cria intenção de compra na Payments API; retorna `paymentId`, `status`, `checkoutUrl` (stub se Payments não configurado).

### Exemplo POST /games (admin)

```json
{
  "title": "My Game",
  "description": "A great game",
  "genre": "Action",
  "studio": "FCG Studio",
  "price": 49.90,
  "coverUrl": "https://example.com/cover.jpg",
  "tags": ["action", "multiplayer"],
  "isPublished": false,
  "isActive": true
}
```

### Exemplo POST /me/library

```json
{
  "gameId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Wishlist",
  "notes": "Want to play soon"
}
```

### Exemplo resposta POST /games/{id}/purchase (stub)

```json
{
  "paymentId": "stub-abc123",
  "status": "Pending",
  "checkoutUrl": "https://payments.example.com/checkout/stub"
}
```
