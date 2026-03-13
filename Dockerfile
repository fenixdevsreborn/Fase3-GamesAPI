# Build — context = FASE3: docker build -f Fase3-GamesAPI/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Fase3-GamesAPI/src/Fcg.Games.Api/Fcg.Games.Api.csproj Fase3-GamesAPI/src/Fcg.Games.Api/
COPY Fase3-GamesAPI/src/Fcg.Games.Application/Fcg.Games.Application.csproj Fase3-GamesAPI/src/Fcg.Games.Application/
COPY Fase3-GamesAPI/src/Fcg.Games.Contracts/Fcg.Games.Contracts.csproj Fase3-GamesAPI/src/Fcg.Games.Contracts/
COPY Fase3-GamesAPI/src/Fcg.Games.Domain/Fcg.Games.Domain.csproj Fase3-GamesAPI/src/Fcg.Games.Domain/
COPY Fase3-GamesAPI/src/Fcg.Games.Infrastructure/Fcg.Games.Infrastructure.csproj Fase3-GamesAPI/src/Fcg.Games.Infrastructure/
COPY Fase3-Shared/src/Fcg.Shared.Auth/Fcg.Shared.Auth.csproj Fase3-Shared/src/Fcg.Shared.Auth/
COPY Fase3-Shared/src/Fcg.Shared.Observability.AspNetCore/Fcg.Shared.Observability.AspNetCore.csproj Fase3-Shared/src/Fcg.Shared.Observability.AspNetCore/
COPY Fase3-Shared/src/Fcg.Shared.Observability/Fcg.Shared.Observability.csproj Fase3-Shared/src/Fcg.Shared.Observability/

RUN dotnet restore Fase3-GamesAPI/src/Fcg.Games.Api/Fcg.Games.Api.csproj
COPY Fase3-GamesAPI/src Fase3-GamesAPI/src
COPY Fase3-Shared Fase3-Shared
RUN dotnet publish Fase3-GamesAPI/src/Fcg.Games.Api/Fcg.Games.Api.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Games.Api.dll"]
