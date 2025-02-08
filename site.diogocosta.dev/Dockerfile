FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["site.diogocosta.dev/site.diogocosta.dev.csproj", "site.diogocosta.dev/"]
RUN dotnet restore "site.diogocosta.dev/site.diogocosta.dev.csproj"
COPY . .
WORKDIR "/src/site.diogocosta.dev"
RUN dotnet build "site.diogocosta.dev.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "site.diogocosta.dev.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "site.diogocosta.dev.dll"]
