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

# Run unit tests with coverage and results output
RUN dotnet test CardValidation.Tests \
    --filter "FullyQualifiedName~UnitTests" \
    --configuration Release \
    --logger "trx;LogFileName=unit-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory /app/test-results \
    --collect:"XPlat Code Coverage" \
    --settings coverlet.runsettings || true

# Run integration tests with results output
RUN dotnet test CardValidation.Tests \
    --filter "FullyQualifiedName~IntegrationTests" \
    --configuration Release \
    --logger "trx;LogFileName=integration-tests.trx" \
    --logger "console;verbosity=detailed" \
    --results-directory /app/test-results \
    --collect:"XPlat Code Coverage" \
    --settings coverlet.runsettings || true

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