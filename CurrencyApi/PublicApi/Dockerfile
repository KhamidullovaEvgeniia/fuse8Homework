FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

LABEL org.opencontainers.image.authors="evgeniia_khamidullova@my.com"
LABEL description="Сервис для получения курса валют"
LABEL version="1.0.0"

RUN apt-get update && apt-get install -y curl

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5001/healthcheck || exit 1
  
VOLUME /app/Data

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-subset --version 0.3.2
WORKDIR /src
COPY ["PublicApi/PublicApi.csproj", "PublicApi/"]
COPY ["General/General.csproj", "General/"]
COPY ["PublicApi.DataAccess/PublicApi.DataAccess.csproj", "PublicApi.DataAccess/"]
RUN dotnet subset restore "PublicApi/PublicApi.csproj" --root-directory /src --output nuget_subset/
RUN dotnet restore "PublicApi/PublicApi.csproj"
COPY . .
WORKDIR "/src/PublicApi"
RUN dotnet build "PublicApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PublicApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PublicApi.dll"]
