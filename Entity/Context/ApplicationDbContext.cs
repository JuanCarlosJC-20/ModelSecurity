using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Reflection;

namespace Entity.Context
{
    ///<summary>
    ///Representa el contexto de la base de datos de la aplicacion, proporcionando configuraciones y metedos
    ///para la gestion de entidades y consultas personalizadas con Dapper
    ///</summary>
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
        protected override void OnModuleCreaating(ModelBuilder modelBuilder)
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

    }
}
