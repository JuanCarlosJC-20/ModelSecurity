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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mi API", Version = "v1" });

    // Configuración para que Swagger acepte JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Introduce el token JWT con el formato: Bearer {tu_token}",
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
            new string[] {}
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
    options.RequireHttpsMetadata = false; // true en producción con HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// === Registrar tus servicios ===
// Rol
builder.Services.AddScoped<RolData>();
builder.Services.AddScoped<RolBusiness>();

// RolUser
builder.Services.AddScoped<RolUserData>();
builder.Services.AddScoped<RolUserBusiness>();

// User
builder.Services.AddScoped<UserData>();
builder.Services.AddScoped<UserBusiness>();

// === Auth (nuevo, separado de User) ===
builder.Services.AddScoped<AuthData>();
builder.Services.AddScoped<AuthBusiness>();

// Form
builder.Services.AddScoped<FormData>();
builder.Services.AddScoped<FormBusiness>();

// FormModule
builder.Services.AddScoped<FormModuleData>();
builder.Services.AddScoped<FormModuleBusiness>();

// Module
builder.Services.AddScoped<ModuleData>();
builder.Services.AddScoped<ModuleBusiness>();

// Permission
builder.Services.AddScoped<PermissionData>();
builder.Services.AddScoped<PermissionBusiness>();

// Person
builder.Services.AddScoped<PersonData>();
builder.Services.AddScoped<PersonBusiness>();

// RolFormPermission
builder.Services.AddScoped<RolFormPermissionData>();
builder.Services.AddScoped<RolFormPermissionBusiness>();

// Agregar DbContext
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseSqlServer("name=DefaultConnection"));

// Configurar política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("*") // cambia si tu frontend está en otro host
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware para Swagger siempre activo (Producción y Desarrollo)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mi API v1");
    c.RoutePrefix = string.Empty; // Muestra Swagger en la raíz "/"
});

app.UseHttpsRedirection();

// Usar CORS antes de Authorization
app.UseCors("PermitirFrontend");

// Habilitar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
