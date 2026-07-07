FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Directory.Build.props ./
COPY src/TxConcert.Domain/TxConcert.Domain.csproj src/TxConcert.Domain/
COPY src/TxConcert.Application/TxConcert.Application.csproj src/TxConcert.Application/
COPY src/TxConcert.Infrastructure/TxConcert.Infrastructure.csproj src/TxConcert.Infrastructure/
COPY src/TxConcert.Worker/TxConcert.Worker.csproj src/TxConcert.Worker/
RUN dotnet restore src/TxConcert.Worker/TxConcert.Worker.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/TxConcert.Worker/TxConcert.Worker.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TxConcert.Worker.dll"]
