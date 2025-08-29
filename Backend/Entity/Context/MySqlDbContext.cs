using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Entity.Context
{
    public class MySqlDbContext : ApplicationDbContext<MySqlDbContext>
    {
        public MySqlDbContext(DbContextOptions<MySqlDbContext> options, IConfiguration config)
            : base(options, config) { }
    }
}
