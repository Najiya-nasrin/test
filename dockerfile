# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CardValidation.sln .
COPY CardValidation.Core/CardValidation.Core.csproj CardValidation.Core/
COPY CardValidation.Tests/CardValidation.Tests.csproj CardValidation.Tests/
COPY CardValidation.Web/CardValidation.Web.csproj CardValidation.Web/

RUN dotnet restore CardValidation.sln
COPY . .
RUN dotnet publish CardValidation.Web/CardValidation.Web.csproj -c Release -o /app/publish

# --- Stage 2: Test ---
FROM build AS test
WORKDIR /src

# Ensure test-results exists
RUN mkdir -p /app/test-results /app/allure-results

RUN dotnet test CardValidation.Tests/CardValidation.Tests.csproj \
    --logger "trx;LogFileName=all-tests.trx" \
    --results-directory /app/test-results \
     --collect:"XPlat Code Coverage" \
    "/p:CoverletInclude=CardValidation.Core*,CardValidation.Web*" || true

RUN if [ -d "CardValidation.Tests/allure-results" ]; then \
      cp -r CardValidation.Tests/allure-results/* /app/allure-results/; \
    fi

# --- Stage 3: Final Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CardValidation.Web.dll"]
