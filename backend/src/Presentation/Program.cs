using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DependencyInjection;
using Presentation.Middleware;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar dependencias usando tu sistema SOLID
builder.Services.AddApplicationServices(builder.Configuration);

// Configurar ASP.NET Identity con MongoDB
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Configuración de Identity más flexible para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;

    // Configuración de bloqueo de cuenta más permisiva para desarrollo
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;
})
.AddMongoDbStores<ApplicationUser, ApplicationRole, string>(builder.Configuration.GetConnectionString("MongoDb"), "tasksmanagerbd")
.AddDefaultTokenProviders();

// No necesitamos ApplicationDbContext personalizado ya que ASP.NET Identity maneja MongoDB
// builder.Services.AddSingleton<ApplicationDbContext>();

// Configurar autenticación JWT para compatibilidad con el frontend existente
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["SecretKey"] ?? "TaskManager2025-SecretKey-256bits-Required-For-HS256";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "TaskManager",
        ValidAudience = jwtSettings["Audience"] ?? "TaskManagerUsers",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS para desarrollo permitir llamadas desde el frontend local
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "LocalDev", policy =>
    {
    policy.WithOrigins("http://localhost:3000", "http://localhost:4200", "http://localhost:4201", "http://localhost:4202", "http://localhost:4203", "http://localhost:4204", "http://localhost:4205", "http://localhost:4300", "http://localhost:5003", "http://127.0.0.1:3000", "http://127.0.0.1:4200", "http://127.0.0.1:4201", "http://127.0.0.1:4202", "http://127.0.0.1:4203", "http://127.0.0.1:4204", "http://127.0.0.1:4205", "http://127.0.0.1:4300", "http://127.0.0.1:5003")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configuración de la API
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // Usar camelCase en las respuestas JSON para coincidir con el frontend
    opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "TaskManager API", 
        Version = "v1",
        Description = "API para gestión de tableros y tareas"
    });

    // Configurar el orden de los tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((docName, description) => true);
    c.OrderActionsBy(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.RelativePath}");
    
    // Configurar Swagger para JWT y DTOs
    c.CustomSchemaIds(type => type.FullName);

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// Configurar URLs (comentado para Docker - usa ASPNETCORE_URLS del entorno)
// builder.WebHost.UseUrls("http://localhost:5003");

var app = builder.Build();


// Middleware de excepciones de validación (FluentValidation)
app.UseMiddleware<Presentation.Middleware.ValidationExceptionMiddleware>();

// Middleware de excepciones global (genérico)
app.UseMiddleware<Presentation.Middleware.ExceptionMiddleware>();

// Middleware - ORDEN IMPORTANTE
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskManager API V1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Documentación interactiva de TaskManager API";
    c.DefaultModelsExpandDepth(-1); // Oculta el esquema de modelos por defecto
});

// Agregar autenticación y autorización
// Habilitar CORS antes de la autenticación/autorization para permitir llamadas desde el frontend
app.UseCors("LocalDev");

app.UseAuthentication();
app.UseAuthorization();

// Endpoints básicos (excluidos de Swagger)
app.MapGet("/", () => "TaskManager API - Funcionando ✅ con JWT")
    .ExcludeFromDescription();
app.MapGet("/health", () => new { status = "OK", timestamp = DateTime.Now })
    .ExcludeFromDescription();

// Mapear todos tus controllers (Auth, Tasks, Boards, Users)
app.MapControllers();

// Función para crear usuario administrador con ASP.NET Identity
async Task CreateAdminUserAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        try
        {
            // Crear rol Admin si no existe
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new ApplicationRole("Admin", "Administrador del sistema");
                await roleManager.CreateAsync(adminRole);
                Console.WriteLine("👑 Rol 'Admin' creado");
            }

            // Crear rol User si no existe
            if (!await roleManager.RoleExistsAsync("User"))
            {
                var userRole = new ApplicationRole("User", "Usuario regular del sistema");
                await roleManager.CreateAsync(userRole);
                Console.WriteLine("👤 Rol 'User' creado");
            }

            // Verificar si ya existe un administrador
            var existingAdmin = await userManager.FindByNameAsync("admin");
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser("admin", "admin@taskmanager.com", "Admin", "User")
                {
                    Role = Domain.Enums.UserRole.Admin,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("👑 Usuario administrador creado: admin / Admin123");
                }
                else
                {
                    Console.WriteLine($"❌ Error creando usuario admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("👑 Usuario administrador ya existe");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error al crear/verificar usuario administrador: {ex.Message}");
            Console.WriteLine("Continuando sin usuario administrador...");
        }
    }
}

async Task CreateTestUserAsync(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        try
        {
            // Verificar si ya existe el usuario de prueba
            var existingTestUser = await userManager.FindByEmailAsync("testuser@test.com");
            if (existingTestUser == null)
            {
                var testUser = new ApplicationUser("testuser", "testuser@test.com", "Test", "User")
                {
                    Role = Domain.Enums.UserRole.User,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(testUser, "Test123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(testUser, "User");
                    Console.WriteLine("🧪 Usuario de prueba creado: testuser@test.com / Test123");
                }
                else
                {
                    Console.WriteLine($"❌ Error creando usuario de prueba: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("🧪 Usuario de prueba ya existe");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error al crear/verificar usuario de prueba: {ex.Message}");
            Console.WriteLine("Continuando sin usuario de prueba...");
        }
    }
}

// Crear usuario administrador por defecto
try
{
    await CreateAdminUserAsync(app);
}
catch (Exception ex)
{
    Console.WriteLine($"Error crítico al crear usuario admin: {ex.Message}");
    Console.WriteLine("La aplicación podría no funcionar correctamente sin usuario admin");
}

// Crear usuario de prueba para testing
try
{
    await CreateTestUserAsync(app);
}
catch (Exception ex)
{
    Console.WriteLine($"Error crítico al crear usuario de prueba: {ex.Message}");
    Console.WriteLine("Continuando sin usuario de prueba...");
}

Console.WriteLine("🚀 TaskManager API iniciado en: http://localhost:5003");
Console.WriteLine("📖 Swagger disponible en: http://localhost:5003/swagger");
Console.WriteLine("🔐 ASP.NET Identity con JWT configurado");

app.Run();
