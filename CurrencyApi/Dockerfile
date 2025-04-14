# Используем многоэтапную сборку для оптимизации образа
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS prepare-restore
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-subset --version 0.3.2
WORKDIR /src

COPY ["InternalApi/InternalApi.csproj", "InternalApi/"]
COPY ["InternalApi.DataAccess/InternalApi.DataAccess.csproj", "InternalApi.DataAccess/"]
COPY ["PublicApi/PublicApi.csproj", "PublicApi/"]
COPY ["PublicApi.DataAccess/PublicApi.DataAccess.csproj", "PublicApi.DataAccess/"]
COPY ["General/General.csproj", "General/"]

RUN dotnet subset restore "InternalApi/InternalApi.csproj" --root-directory /src --output nuget_subset/
RUN dotnet subset restore "PublicApi/PublicApi.csproj" --root-directory /src --output nuget_subset/

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY --from=prepare-restore /src/nuget_subset .
RUN dotnet restore "InternalApi/InternalApi.csproj" --packages /src/packages
RUN dotnet restore "PublicApi/PublicApi.csproj" --packages /src/packages

COPY . .

RUN dotnet publish "InternalApi/InternalApi.csproj" -c Release -o /app/publish/internal-api
RUN dotnet publish "PublicApi/PublicApi.csproj" -c Release -o /app/publish/public-api

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Устанавливаем curl для healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Метаданные образа
LABEL org.opencontainers.image.authors="evgeniia_khamidullova@my.com"
LABEL description="Сервис для получения курса валют"
LABEL version="1.0.0"

# Переменные окружения
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Внутренний API
FROM final AS internal-api
COPY --from=build /app/publish/internal-api .
EXPOSE 5000 5002
VOLUME /app/Data
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1
ENTRYPOINT ["dotnet", "InternalApi.dll"]

# Публичный API
FROM final AS public-api
COPY --from=build /app/publish/public-api .
EXPOSE 5001
VOLUME /app/Data
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5001/health || exit 1
ENTRYPOINT ["dotnet", "PublicApi.dll"]