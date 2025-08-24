# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj (no subfolder now)
COPY GameOfLife.API.csproj ./
RUN dotnet restore GameOfLife.API.csproj

# Copy everything else
COPY . .

RUN dotnet build GameOfLife.API.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish GameOfLife.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

RUN mkdir -p /app/logs && chmod 755 /app/logs

EXPOSE 80
EXPOSE 443
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GameOfLife.API.dll"]
