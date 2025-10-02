# 🚀 Quick Start - TaskManager Docker

## URLs de Acceso

- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:8080
- **Swagger Docs**: http://localhost:8080/swagger

## 🔐 Credenciales de Prueba

### Usuario Administrador
- **Email**: `admin`
- **Password**: `Admin123`
- **Rol**: Admin

### Usuario de Prueba
- **Email**: `testuser@test.com`
- **Password**: `Test123`
- **Rol**: User

## ⚡ Comandos Docker

```bash
# Iniciar todos los servicios
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f backend
docker-compose logs -f frontend

# Verificar estado de contenedores
docker-compose ps

# Reiniciar un servicio
docker-compose restart backend

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (borra datos)
docker-compose down -v

# Reconstruir un servicio específico
docker-compose up -d --build frontend
```

## 🐛 Solución de Problemas

### No puedo acceder al frontend (ERR_CONNECTION_REFUSED)

1. Verificar que el contenedor esté corriendo:
   ```bash
   docker-compose ps
   ```

2. Ver logs del frontend:
   ```bash
   docker-compose logs frontend
   ```

3. Reiniciar el servicio:
   ```bash
   docker-compose restart frontend
   ```

### Error al iniciar sesión

1. Verificar que el backend esté healthy:
   ```bash
   docker-compose ps
   ```

2. Ver logs del backend:
   ```bash
   docker-compose logs backend --tail=50
   ```

3. Verificar que la URL de API esté correcta:
   - Frontend debe apuntar a: `http://localhost:8080/api`

4. Probar el endpoint directamente:
   - Abrir Swagger: http://localhost:8080/swagger
   - Probar el endpoint POST `/api/auth/login`

### Backend no responde (unhealthy)

1. Ver logs detallados:
   ```bash
   docker-compose logs backend
   ```

2. Verificar conexión a MongoDB:
   ```bash
   docker-compose logs mongodb
   ```

3. Reiniciar servicios en orden:
   ```bash
   docker-compose restart mongodb
   docker-compose restart backend
   ```

### MongoDB no inicia

1. Verificar si hay otro MongoDB corriendo:
   ```bash
   netstat -ano | findstr :27017
   ```

2. Cambiar el puerto en `docker-compose.yml` si está ocupado

3. Limpiar volúmenes y reiniciar:
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

### Error al enviar la invitación

1. **Asegúrate de llenar el campo de email/username**:
   - Si usas email: escribe un email válido registrado
   - Si usas username: escribe el nombre de usuario exacto

2. **Verifica que el usuario exista**:
   - El usuario debe estar registrado en la aplicación
   - Puedes verificar en Swagger: http://localhost:8080/swagger
   - Endpoint: GET `/api/users` (requiere admin)

3. **Ver logs del backend**:
   ```bash
   docker-compose logs backend --tail=50
   ```

4. **Listar usuarios en MongoDB**:
   ```bash
   docker-compose exec mongodb mongosh -u admin -p password123 --authenticationDatabase admin tasksmanagerbd --eval "db.users.find({}, {email: 1, userName: 1, _id: 0}).pretty()"
   ```

## 📊 Health Checks

Todos los servicios tienen health checks automáticos:

- **MongoDB**: Verifica conectividad con `mongosh`
- **Backend**: Verifica endpoint `/health`
- **Frontend**: Verifica que Nginx responda

Esperar 30-40 segundos después de `docker-compose up` para que todos los servicios estén healthy.

## 🔄 Flujo de Inicio Correcto

1. MongoDB inicia primero y espera estar healthy
2. Backend espera a que MongoDB esté healthy
3. Backend se inicia y crea usuarios por defecto
4. Frontend se inicia y conecta con el backend

## 🌐 Endpoints API Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/refresh` - Refrescar token
- `POST /api/auth/logout` - Cerrar sesión

### Usuarios
- `GET /api/users/profile` - Obtener perfil
- `PUT /api/users/profile` - Actualizar perfil
- `GET /api/users` - Listar usuarios (Admin)

### Boards
- `GET /api/boards` - Listar boards
- `POST /api/boards` - Crear board
- `PUT /api/boards/{id}` - Actualizar board
- `DELETE /api/boards/{id}` - Eliminar board

## 💡 Tips

1. **Primera vez**: Espera 1-2 minutos después de `docker-compose up` para que todos los servicios estén listos

2. **Desarrollo**: Usa `docker-compose logs -f` para ver logs en tiempo real

3. **Limpieza**: Si algo no funciona, prueba:
   ```bash
   docker-compose down -v
   docker-compose up --build
   ```

4. **Performance**: Las imágenes están optimizadas con multi-stage builds y caching

5. **Persistencia**: Los datos de MongoDB persisten en el volumen `mongodb_data`
