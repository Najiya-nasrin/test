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

# Install reportgenerator tool for test coverage (optional)
RUN dotnet tool install --global dotnet-reportgenerator-globaltool
ENV PATH="$PATH:/root/.dotnet/tools"

# Run all tests with coverage collection (matching local setup exactly)
RUN dotnet test --collect:"XPlat Code Coverage" --results-directory ./test-results

# Generate HTML coverage report (matching local setup exactly)
RUN reportgenerator \
    "-reports:test-results/*/coverage.cobertura.xml" \
    "-targetdir:test-results/CoverageReport" \
    "-reporttypes:Html" || true

# Also generate TRX for GitHub Actions integration
RUN dotnet test \
    --logger "trx;LogFileName=all-tests.trx" \
    --results-directory ./test-results \
    --no-build || true

# Generate HTML coverage report (optional)
RUN reportgenerator \
    "-reports:/app/test-results/**/coverage.cobertura.xml" \
    "-targetdir:/app/test-results/coverage-report" \
    "-reporttypes:Html" || true

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