FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5243
EXPOSE 5244

LABEL org.opencontainers.image.authors="evgeniia_khamidullova@my.com"
LABEL description="Сервис для получения курса валют"
LABEL version="1.0.0"

RUN apt-get update && apt-get install -y curl

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5243/healthcheck || exit 1
  
VOLUME /app/Data

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-subset --version 0.3.2
WORKDIR /src
COPY ["InternalApi/InternalApi.csproj", "InternalApi/"]
COPY ["InternalApi.DataAccess/InternalApi.DataAccess.csproj", "InternalApi.DataAccess/"]
COPY ["General/General.csproj", "General/"]
RUN dotnet subset restore "InternalApi/InternalApi.csproj" --root-directory /src --output nuget_subset/
RUN dotnet restore "InternalApi/InternalApi.csproj"
COPY . .
WORKDIR "/src/InternalApi"
RUN dotnet build "InternalApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "InternalApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InternalApi.dll"]
