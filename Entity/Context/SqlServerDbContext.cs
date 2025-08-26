using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Entity.Context
{
    public class SqlServerDbContext : ApplicationDbContext<SqlServerDbContext>
    {
        public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options, IConfiguration config)
            : base(options, config) { }
    }
}
