# Application.Tests

Proyecto de pruebas unitarias para el backend de TaskManager.

## 📦 Paquetes Instalados

- **xUnit**: Framework de testing principal
- **Moq 4.20.72**: Biblioteca de mocking para simular dependencias
- **FluentAssertions 8.7.1**: Assertions más legibles y descriptivas
- **coverlet.collector**: Recopilación de cobertura de código
- **Microsoft.Extensions.DependencyInjection**: Inyección de dependencias

## 🎯 Áreas de Testing

### 1. Handlers de Notificaciones
- **GetUserNotificationsQueryHandler**: Obtención de notificaciones con filtros
- **MarkNotificationAsReadCommandHandler**: Marcar notificaciones como leídas

### 2. Handlers de Invitaciones
- **RespondToBoardInvitationCommandHandler**: Responder a invitaciones y crear notificaciones

## 🚀 Cómo Ejecutar los Tests

### Ejecutar todos los tests
```powershell
dotnet test
```

### Ejecutar tests con coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Ejecutar un test específico
```powershell
dotnet test --filter "FullyQualifiedName~GetUserNotificationsQueryHandlerTests"
```

### Ver resultados detallados
```powershell
dotnet test --logger "console;verbosity=detailed"
```

## 📊 Estructura de Tests

```
Application.Tests/
├── Handlers/
│   ├── Notifications/
│   │   ├── GetUserNotificationsQueryHandlerTests.cs
│   │   └── MarkNotificationAsReadCommandHandlerTests.cs
│   └── Boards/
│       └── RespondToBoardInvitationCommandHandlerTests.cs
└── Application.Tests.csproj
```

## ⚠️ Notas Importantes

### Limitaciones Actuales con Mocking

Los tests actuales tienen problemas de compilación debido a que los repositorios son **clases concretas** que heredan de `GenericRepository` y tienen dependencias de MongoDB:

```csharp
public class NotificationRepository : GenericRepository<Notification>
public class UserRepository : GenericRepository<ApplicationUser>
public class BoardRepository : GenericRepository<Board>
```

**Problema**: Moq no puede crear mocks de clases concretas que:
1. No tienen constructores públicos sin parámetros
2. Tienen dependencias de MongoDB (IMongoCollection)
3. No están marcadas como `virtual` para override

### Soluciones Propuestas

#### Opción 1: Refactorizar a Interfaces (RECOMENDADO)
Crear interfaces para los repositorios:

```csharp
// Domain/Interfaces/INotificationRepository.cs
public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(string id);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool onlyUnread = false, int limit = 50);
    Task<Notification> CreateAsync(Notification entity);
    Task<Notification> UpdateAsync(Notification entity);
    // ... otros métodos
}

// Infrastructure/Repositories/NotificationRepository.cs
public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    // Implementación existente
}
```

#### Opción 2: Tests de Integración
Usar MongoDB en memoria o TestContainers para tests de integración reales:

```csharp
// Usar MongoDB.Driver.Core.TestHelpers o Testcontainers
using Testcontainers.MongoDb;

public class NotificationRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly MongoDbContainer _mongoContainer;
    
    public async Task InitializeAsync()
    {
        await _mongoContainer.StartAsync();
    }
}
```

#### Opción 3: Tests de Contrato
Verificar el comportamiento esperado sin mocks:

```csharp
[Theory]
[InlineData("user123", true, 10)]
[InlineData("user456", false, 50)]
public void GetUserNotificationsAsync_ShouldAcceptCorrectParameters(
    string userId, bool onlyUnread, int limit)
{
    // Verificar que el método existe con la firma correcta
    var method = typeof(NotificationRepository)
        .GetMethod("GetUserNotificationsAsync");
    
    method.Should().NotBeNull();
    method.ReturnType.Should().Be(typeof(Task<IEnumerable<Notification>>));
}
```

## 🔧 Estado Actual

❌ **Tests no compilan** debido a problemas con mocking de clases concretas

Para hacer que los tests funcionen, se necesita:
1. Refactorizar repositorios para usar interfaces
2. O cambiar a tests de integración con MongoDB real/contenedor
3. O simplificar tests para verificar contratos sin ejecución

## 📝 Casos de Prueba Documentados

### GetUserNotificationsQueryHandler

1. ✅ **Handle_ShouldReturnAllNotifications_WhenNoFiltersApplied**
   - Verifica que devuelve todas las notificaciones sin filtros

2. ✅ **Handle_ShouldReturnOnlyUnreadNotifications_WhenUnreadOnlyIsTrue**
   - Verifica filtro de solo no leídas

3. ✅ **Handle_ShouldRespectLimit_WhenLimitIsSpecified**
   - Verifica que respeta el límite especificado

4. ✅ **Handle_ShouldReturnEmptyList_WhenNoNotificationsExist**
   - Verifica comportamiento con lista vacía

5. ✅ **Handle_ShouldMapNotificationPropertiesCorrectly**
   - Verifica mapeo correcto de propiedades

### MarkNotificationAsReadCommandHandler

1. ✅ **Handle_ShouldMarkNotificationAsRead_WhenValidRequest**
   - Verifica marcado de notificación como leída

2. ✅ **Handle_ShouldThrowKeyNotFoundException_WhenNotificationDoesNotExist**
   - Verifica excepción cuando no existe

3. ✅ **Handle_ShouldThrowUnauthorizedAccessException_WhenUserDoesNotOwnNotification**
   - Verifica autorización

4. ✅ **Handle_ShouldReturnTrue_WhenNotificationAlreadyRead**
   - Verifica idempotencia

### RespondToBoardInvitationCommandHandler

1. ✅ **Handle_ShouldCreateNotification_WhenInvitationIsAccepted**
   - Verifica creación de notificación al aceptar

2. ✅ **Handle_ShouldCreateNotification_WhenInvitationIsRejected**
   - Verifica creación de notificación al rechazar

3. ✅ **Handle_ShouldThrowKeyNotFoundException_WhenInvitationDoesNotExist**
   - Verifica excepción con invitación inexistente

4. ✅ **Handle_ShouldThrowUnauthorizedException_WhenUserIsNotInvitee**
   - Verifica autorización

5. ✅ **Handle_ShouldThrowInvalidOperationException_WhenInvitationAlreadyResponded**
   - Verifica que no se puede responder dos veces

## 📚 Referencias

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4)
- [FluentAssertions](https://fluentassertions.com/)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
