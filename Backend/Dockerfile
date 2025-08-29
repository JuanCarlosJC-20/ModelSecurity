# Dockerfile para API .NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar solo los archivos de proyecto primero para aprovechar el cache de Docker
COPY ["Web/Web.csproj", "Web/"]
RUN dotnet restore "./Web/Web.csproj"

# Copiar el resto del código fuente
COPY . .
WORKDIR "/src/Web"
RUN dotnet build "./Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear usuario no-root para seguridad
RUN addgroup --system --gid 1001 dotnetgroup
RUN adduser --system --uid 1001 --ingroup dotnetgroup dotnetuser
USER dotnetuser

ENTRYPOINT ["dotnet", "Web.dll"]