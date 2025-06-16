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
RUN dotnet restore

# Copy source code
COPY . .

# Build the solution
RUN dotnet build CardValidation.sln -c Release --no-restore

# Test stage - NEW ADDITION
FROM build AS test
WORKDIR /src
RUN dotnet test CardValidation.Tests/CardValidation.Tests.csproj -c Release --no-build --logger trx --results-directory /app/test-results

# Publish stage
FROM build AS publish
RUN dotnet publish CardValidation.Web/CardValidation.Web.csproj -c Release -o /app/publish --no-build

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "CardValidation.Web.dll"]