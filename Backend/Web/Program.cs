using Business;
using Business.Services;
using Data;
using Entity.Context;
using Entity.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// === Swagger con JWT ===
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Security API", Version = "v1" });
    
    // Configurar para manejar múltiples schemas con el mismo nombre
    c.CustomSchemaIds(type => type.FullName);
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// === JWT Authentication ===
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "MiClaveSuperSecreta123!");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// === DbContext Factories ===
builder.Services.AddDbContextFactory<SqlServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddDbContextFactory<PostgresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb"),
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

builder.Services.AddDbContextFactory<MySqlDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection")),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

// === Servicios de negocio ===
builder.Services.AddScoped<MultiDatabaseService>();
builder.Services.AddScoped<AuthBusiness>();
builder.Services.AddScoped<FormBusiness>();
builder.Services.AddScoped<FormModuleBusiness>();
builder.Services.AddScoped<ModuleBusiness>();
builder.Services.AddScoped<PermissionBusiness>();
builder.Services.AddScoped<PersonBusiness>();
builder.Services.AddScoped<RolBusiness>();
builder.Services.AddScoped<RolFormPermissionBusiness>();
builder.Services.AddScoped<RolUserBusiness>();
builder.Services.AddScoped<UserBusiness>();

// === Repositorios genéricos ===
// SQL Server
builder.Services.AddScoped<AuthData<SqlServerDbContext>>();
builder.Services.AddScoped<FormData<SqlServerDbContext>>();
builder.Services.AddScoped<FormModuleData<SqlServerDbContext>>();
builder.Services.AddScoped<ModuleData<SqlServerDbContext>>();
builder.Services.AddScoped<PermissionData<SqlServerDbContext>>();
builder.Services.AddScoped<PersonData<SqlServerDbContext>>();
builder.Services.AddScoped<RolData<SqlServerDbContext>>();
builder.Services.AddScoped<RolFormPermissionData<SqlServerDbContext>>();
builder.Services.AddScoped<RolUserData<SqlServerDbContext>>();
builder.Services.AddScoped<UserData<SqlServerDbContext>>();

// PostgreSQL
builder.Services.AddScoped<AuthData<PostgresDbContext>>();
builder.Services.AddScoped<FormData<PostgresDbContext>>();
builder.Services.AddScoped<FormModuleData<PostgresDbContext>>();
builder.Services.AddScoped<ModuleData<PostgresDbContext>>();
builder.Services.AddScoped<PermissionData<PostgresDbContext>>();
builder.Services.AddScoped<PersonData<PostgresDbContext>>();
builder.Services.AddScoped<RolData<PostgresDbContext>>();
builder.Services.AddScoped<RolFormPermissionData<PostgresDbContext>>();
builder.Services.AddScoped<RolUserData<PostgresDbContext>>();
builder.Services.AddScoped<UserData<PostgresDbContext>>();

// MySQL
builder.Services.AddScoped<AuthData<MySqlDbContext>>();
builder.Services.AddScoped<FormData<MySqlDbContext>>();
builder.Services.AddScoped<FormModuleData<MySqlDbContext>>();
builder.Services.AddScoped<ModuleData<MySqlDbContext>>();
builder.Services.AddScoped<PermissionData<MySqlDbContext>>();
builder.Services.AddScoped<PersonData<MySqlDbContext>>();
builder.Services.AddScoped<RolData<MySqlDbContext>>();
builder.Services.AddScoped<RolFormPermissionData<MySqlDbContext>>();
builder.Services.AddScoped<RolUserData<MySqlDbContext>>();
builder.Services.AddScoped<UserData<MySqlDbContext>>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// === Migraciones automáticas con manejo de errores mejorado ===
async Task MigrateWithRetryAsync<T>(IDbContextFactory<T> factory, string dbName, int retries = 15, int delayMs = 3000) where T : DbContext
{
    Console.WriteLine($"🔄 Iniciando migración para {dbName}...");
    
    for (int i = 0; i < retries; i++)
    {
        try
        {
            Console.WriteLine($"📡 Intento {i + 1}/{retries} para conectar a {dbName}...");
            
            using var db = factory.CreateDbContext();
            
            // Verificar conexión con timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await db.Database.CanConnectAsync(cts.Token);
            
            Console.WriteLine($"✅ Conectado a {dbName}, ejecutando migraciones...");
            
            // Saltar la creación manual de base de datos para SQL Server, usar solo migraciones
            Console.WriteLine($"🔧 Ejecutando migraciones para {dbName}...");
            
            await db.Database.MigrateAsync(cts.Token);
            
            // Ejecutar seeder después de la migración
            Console.WriteLine($"🌱 Ejecutando seeder para {dbName}...");
            await DatabaseSeeder.SeedAsync(db);
            
            Console.WriteLine($"✅ {dbName} migrada correctamente.");
            return;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"⏰ Timeout conectando a {dbName} en intento {i + 1}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Intento {i + 1} fallido para {dbName}: {ex.GetType().Name} - {ex.Message}");
        }
        
        if (i < retries - 1)
        {
            Console.WriteLine($"⏳ Esperando {delayMs}ms antes del siguiente intento...");
            await Task.Delay(delayMs);
        }
    }
    
    Console.WriteLine($"🚫 No se pudo migrar {dbName} después de {retries} intentos. La aplicación continuará sin esta base de datos.");
}

// === Ejecutar migraciones en paralelo ===
Console.WriteLine("🚀 Iniciando migraciones de bases de datos...");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Ejecutar migraciones en paralelo para ser más eficiente
    var migrationTasks = new List<Task>();

    try
    {
        var sqlFactory = services.GetRequiredService<IDbContextFactory<SqlServerDbContext>>();
        migrationTasks.Add(MigrateWithRetryAsync(sqlFactory, "SQL Server"));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ No se pudo configurar SQL Server: {ex.Message}");
    }

    try
    {
        var pgFactory = services.GetRequiredService<IDbContextFactory<PostgresDbContext>>();
        migrationTasks.Add(MigrateWithRetryAsync(pgFactory, "PostgreSQL"));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ No se pudo configurar PostgreSQL: {ex.Message}");
    }

    try
    {
        var mySqlFactory = services.GetRequiredService<IDbContextFactory<MySqlDbContext>>();
        migrationTasks.Add(MigrateWithRetryAsync(mySqlFactory, "MySQL"));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ No se pudo configurar MySQL: {ex.Message}");
    }

    // Esperar que todas las migraciones terminen (exitosas o fallidas)
    await Task.WhenAll(migrationTasks);
    Console.WriteLine("🏁 Proceso de migraciones completado.");
}

// Middleware Swagger y CORS
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Security API V1");
    c.RoutePrefix = string.Empty; // Swagger UI en la raíz
});
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
