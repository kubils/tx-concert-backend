FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY Directory.Build.props ./
COPY src/TxConcert.Domain/TxConcert.Domain.csproj src/TxConcert.Domain/
COPY src/TxConcert.Application/TxConcert.Application.csproj src/TxConcert.Application/
COPY src/TxConcert.Infrastructure/TxConcert.Infrastructure.csproj src/TxConcert.Infrastructure/
COPY src/TxConcert.Api/TxConcert.Api.csproj src/TxConcert.Api/
RUN dotnet restore src/TxConcert.Api/TxConcert.Api.csproj

# Copy everything and build
COPY . .
RUN dotnet publish src/TxConcert.Api/TxConcert.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "TxConcert.Api.dll"]
