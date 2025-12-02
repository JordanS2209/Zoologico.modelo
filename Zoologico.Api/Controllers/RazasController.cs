using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zoologico.modelo;

namespace Zoologico.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RazasController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlcontext;
        private readonly PostgresDbContext _pgContext;

        public RazasController(SqlServerDbContext sqlcontext, PostgresDbContext pgContext)
        {
            _sqlcontext = sqlcontext;
            _pgContext = pgContext;
        }

        // GET: api/Razas (combina ambas bases)
        [HttpGet]
        public async Task<ActionResult<ApiResult<List<Raza>>>> GetRaza()
        {
            try
            {
                Console.WriteLine("Obteniendo datos de SQL Server...");
                var dataSql = await _sqlcontext.Razas
                    .Include(r => r.Animales)
                    .ToListAsync();

                Console.WriteLine("Obteniendo datos de PostgreSQL...");
                var dataPg = await _pgContext.Razas
                    .Include(r => r.Animales)
                    .ToListAsync();

                var allData = dataSql.Concat(dataPg).ToList();
                return ApiResult<List<Raza>>.Ok(allData);
            }
            catch (Exception ex)
            {
                return ApiResult<List<Raza>>.Fail(ex.Message);
            }
        }

        // GET: api/Razas/5 (busca en ambas bases, prioriza SQL Server)
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Raza>>> GetRaza(int id)
        {
            try
            {
                var razaSql = await _sqlcontext.Razas
                    .Include(e => e.Animales)
                    .FirstOrDefaultAsync(e => e.Id == id);

                var razaPg = await _pgContext.Razas
                    .Include(e => e.Animales)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (razaSql == null && razaPg == null)
                {
                    return ApiResult<Raza>.Fail("Datos no encontrados en ninguna base de datos");
                }

                return ApiResult<Raza>.Ok(razaSql ?? razaPg);
            }
            catch (Exception ex)
            {
                return ApiResult<Raza>.Fail(ex.Message);
            }
        }

        // PUT: api/Razas/5 (actualiza en ambas bases)
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult<Raza>>> PutRaza(int id, Raza raza)
        {
            if (id != raza.Id)
            {
                return ApiResult<Raza>.Fail("Identificador no coincide");
            }

            _sqlcontext.Entry(raza).State = EntityState.Modified;
            _pgContext.Entry(raza).State = EntityState.Modified;

            try
            {
                await _sqlcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RazaExistsSql(id))
                {
                    return ApiResult<Raza>.Fail("Datos no encontrados en SQL Server");
                }
                else
                {
                    return ApiResult<Raza>.Fail(ex.Message);
                }
            }

            try
            {
                await _pgContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!RazaExistsPg(id))
                {
                    return ApiResult<Raza>.Fail("Datos no encontrados en PostgreSQL");
                }
                else
                {
                    return ApiResult<Raza>.Fail(ex.Message);
                }
            }

            return ApiResult<Raza>.Ok(null);
        }

        // POST: api/Razas (crea en ambas bases)
        [HttpPost]
        public async Task<ActionResult<ApiResult<Raza>>> PostRaza(Raza raza)
        {
            // Guardar en SQL Server
            try
            {
                _sqlcontext.Razas.Add(raza);
                await _sqlcontext.SaveChangesAsync();
            }
            catch (Exception exSql)
            {
                return ApiResult<Raza>.Fail($"Error en SQL Server: {exSql.Message}");
            }

            // Guardar en PostgreSQL (nuevo objeto para evitar conflicto de ID)
            try
            {
                var razaPg = new Raza
                {
                    Nombre = raza.Nombre
                };
                _pgContext.Razas.Add(razaPg);
                await _pgContext.SaveChangesAsync();
            }
            catch (Exception exPg)
            {
                return ApiResult<Raza>.Fail($"Error en PostgreSQL: {exPg.Message}");
            }

            return ApiResult<Raza>.Ok(raza);
        }

        // DELETE: api/Razas/5 (elimina en ambas bases)
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResult<Raza>>> DeleteRaza(int id)
        {
            try
            {
                var razaSql = await _sqlcontext.Razas.FindAsync(id);
                var razaPg = await _pgContext.Razas.FindAsync(id);

                if (razaSql == null && razaPg == null)
                {
                    return ApiResult<Raza>.Fail("Datos no encontrados en ninguna base de datos");
                }

                if (razaSql != null)
                {
                    _sqlcontext.Razas.Remove(razaSql);
                    await _sqlcontext.SaveChangesAsync();
                }

                if (razaPg != null)
                {
                    _pgContext.Razas.Remove(razaPg);
                    await _pgContext.SaveChangesAsync();
                }

                return ApiResult<Raza>.Ok(null);
            }
            catch (Exception ex)
            {
                return ApiResult<Raza>.Fail(ex.Message);
            }
        }

        private bool RazaExistsSql(int id)
        {
            return _sqlcontext.Razas.Any(e => e.Id == id);
        }

        private bool RazaExistsPg(int id)
        {
            return _pgContext.Razas.Any(e => e.Id == id);
        }
    }
}