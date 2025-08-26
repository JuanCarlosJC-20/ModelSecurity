using Business;
using Data;
using Entity.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// === Swagger con soporte de JWT ===
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Security API", Version = "v1" });
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// === Configurar JWT Authentication ===
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

// === Configuración de Factory de DbContext para múltiples bases de datos ===
builder.Services.AddDbContextFactory<SqlServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection")));

builder.Services.AddDbContextFactory<PostgresDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDb")));

builder.Services.AddDbContextFactory<MySqlDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))));

// === Registrar servicios de Business ===
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

// === Registro de repositorios genéricos ===
// SQL Server
builder.Services.AddScoped<FormData<SqlServerDbContext>>();
builder.Services.AddScoped<FormModuleData<SqlServerDbContext>>();
builder.Services.AddScoped<ModuleData<SqlServerDbContext>>();
builder.Services.AddScoped<PermissionData<SqlServerDbContext>>();
builder.Services.AddScoped<PersonData<SqlServerDbContext>>();
builder.Services.AddScoped<RolData<SqlServerDbContext>>();
builder.Services.AddScoped<RolFormPermissionData<SqlServerDbContext>>();
builder.Services.AddScoped<RolUserData<SqlServerDbContext>>();
builder.Services.AddScoped<UserData<SqlServerDbContext>>();
builder.Services.AddScoped<AuthData<SqlServerDbContext>>();

// PostgreSQL
builder.Services.AddScoped<FormData<PostgresDbContext>>();
builder.Services.AddScoped<FormModuleData<PostgresDbContext>>();
builder.Services.AddScoped<ModuleData<PostgresDbContext>>();
builder.Services.AddScoped<PermissionData<PostgresDbContext>>();
builder.Services.AddScoped<PersonData<PostgresDbContext>>();
builder.Services.AddScoped<RolData<PostgresDbContext>>();
builder.Services.AddScoped<RolFormPermissionData<PostgresDbContext>>();
builder.Services.AddScoped<RolUserData<PostgresDbContext>>();
builder.Services.AddScoped<UserData<PostgresDbContext>>();
builder.Services.AddScoped<AuthData<PostgresDbContext>>();

// MySQL
builder.Services.AddScoped<FormData<MySqlDbContext>>();
builder.Services.AddScoped<FormModuleData<MySqlDbContext>>();
builder.Services.AddScoped<ModuleData<MySqlDbContext>>();
builder.Services.AddScoped<PermissionData<MySqlDbContext>>();
builder.Services.AddScoped<PersonData<MySqlDbContext>>();
builder.Services.AddScoped<RolData<MySqlDbContext>>();
builder.Services.AddScoped<RolFormPermissionData<MySqlDbContext>>();
builder.Services.AddScoped<RolUserData<MySqlDbContext>>();
builder.Services.AddScoped<UserData<MySqlDbContext>>();
builder.Services.AddScoped<AuthData<MySqlDbContext>>();

// Configurar política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Middleware para Swagger siempre activo (Producción y Desarrollo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Security API V1");
});

// Usar CORS antes de Authorization
app.UseCors("AllowAll");

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();