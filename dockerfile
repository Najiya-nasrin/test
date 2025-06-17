# --- Stage 2: Test ---
FROM build AS test
WORKDIR /src

# Install ReportGenerator tool for coverage reports
RUN dotnet tool install --global dotnet-reportgenerator-globaltool

# Ensure directories exist
RUN mkdir -p /app/test-results /app/allure-results /app/test-results/CoverageReport

# Run tests with inline coverage settings (no runsettings file needed)
RUN dotnet test CardValidation.Tests/CardValidation.Tests.csproj \
    --logger "trx;LogFileName=all-tests.trx" \
    --results-directory /app/test-results \
    --collect:"XPlat Code Coverage" \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    /p:CoverletOutput=/app/test-results/coverage.opencover.xml \
    /p:Include="[CardValidation.Core]*,[CardValidation.Web]*" \
    /p:Exclude="[*.Tests]*" || true

# Generate HTML coverage report
RUN /root/.dotnet/tools/reportgenerator \
    -reports:"/app/test-results/coverage.opencover.xml" \
    -targetdir:"/app/test-results/CoverageReport" \
    -reporttypes:"Html;Badges" || echo "Coverage report generation failed"

# Copy Allure results if they exist
RUN if [ -d "/src/CardValidation.Tests/bin/Release/net8.0/allure-results" ]; then \
      cp -r /src/CardValidation.Tests/bin/Release/net8.0/allure-results/* /app/allure-results/ 2>/dev/null || true; \
    fi

# Also check for allure-results in test project root
RUN if [ -d "/src/CardValidation.Tests/allure-results" ]; then \
      cp -r /src/CardValidation.Tests/allure-results/* /app/allure-results/ 2>/dev/null || true; \
    fi

# List contents for debugging
RUN echo "=== Test Results Structure ===" && \
    find /app/test-results -type f 2>/dev/null || echo "No test results found" && \
    echo "=== Allure Results Structure ===" && \
    find /app/allure-results -type f 2>/dev/null || echo "No allure results found"