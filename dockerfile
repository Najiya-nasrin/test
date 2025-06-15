# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY CardValidation.sln .

# Copy project files
COPY CardValidation.Core/CardValidation.Core.csproj CardValidation.Core/
COPY CardValidation.Tests/CardValidation.Tests.csproj CardValidation.Tests/
COPY CardValidation.Web/CardValidation.Web.csproj CardValidation.Web/

# Restore dependencies
RUN dotnet restore CardValidation.sln

# Copy source code
COPY . .

# Build and publish
RUN dotnet publish CardValidation.Web/CardValidation.Web.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "CardValidation.Web.dll"]