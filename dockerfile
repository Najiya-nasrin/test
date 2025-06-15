# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Set working directory
WORKDIR /app

# Copy everything first (we'll optimize later once it works)
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build the application
RUN dotnet build --configuration Release --no-restore

# Run tests
RUN dotnet test --configuration Release --no-build

# Set the entry point to keep container running
CMD ["tail", "-f", "/dev/null"]