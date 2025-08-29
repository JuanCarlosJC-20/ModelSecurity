using Entity.Model;
using Microsoft.AspNetCore.Mvc;
using Entity.Context;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MultiDbController : ControllerBase
    {
        private readonly SqlServerDbContext _sql;
        private readonly PostgresDbContext _pg;
        private readonly MySqlDbContext _my;

        public MultiDbController(SqlServerDbContext sql, PostgresDbContext pg, MySqlDbContext my)
        {
            _sql = sql;
            _pg = pg;
            _my = my;
        }

        [HttpPost("saveUser")]
        public async Task<IActionResult> SaveUser(User user)
        {
            try
            {
                _sql.User.Add(user);
                _pg.User.Add(user);
                _my.User.Add(user);

                await _sql.SaveChangesAsync();
                await _pg.SaveChangesAsync();
                await _my.SaveChangesAsync();

                return Ok("Usuario guardado en las 3 bases 🚀");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al guardar: {ex.Message}");
            }
        }
    }
}