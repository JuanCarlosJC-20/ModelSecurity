using Entity.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class MultiDatabaseService
    {
        private readonly IDbContextFactory<SqlServerDbContext> _sqlServerFactory;
        private readonly IDbContextFactory<PostgresDbContext> _postgresFactory;
        private readonly IDbContextFactory<MySqlDbContext> _mySqlFactory;
        private readonly ILogger<MultiDatabaseService> _logger;

        public MultiDatabaseService(
            IDbContextFactory<SqlServerDbContext> sqlServerFactory,
            IDbContextFactory<PostgresDbContext> postgresFactory,
            IDbContextFactory<MySqlDbContext> mySqlFactory,
            ILogger<MultiDatabaseService> logger)
        {
            _sqlServerFactory = sqlServerFactory;
            _postgresFactory = postgresFactory;
            _mySqlFactory = mySqlFactory;
            _logger = logger;
        }

        public async Task<T> ExecuteWithFailoverAsync<T>(Func<DbContext, Task<T>> operation)
        {
            var databases = GetDatabaseContexts();
            Exception lastException = null;

            foreach (var (dbName, factory) in databases)
            {
                try
                {
                    _logger.LogInformation("🔄 Intentando ejecutar operación en {DbName}", dbName);
                    using var context = factory();
                    var result = await operation(context);
                    _logger.LogInformation("✅ Operación exitosa en {DbName}", dbName);
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("❌ Error en {DbName}: {Error}", dbName, ex.Message);
                    lastException = ex;
                    continue;
                }
            }

            _logger.LogError("🚫 Todas las bases de datos fallaron");
            throw new Exception("Todas las bases de datos están inaccesibles", lastException);
        }

        public async Task ExecuteInAllDatabasesAsync(Func<DbContext, Task> operation)
        {
            var databases = GetDatabaseContexts();
            var tasks = new List<Task>();
            var successCount = 0;
            var errors = new List<string>();

            foreach (var (dbName, factory) in databases)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("🔄 Ejecutando operación en {DbName}", dbName);
                        using var context = factory();
                        await operation(context);
                        _logger.LogInformation("✅ Operación exitosa en {DbName}", dbName);
                        Interlocked.Increment(ref successCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("❌ Error en {DbName}: {Error}", dbName, ex.Message);
                        lock (errors)
                        {
                            errors.Add($"{dbName}: {ex.Message}");
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);

            if (successCount == 0)
            {
                _logger.LogError("🚫 No se pudo ejecutar la operación en ninguna base de datos");
                throw new Exception($"Falló en todas las bases de datos: {string.Join("; ", errors)}");
            }

            if (errors.Any())
            {
                _logger.LogWarning("⚠️ Operación completada parcialmente. Éxitos: {Success}, Errores: {Errors}", 
                    successCount, errors.Count);
            }
            else
            {
                _logger.LogInformation("✅ Operación completada exitosamente en todas las bases de datos ({Count})", successCount);
            }
        }

        public async Task<T> ExecuteInAllDatabasesAsync<T>(Func<DbContext, Task<T>> operation)
        {
            var databases = GetDatabaseContexts();
            var tasks = new List<Task<T>>();
            T result = default(T);
            var successCount = 0;
            var errors = new List<string>();

            foreach (var (dbName, factory) in databases)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation("🔄 Ejecutando operación en {DbName}", dbName);
                        using var context = factory();
                        var dbResult = await operation(context);
                        _logger.LogInformation("✅ Operación exitosa en {DbName}", dbName);
                        
                        if (result == null || result.Equals(default(T)))
                        {
                            result = dbResult;
                        }
                        
                        Interlocked.Increment(ref successCount);
                        return dbResult;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("❌ Error en {DbName}: {Error}", dbName, ex.Message);
                        lock (errors)
                        {
                            errors.Add($"{dbName}: {ex.Message}");
                        }
                        throw;
                    }
                }));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                return results.FirstOrDefault();
            }
            catch when (successCount > 0)
            {
                _logger.LogWarning("⚠️ Operación completada parcialmente. Éxitos: {Success}, Errores: {Errors}", 
                    successCount, errors.Count);
                return result;
            }
        }

        private List<(string Name, Func<DbContext> Factory)> GetDatabaseContexts()
        {
            return new List<(string, Func<DbContext>)>
            {
                ("SQL Server", () => _sqlServerFactory.CreateDbContext()),
                ("PostgreSQL", () => _postgresFactory.CreateDbContext()),
                ("MySQL", () => _mySqlFactory.CreateDbContext())
            };
        }

        public async Task<bool> TestDatabaseConnectionAsync(string databaseName)
        {
            var databases = GetDatabaseContexts();
            var database = databases.FirstOrDefault(d => d.Name.Equals(databaseName, StringComparison.OrdinalIgnoreCase));
            
            if (database.Factory == null)
                return false;

            try
            {
                using var context = database.Factory();
                return await context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<Dictionary<string, bool>> GetDatabaseStatusAsync()
        {
            var databases = GetDatabaseContexts();
            var status = new Dictionary<string, bool>();
            var tasks = new List<Task>();

            foreach (var (dbName, factory) in databases)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var context = factory();
                        var canConnect = await context.Database.CanConnectAsync();
                        lock (status)
                        {
                            status[dbName] = canConnect;
                        }
                    }
                    catch
                    {
                        lock (status)
                        {
                            status[dbName] = false;
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return status;
        }
    }
}