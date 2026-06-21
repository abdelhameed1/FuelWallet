

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Web/FuelWallet.Web.csproj", "src/Web/"]
COPY ["src/Application/FuelWallet.Application.csproj", "src/Application/"]
COPY ["src/Domain/FuelWallet.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/FuelWallet.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/Web/FuelWallet.Web.csproj"


COPY src/ src/
RUN dotnet publish "src/Web/FuelWallet.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app


ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

EXPOSE 8080


USER $APP_UID

ENTRYPOINT ["dotnet", "FuelWallet.Web.dll"]
