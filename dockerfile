# Use the official .NET SDK image for building and testing
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# Copy solution file and project files
COPY *.sln ./
COPY CardValidation.Core/*.csproj ./CardValidation.Core/
COPY CardValidation.Web/*.csproj ./CardValidation.Web/
COPY CardValidation.Tests/*.csproj ./CardValidation.Tests/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Test stage - runs both unit and integration tests
FROM build AS test

# Create test results directory
RUN mkdir -p /app/test-results
RUN mkdir -p /app/CardValidation.Tests/allure-results

# Set Allure environment variables
ENV ALLURE_RESULTS_DIRECTORY=/app/CardValidation.Tests/allure-results

# Run all tests with coverage collection, TRX logging, and Allure reporting
RUN dotnet test \
    --collect:"XPlat Code Coverage" \
    --logger "trx;LogFileName=all-tests.trx" \
    --results-directory ./test-results \
    --environment ALLURE_RESULTS_DIRECTORY=/app/CardValidation.Tests/allure-results \
    --verbosity normal

# Production build stage
FROM build AS publish
RUN dotnet publish CardValidation.Web -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port (adjust if your app uses a different port)
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "CardValidation.Web.dll"]