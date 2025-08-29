using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Entity.Context
{
    public class PostgresDbContext : ApplicationDbContext<PostgresDbContext>
    {
        public PostgresDbContext(DbContextOptions<PostgresDbContext> options, IConfiguration config)
            : base(options, config) { }
    }
}
