FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY StockifyPlus.csproj ./
RUN dotnet restore "StockifyPlus.csproj"

COPY . ./
RUN dotnet publish "StockifyPlus.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
CMD ["sh", "-c", "dotnet StockifyPlus.dll --urls http://0.0.0.0:${PORT:-8080}"]
