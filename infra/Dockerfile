FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/LinkShort.csproj", "backend/"]
RUN dotnet restore "backend/LinkShort.csproj"
COPY backend/ backend/
WORKDIR "/src/backend"
RUN dotnet build "LinkShort.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LinkShort.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /data && chown $APP_UID /data
USER $APP_UID

ENTRYPOINT ["dotnet", "LinkShort.dll"]