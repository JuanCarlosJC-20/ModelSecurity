using Entity.Context;
using Entity.Model;
using Microsoft.EntityFrameworkCore;

namespace Entity.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(DbContext context)
        {
            try
            {
                await SeedRolesAsync(context);
                await SeedPermissionsAsync(context);
                await SeedModulesAsync(context);
                await SeedFormsAsync(context);
                await context.SaveChangesAsync();
                
                Console.WriteLine("✅ Datos iniciales creados correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando datos iniciales: {ex.Message}");
            }
        }

        private static async Task SeedRolesAsync(DbContext context)
        {
            var roles = new List<Rol>
            {
                new Rol
                {
                    Name = "Admin",
                    Description = "Administrador del sistema con todos los permisos",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Rol
                {
                    Name = "User",
                    Description = "Usuario estándar con permisos básicos",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                }
            };

            foreach (var rol in roles)
            {
                var existingRol = await context.Set<Rol>()
                    .FirstOrDefaultAsync(r => r.Name == rol.Name);
                
                if (existingRol == null)
                {
                    context.Set<Rol>().Add(rol);
                    Console.WriteLine($"🔧 Creando rol: {rol.Name}");
                }
            }
        }

        private static async Task SeedPermissionsAsync(DbContext context)
        {
            var permissions = new List<Permission>
            {
                new Permission
                {
                    Name = "Leer",
                    Code = "READ",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Escribir",
                    Code = "WRITE",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Actualizar",
                    Code = "UPDATE",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Eliminar",
                    Code = "DELETE",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Permission
                {
                    Name = "Administrar",
                    Code = "ADMIN",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                }
            };

            foreach (var permission in permissions)
            {
                var existingPermission = await context.Set<Permission>()
                    .FirstOrDefaultAsync(p => p.Code == permission.Code);
                
                if (existingPermission == null)
                {
                    context.Set<Permission>().Add(permission);
                    Console.WriteLine($"🔧 Creando permiso: {permission.Name}");
                }
            }
        }

        private static async Task SeedModulesAsync(DbContext context)
        {
            var modules = new List<Module>
            {
                new Module
                {
                    Name = "Usuarios",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Module
                {
                    Name = "Roles",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Module
                {
                    Name = "Permisos",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Module
                {
                    Name = "Configuración",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                }
            };

            foreach (var module in modules)
            {
                var existingModule = await context.Set<Module>()
                    .FirstOrDefaultAsync(m => m.Name == module.Name);
                
                if (existingModule == null)
                {
                    context.Set<Module>().Add(module);
                    Console.WriteLine($"🔧 Creando módulo: {module.Name}");
                }
            }
        }

        private static async Task SeedFormsAsync(DbContext context)
        {
            var forms = new List<Form>
            {
                new Form
                {
                    Name = "Login",
                    Code = "LOGIN_FORM",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Form
                {
                    Name = "Registro",
                    Code = "REGISTER_FORM",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Form
                {
                    Name = "Gestión de Usuarios",
                    Code = "USER_MANAGEMENT_FORM",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                },
                new Form
                {
                    Name = "Gestión de Roles",
                    Code = "ROLE_MANAGEMENT_FORM",
                    Active = true,
                    CreateAt = DateTime.UtcNow
                }
            };

            foreach (var form in forms)
            {
                var existingForm = await context.Set<Form>()
                    .FirstOrDefaultAsync(f => f.Code == form.Code);
                
                if (existingForm == null)
                {
                    context.Set<Form>().Add(form);
                    Console.WriteLine($"🔧 Creando formulario: {form.Name}");
                }
            }
        }
    }
}