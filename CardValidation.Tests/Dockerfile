# Use the official .NET SDK image for building and running tests
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base

# Install curl for debugging connectivity
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /app

# Copy solution file and project files for dependency resolution
COPY *.sln ./
COPY CardValidation.Tests/*.csproj ./CardValidation.Tests/

# If you have other projects in the solution, copy their .csproj files too
# COPY CardValidation.Api/*.csproj ./CardValidation.Api/
# COPY CardValidation.Core/*.csproj ./CardValidation.Core/

# Restore NuGet packages
RUN dotnet restore CardValidation.Tests/CardValidation.Tests.csproj

# Copy the entire source code
COPY . .

# Set working directory to the test project
WORKDIR /app/CardValidation.Tests

# Create TestResults directory for output
RUN mkdir -p /app/TestResults

# Set environment variables for test execution
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1

# Create a startup script for better debugging
RUN echo '#!/bin/bash\n\
echo "=== Docker Container Debug Info ==="\n\
echo "API_BASE_URL: $API_BASE_URL"\n\
echo "Current directory: $(pwd)"\n\
echo "Network info:"\n\
ip addr show || true\n\
echo ""\n\
echo "=== Testing API Connectivity ==="\n\
if [ ! -z "$API_BASE_URL" ]; then\n\
  echo "Testing connection to: $API_BASE_URL"\n\
  curl -v "$API_BASE_URL" --connect-timeout 10 --max-time 30 || echo "Connection failed"\n\
  echo ""\n\
  echo "Testing Swagger endpoint:"\n\
  curl -v "$API_BASE_URL/swagger/index.html" --connect-timeout 10 --max-time 30 || echo "Swagger connection failed"\n\
  echo ""\n\
fi\n\
echo "=== Starting Tests ==="\n\
dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory /app/TestResults --verbosity normal\n\
' > /start.sh && chmod +x /start.sh

# Default command to run the tests with debugging
CMD ["/start.sh"]