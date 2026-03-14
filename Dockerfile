# Build — context = raiz do repositório do serviço.
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Fcg.Games.Api/Fcg.Games.Api.csproj src/Fcg.Games.Api/
COPY src/Fcg.Games.Application/Fcg.Games.Application.csproj src/Fcg.Games.Application/
COPY src/Fcg.Games.Contracts/Fcg.Games.Contracts.csproj src/Fcg.Games.Contracts/
COPY src/Fcg.Games.Domain/Fcg.Games.Domain.csproj src/Fcg.Games.Domain/
COPY src/Fcg.Games.Infrastructure/Fcg.Games.Infrastructure.csproj src/Fcg.Games.Infrastructure/

RUN dotnet restore src/Fcg.Games.Api/Fcg.Games.Api.csproj
COPY src src
RUN dotnet publish src/Fcg.Games.Api/Fcg.Games.Api.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Games.Api.dll"]
