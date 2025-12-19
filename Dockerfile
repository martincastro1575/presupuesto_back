# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY src/PlanificadorGastos.API/PlanificadorGastos.API.csproj ./PlanificadorGastos.API/
RUN dotnet restore ./PlanificadorGastos.API/PlanificadorGastos.API.csproj

# Copy everything else and build
COPY src/PlanificadorGastos.API/ ./PlanificadorGastos.API/
WORKDIR /src/PlanificadorGastos.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the app
ENTRYPOINT ["dotnet", "PlanificadorGastos.API.dll"]