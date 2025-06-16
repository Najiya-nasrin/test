# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CardValidation.sln .
COPY CardValidation.Core/CardValidation.Core.csproj CardValidation.Core/
COPY CardValidation.Tests/CardValidation.Tests.csproj CardValidation.Tests/
COPY CardValidation.Web/CardValidation.Web.csproj CardValidation.Web/

RUN dotnet restore CardValidation.sln

COPY . .

RUN dotnet publish CardValidation.Web/CardValidation.Web.csproj -c Release -o /app/publish

# --- Test stage ---
FROM build AS test
WORKDIR /src

# Run tests with coverage and TRX logger
# Adding '|| true' ensures the build doesn't fail immediately if tests fail,
# allowing the subsequent debug command to run.
RUN dotnet test CardValidation.Tests/CardValidation.Tests.csproj \
    --logger "trx;LogFileName=all-tests.trx" \
    --results-directory /app/test-results \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=/app/test-results/coverage.xml || true

# --- IMPORTANT DEBUG STEP: List contents of test-results immediately after tests ---
RUN echo "--- Contents of /app/test-results inside the container after dotnet test: ---" && \
    ls -la /app/test-results && \
    echo "-----------------------------------------------------------------------"

# Copy Allure results if available (for Allure.NUnit)
RUN if [ -d "CardValidation.Tests/allure-results" ]; then \
      cp -r CardValidation.Tests/allure-results /app/allure-results; \
    fi

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "CardValidation.Web.dll"]
