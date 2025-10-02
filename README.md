# TaskManager - Docker Setup

Esta aplicación TaskManager está completamente dockerizada y lista para ejecutarse en contenedores.

## Arquitectura

La aplicación consta de tres servicios principales:

- **Frontend**: Aplicación Angular 20 servida con Nginx
- **Backend**: API REST .NET 9.0
- **Database**: MongoDB 7

## Puertos

- Frontend: http://localhost:4200
- Backend API: http://localhost:8080
- MongoDB: localhost:27017

## Inicio Rápido

### Prerrequisitos

- Docker Desktop instalado
- Docker Compose V2

### Ejecutar la aplicación

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd DesafioTM
   ```

2. **Construir e iniciar los servicios**
   ```bash
   docker-compose up --build
   ```

3. **Acceder a la aplicación**
   - Frontend: http://localhost:4200
   - API Documentation: http://localhost:8080/swagger

### Comandos útiles

```bash
# Construir e iniciar en segundo plano
docker-compose up -d --build

# Ver logs
docker-compose logs -f

# Detener servicios
docker-compose down

# Limpiar volúmenes (borra datos de MongoDB)
docker-compose down -v

# Reiniciar un servicio específico
docker-compose restart backend
```

## Desarrollo

### Ejecutar solo el backend

```bash
docker-compose up mongodb backend --build
```

### Ejecutar solo el frontend

```bash
docker-compose up frontend --build
```

### Variables de entorno

Crear un archivo `.env` en la raíz del proyecto:

```env
# MongoDB
MONGO_ROOT_USERNAME=admin
MONGO_ROOT_PASSWORD=password123
MONGO_DATABASE=tasksmanagerbd

# JWT
JWT_SECRET_KEY=TaskManager2025-SecretKey-256bits-Required-For-HS256-Docker-Key
JWT_ISSUER=TaskManager
JWT_AUDIENCE=TaskManagerUsers
JWT_EXPIRATION_HOURS=24

# Email (opcional)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=tu-email@gmail.com
SMTP_PASSWORD=tu-app-password
```

## Health Checks

Todos los servicios incluyen health checks automáticos:

- **MongoDB**: Verifica conectividad a la base de datos
- **Backend**: Verifica endpoint `/health`
- **Frontend**: Verifica que Nginx responda

## Volúmenes

- `mongodb_data`: Persistencia de datos de MongoDB

## Troubleshooting

### Problemas comunes

1. **Puerto ocupado**: Cambiar los puertos en `docker-compose.yml`
2. **Build lento**: Verificar que `.dockerignore` excluya archivos innecesarios
3. **MongoDB no inicia**: Verificar que no haya otro MongoDB corriendo localmente

### Logs de debug

```bash
# Ver logs de un servicio específico
docker-compose logs backend

# Ver logs en tiempo real
docker-compose logs -f frontend
```

### Acceder a contenedores

```bash
# Shell en backend
docker-compose exec backend bash

# Shell en MongoDB
docker-compose exec mongodb mongosh -u admin -p password123
```

## Producción

Para despliegue en producción:

1. Configurar variables de entorno seguras
2. Usar secrets de Docker o servicios de configuración
3. Configurar reverse proxy (nginx, traefik)
4. Implementar HTTPS
5. Configurar backups automáticos para MongoDB

## Estado de Dockerización ✅

- ✅ **Backend Dockerfile**: Multi-stage build con .NET 9.0, seguridad hardening, health checks
- ✅ **Frontend Dockerfile**: Build Angular + Nginx, optimización SPA routing
- ✅ **docker-compose.yml**: Orquestación completa con dependencias y redes
- ✅ **nginx.conf**: Configuración SPA routing y compresión
- ✅ **.dockerignore**: Optimización build contexts
- ✅ **README.md**: Documentación completa de uso
- ✅ **.env.example**: Variables de entorno de ejemplo
- ✅ **Build verificado**: Todas las imágenes construidas exitosamente

## Estructura de archivos

```
DesafioTM/
├── backend/
│   ├── Dockerfile
│   ├── .dockerignore
│   └── src/
├── frontend/
│   ├── Dockerfile
│   ├── nginx.conf
│   ├── .dockerignore
│   └── src/
├── docker-compose.yml
└── README.md
```