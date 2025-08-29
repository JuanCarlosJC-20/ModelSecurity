using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Entity.Context
{
    public class MySqlDbContextFactory : IDesignTimeDbContextFactory<MySqlDbContext>
    {
        public MySqlDbContext CreateDbContext(string[] args)
        {
            // Cargar configuración desde appsettings.json (opcional)
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // ⚡ Dummy connection string para migraciones
            var connectionString = "Server=127.0.0.1;Port=3306;Database=TempDb;User=root;Password=1234567;";

            var optionsBuilder = new DbContextOptionsBuilder<MySqlDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            // Retorna el DbContext con IConfiguration (aunque no se use la DB real)
            return new MySqlDbContext(optionsBuilder.Options, config);
        }
    }
}
