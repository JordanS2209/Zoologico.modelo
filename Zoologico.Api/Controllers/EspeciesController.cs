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
    public class EspeciesController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlcontext;
        private readonly PostgresDbContext _pgContext;

        public EspeciesController(SqlServerDbContext sqlcontext, PostgresDbContext pgContext)
        {
            _sqlcontext = sqlcontext;
            _pgContext = pgContext;
        }

        // GET: api/Especies (combina ambas bases)
        [HttpGet]
        public async Task<ActionResult<ApiResult<List<Especie>>>> GetEspecie()
        {
            try
            {
                Console.WriteLine("Obteniendo datos de SQL Server...");
                var dataSql = await _sqlcontext.Especies
                    .Include(e => e.Animales)
                    .ToListAsync();

                Console.WriteLine("Obteniendo datos de PostgreSQL...");
                var dataPg = await _pgContext.Especies
                    .Include(e => e.Animales)
                    .ToListAsync();

                var allData = dataSql.Concat(dataPg).ToList();
                return ApiResult<List<Especie>>.Ok(allData);
            }
            catch (Exception ex)
            {
                return ApiResult<List<Especie>>.Fail(ex.Message);
            }
        }

        // GET: api/Especies/5 (busca en ambas bases, prioriza SQL Server)
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Especie>>> GetEspecie(int id)
        {
            try
            {
                var especieSql = await _sqlcontext.Especies
                    .Include(e => e.Animales)
                    .FirstOrDefaultAsync(e => e.Id == id);

                var especiePg = await _pgContext.Especies
                    .Include(e => e.Animales)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (especieSql == null && especiePg == null)
                {
                    return ApiResult<Especie>.Fail("Datos no encontrados en ninguna base de datos");
                }

                return ApiResult<Especie>.Ok(especieSql ?? especiePg);
            }
            catch (Exception ex)
            {
                return ApiResult<Especie>.Fail(ex.Message);
            }
        }

        // PUT: api/Especies/5 (actualiza en ambas bases)
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult<Especie>>> PutEspecie(int id, Especie especie)
        {
            if (id != especie.Id)
            {
                return ApiResult<Especie>.Fail("Identificador no coincide");
            }

            _sqlcontext.Entry(especie).State = EntityState.Modified;
            _pgContext.Entry(especie).State = EntityState.Modified;

            try
            {
                await _sqlcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!EspecieExistsSql(id))
                {
                    return ApiResult<Especie>.Fail("Datos no encontrados en SQL Server");
                }
                else
                {
                    return ApiResult<Especie>.Fail(ex.Message);
                }
            }

            try
            {
                await _pgContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!EspecieExistsPg(id))
                {
                    return ApiResult<Especie>.Fail("Datos no encontrados en PostgreSQL");
                }
                else
                {
                    return ApiResult<Especie>.Fail(ex.Message);
                }
            }

            return ApiResult<Especie>.Ok(null);
        }

        // POST: api/Especies (crea en ambas bases)
        [HttpPost]
        public async Task<ActionResult<ApiResult<Especie>>> PostEspecie(Especie especie)
        {
            // Guardar en SQL Server
            try
            {
                _sqlcontext.Especies.Add(especie);
                await _sqlcontext.SaveChangesAsync();
            }
            catch (Exception exSql)
            {
                return ApiResult<Especie>.Fail($"Error en SQL Server: {exSql.Message}");
            }

            // Guardar en PostgreSQL (nuevo objeto para evitar conflicto de ID)
            try
            {
                var especiePg = new Especie
                {
                    Nombre = especie.Nombre
                };
                _pgContext.Especies.Add(especiePg);
                await _pgContext.SaveChangesAsync();
            }
            catch (Exception exPg)
            {
                return ApiResult<Especie>.Fail($"Error en PostgreSQL: {exPg.Message}");
            }

            return ApiResult<Especie>.Ok(especie);
        }

        // DELETE: api/Especies/5 (elimina en ambas bases)
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResult<Especie>>> DeleteEspecie(int id)
        {
            try
            {
                var especieSql = await _sqlcontext.Especies.FindAsync(id);
                var especiePg = await _pgContext.Especies.FindAsync(id);

                if (especieSql == null && especiePg == null)
                {
                    return ApiResult<Especie>.Fail("Datos no encontrados en ninguna base de datos");
                }

                if (especieSql != null)
                {
                    _sqlcontext.Especies.Remove(especieSql);
                    await _sqlcontext.SaveChangesAsync();
                }

                if (especiePg != null)
                {
                    _pgContext.Especies.Remove(especiePg);
                    await _pgContext.SaveChangesAsync();
                }

                return ApiResult<Especie>.Ok(null);
            }
            catch (Exception ex)
            {
                return ApiResult<Especie>.Fail(ex.Message);
            }
        }

        private bool EspecieExistsSql(int id)
        {
            return _sqlcontext.Especies.Any(e => e.Id == id);
        }

        private bool EspecieExistsPg(int id)
        {
            return _pgContext.Especies.Any(e => e.Id == id);
        }
    }
}