# Usar la imagen oficial de .NET 9.0 SDK para compilar
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Establecer el directorio de trabajo
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["src/Presentation/Presentation.csproj", "src/Presentation/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/DependencyInjection/DependencyInjection.csproj", "src/DependencyInjection/"]

# Restaurar dependencias
RUN dotnet restore "src/Presentation/Presentation.csproj"

# Copiar el resto del código fuente
COPY . .

# Compilar la aplicación
WORKDIR "/src/src/Presentation"
RUN dotnet build "Presentation.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "Presentation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Usar la imagen de runtime para producción
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Instalar curl para health checks
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Establecer el directorio de trabajo
WORKDIR /app

# Copiar la aplicación publicada
COPY --from=publish /app/publish .

# Crear usuario no-root y cambiar permisos
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser:appuser /app
USER appuser

# Exponer el puerto
EXPOSE 8080

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Ejecutar la aplicación
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]