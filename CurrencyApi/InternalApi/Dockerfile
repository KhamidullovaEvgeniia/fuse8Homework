FROM mcr.microsoft.com/dotnet/sdk:9.0 AS prepare-restore-nuget
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-subset --version 0.3.2
COPY . /src
WORKDIR /src
RUN dotnet subset restore InternalApi/InternalApi.csproj --root-directory /src --output restore_subset/

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore-subset
WORKDIR /src
COPY --from=prepare-restore-nuget /src/restore_subset .
RUN dotnet restore InternalApi/InternalApi.csproj

FROM restore-subset AS publish
COPY . .
RUN dotnet publish InternalApi/InternalApi.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
RUN apt-get update && apt-get install -y curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
LABEL org.opencontainers.image.authors="evgeniia_khamidullova@my.com"
LABEL description="Сервис для получения курса валют"
LABEL version="1.0.0"
EXPOSE 5243
EXPOSE 5244
ENTRYPOINT ["dotnet", "InternalApi.dll"]
