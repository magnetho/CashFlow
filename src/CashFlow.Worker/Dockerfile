FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY ["CashFlow.sln", "Directory.Build.props", "global.json", "NuGet.Config", "./"]
COPY ["src/CashFlow.Worker/CashFlow.Worker.csproj", "src/CashFlow.Worker/"]
COPY ["src/CashFlow.Application/CashFlow.Application.csproj", "src/CashFlow.Application/"]
COPY ["src/CashFlow.Contracts/CashFlow.Contracts.csproj", "src/CashFlow.Contracts/"]
COPY ["src/CashFlow.Domain/CashFlow.Domain.csproj", "src/CashFlow.Domain/"]
COPY ["src/CashFlow.Infrastructure/CashFlow.Infrastructure.csproj", "src/CashFlow.Infrastructure/"]
RUN dotnet restore src/CashFlow.Worker/CashFlow.Worker.csproj

COPY src/ src/
RUN dotnet publish src/CashFlow.Worker/CashFlow.Worker.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
USER root
RUN apt-get update \
    && apt-get install --yes --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "CashFlow.Worker.dll"]
