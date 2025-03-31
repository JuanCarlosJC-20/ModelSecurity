using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;

namespace Entity.Context
{
 
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Configuración de la aplicación.
        /// </summary>
        protected readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor del contexto de la base de datos.
        /// </summary>
        /// <param name="options">Opciones de configuración para el contexto de base de datos.</param>
        /// <param name="configuration">Instancia de IConfiguration para acceder a la configuración de la aplicación.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// Configura los modelos de la base de datos aplicando configuraciones desde ensamblados.
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo de base de datos</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        ///<summary>
        ///Configura opciones adicionales del contexto, como el registro de datos sensibles
        /// </summary>
        /// <param name="optionsBuilder">Constructor de opciones de confu¿iguracion del contexto</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            //otras configuraciones adicionales pueden ir aqui
        }

        ///<summary>
        ///Configura convenciones de tipos de datos, estableciendo la precision por defecto de los valores decimales
        /// </summary>
        /// <param name="configurationBuilder">Constructor de la configurcion de modelos</param>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

        ///<summary>
        ///Guarda los cambios en la base de datos, asegurando la auditoria antes de persistir los datos
        /// </summary>
        /// <return>Numero de filas afectadas</return>
        public override int SaveChanges()
        {
            EnsureAudit();
            return base.SaveChanges();
        }
        ///<summary>
        ///Guarda los cambios en la base de datos de manera asincrona, asegurando la auditoria antes de la persistencia
        /// </summary>
        /// <param name="acceptAAllChangesOnSuccess">Indica si se deben aceptar todos los cambios en caso de exito</param>
        /// <param name="cancellationToken">Token de cancelación para abortar la operación.</param>
        /// <returns>Numero de filas afectadas de forma asincrona</returns>
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            EnsureAudit();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        ///<summary>
        ///Ejecuta una consulta SQL utilizando Dapper y devuelve una coleccion de resultados de tipo gemerico
        /// </summary>
        /// <typeparam name="T">Tipo de los datos de retorno</typeparam>
        /// <param name="text">Consulta SQL a ejecutar</param>
        /// <param name="parameters">Parametros opcionales de la consulta</param>
        /// <param name="timeout">Tiempo de espera opcional para la consulta</param>
        /// <param name="type">Tipo opcional de comando SQL</param>
        /// <returns>Una coleccion de objetos del tipo especificado</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(string text, object parameters = null, int? timeout = null, CommandType?  type = null)
        {
            using var command = new DapperEFCoreCommand(this, text, parameters, timeout, type, CancellationToken.None);
            var connection = this.Database.GetDbConnection();
            return await connection.QueryAsync<T>(command.Definition);
        }

        ///<summary>
        ///Ejecuta una consulta SQl utilizando Dapper y devuelve un solo resultado o el valor predeterminado si no hay resultados
        /// </summary>
        /// <typeparam name="T">Tipo de los datos de retorno</typeparam>
        /// <param name="text">Consulta SQL a ejecutar</param>
        /// <param name="parameters">Parametros opcionales de la consulta</param>
        /// <param name="timeout">Tiempo de espera opcional para la consulta</param>
        /// <param name="type">Tipo opcional de comando SQL</param>
        /// <returns>Un objeto del tipo especificado o su valor predetermiando</returns>
        public async Task<T> QueryFirstOrDefaultAsync<T>(string text, object parameters = null, int? timeout = null, CommandType? type = null)
        {
            using var command = new DapperEFCoreCommand(this, text, parameters, timeout, type, CancellationToken.None);
            var connection = this.Database.GetDbConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(command: command.Definition);
        }
        ///<summary>
        ///Metodo interno para garantizar la autoria de los cambios en las entidades
        /// </summary>
        private void EnsureAudit()
        {
            ChangeTracker.DetectChanges();
        }

        ///<summary>
        ///Estructura para ejecutar comandos SQL con Dapper en Entity Framework Core
        /// </summary>
        public readonly struct DapperEFCoreCommand : IDisposable
        {
            ///<summary>
            ///Constructor del comando Dapper
            ///</summary>
            ///<param name="context">Contexto de la base de datos</param>
            ///<param name="text">Consulta SQL</param>
            ///<param name="parameters">Parametros opcionales</param>
            ///<param name="timeout">Tiempo de espera opcional</param>
            ///<param name="type">Tipo de comando >SQL opcional</param>
            ///<param name="ct">Token de cancelacion</param>
            public DapperEFCoreCommand(DbContext context, string text, object parameters, int? timeout, CommandType? type, CancellationToken ct)
            {
                var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
                var commandType = type ?? CommandType.Text;
                var commandTimeout = timeout ?? context.Database.GetCommandTimeout() ?? 30;

                Definition = new CommandDefinition(text, parameters, transaction, commandTimeout, commandType, cancellationToken: ct);
            }

            ///<summary>
            ///Define los parametros del comando SQL
            ///</summary>
            public CommandDefinition Definition { get; }

            ///<summary>
            ///Metodo para liberar los recursos
            ///</summary>
            public void Dispose()
            {
            }
        }

    }
}
