# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["WishlistWeb/WishlistWeb.csproj", "WishlistWeb/"]
COPY ["WishlistModels/WishlistModels.csproj", "WishlistModels/"]
COPY ["WishlistContracts/WishlistContracts.csproj", "WishlistContracts/"]
RUN dotnet restore "WishlistWeb/WishlistWeb.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/WishlistWeb"
RUN dotnet build "WishlistWeb.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "WishlistWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create data directory for SQLite database and set permissions
RUN mkdir -p /data && chmod 777 /data

EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WishlistWeb.dll"]
