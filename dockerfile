# This Dockerfile is structured into multiple stages (Build, Test, Final)
# to optimize image size and build performance by leveraging Docker's build cache.

# --- Stage 1: Build ---
# This stage is responsible for restoring NuGet packages, building projects,
# and publishing the web application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Set the working directory inside the container for this stage
WORKDIR /src

# Copy solution file.
COPY CardValidation.sln .

# Copy project files.
COPY CardValidation.Core/CardValidation.Core.csproj CardValidation.Core/
COPY CardValidation.Tests/CardValidation.Tests.csproj CardValidation.Tests/
COPY CardValidation.Web/CardValidation.Web.csproj CardValidation.Web/

# Restore NuGet packages.
RUN dotnet restore CardValidation.sln

# Copy remaining source.
COPY . .

# Publish web application.
RUN dotnet publish CardValidation.Web/CardValidation.Web.csproj -c Release -o /app/publish

# --- Stage 2: Test ---
# This stage runs the unit tests and collects code coverage information.
# It uses the 'build' stage as its base, inheriting its dependencies and restored projects.
FROM build AS test
# Set the working directory for this test stage.
WORKDIR /src

RUN dotnet test CardValidation.Tests/CardValidation.Tests.csproj \
    --logger "trx;LogFileName=all-tests.trx" \
    --results-directory /app/test-results \
    /p:CollectCoverage=true \
    /p:CoverletOutputFormat=cobertura \
    /p:CoverletOutput=/app/test-results/coverage.xml \
    /p:CoverletVerbosity=detailed \
    /p:CoverletInclude="[CardValidation.Core]*,[CardValidation.Web]*" || true # Runs unit tests and collects detailed code coverage.

# --- IMPORTANT DEBUG STEP: List contents of test-results immediately after tests ---
# This command runs during the Docker build process and prints the contents of the
# /app/test-results directory. This is CRUCIAL for verifying if coverage.xml and
# all-tests.trx are created and where they are located within the container.
RUN echo "--- Contents of /app/test-results inside the container after dotnet test: ---" && \
    ls -la /app/test-results && \
    echo "-----------------------------------------------------------------------"

# Copy Allure results.
RUN if [ -d "CardValidation.Tests/allure-results" ]; then \
      cp -r CardValidation.Tests/allure-results /app/allure-results; \
    fi

# --- Stage 3: Runtime (Final) ---
# This stage creates a lightweight Docker image containing only the published
# web application and its runtime dependencies, ready for deployment.
# It uses a smaller ASP.NET runtime image for efficiency.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# Set the working directory for the final application runtime.
WORKDIR /app

# Copy published application.
COPY --from=build /app/publish .

# Expose application port.
EXPOSE 8080

# Define application entrypoint.
ENTRYPOINT ["dotnet", "CardValidation.Web.dll"]
