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
    public class AnimalsController : ControllerBase
    {
        private readonly SqlServerDbContext _sqlcontext;
        private readonly PostgresDbContext _pgContext;

        public AnimalsController(SqlServerDbContext sqlcontext, PostgresDbContext pgContext)
        {
            _sqlcontext = sqlcontext;
            _pgContext = pgContext;
        }

        // GET: api/Animals (solo SQL Server)
        [HttpGet]
        public async Task<ActionResult<ApiResult<List<Animal>>>> GetAnimal()
        {
            try
            {
                var dataSql = await _sqlcontext.Animales
                    .Include(a => a.Raza)
                    .Include(a => a.Especie)
                    .ToListAsync();
               
                var dataPg = await _pgContext.Animales
                    .Include(a => a.Raza)
                    .Include(a => a.Especie)
                    .ToListAsync();

                var allData = dataSql.Concat(dataPg).ToList();
                return ApiResult<List<Animal>>.Ok(allData);
            }
            catch (Exception ex)
            {
                return ApiResult<List<Animal>>.Fail(ex.Message);
            }
        }

        // GET: api/Animals/5 (solo SQL Server)
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Animal>>> GetAnimal(int id)
        {
            try
            {
                var animalSql = await _sqlcontext.Animales
                    .Include(a => a.Raza)
                    .Include(a => a.Especie)
                    .FirstOrDefaultAsync(a => a.Id == id);

                var animalPg = await _pgContext.Animales
                    .Include(a => a.Raza)
                    .Include(a => a.Especie)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (animalSql == null && animalPg == null)
                {
                    return ApiResult<Animal>.Fail("Datos no encontrados en ninguna base de datos");
                }

                // Prioriza SQL Server, si no existe, devuelve el de Postgres
                return ApiResult<Animal>.Ok(animalSql ?? animalPg);
            }
            catch (Exception ex)
            {
                return ApiResult<Animal>.Fail(ex.Message);
            }
        }

        // PUT: api/Animals/5 (actualiza en ambas bases)
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResult<Animal>>> PutAnimal(int id, Animal animal)
        {
            if (id != animal.Id)
            {
                return ApiResult<Animal>.Fail("Identificador no coincide");
            }

            _sqlcontext.Entry(animal).State = EntityState.Modified;
            _pgContext.Entry(animal).State = EntityState.Modified;

            try
            {
                await _sqlcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!AnimalExistsSql(id))
                {
                    return ApiResult<Animal>.Fail("Datos no encontrados en SQL Server");
                }
                else
                {
                    return ApiResult<Animal>.Fail(ex.Message);
                }
            }

            try
            {
                await _pgContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!AnimalExistsPg(id))
                {
                    return ApiResult<Animal>.Fail("Datos no encontrados en PostgreSQL");
                }
                else
                {
                    return ApiResult<Animal>.Fail(ex.Message);
                }
            }

            return ApiResult<Animal>.Ok(null);
        }

        // POST: api/Animals (crea en ambas bases)
        [HttpPost]
        public async Task<ActionResult<ApiResult<Animal>>> PostAnimal(Animal animal)
        {
            // Guardar en SQL Server
            try
            {
                _sqlcontext.Animales.Add(animal);
                await _sqlcontext.SaveChangesAsync();
            }
            catch (Exception exSql)
            {
                return ApiResult<Animal>.Fail($"Error en SQL Server: {exSql.Message}");
            }

            // Guardar en PostgreSQL (nuevo objeto para evitar conflicto de ID)
            try
            {
                var animalPg = new Animal
                {
                    Nombre = animal.Nombre,
                    año = animal.año,
                    color = animal.color,
                    EspecieId = animal.EspecieId,
                    RazaId = animal.RazaId
                };
                _pgContext.Animales.Add(animalPg);
                await _pgContext.SaveChangesAsync();
            }
            catch (Exception exPg)
            {
                return ApiResult<Animal>.Fail($"Error en PostgreSQL: {exPg.Message}");
            }

            return ApiResult<Animal>.Ok(animal);
        }

        // DELETE: api/Animals/5 (elimina en ambas bases)
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResult<Animal>>> DeleteAnimal(int id)
        {
            try
            {
                var animalSql = await _sqlcontext.Animales.FindAsync(id);
                var animalPg = await _pgContext.Animales.FindAsync(id);

                if (animalSql == null && animalPg == null)
                {
                    return ApiResult<Animal>.Fail("Datos no encontrados en ninguna base de datos");
                }

                if (animalSql != null)
                {
                    _sqlcontext.Animales.Remove(animalSql);
                    await _sqlcontext.SaveChangesAsync();
                }

                if (animalPg != null)
                {
                    _pgContext.Animales.Remove(animalPg);
                    await _pgContext.SaveChangesAsync();
                }

                return ApiResult<Animal>.Ok(null);
            }
            catch (Exception ex)
            {
                return ApiResult<Animal>.Fail(ex.Message);
            }
        }

        private bool AnimalExistsSql(int id)
        {
            return _sqlcontext.Animales.Any(e => e.Id == id);
        }

        private bool AnimalExistsPg(int id)
        {
            return _pgContext.Animales.Any(e => e.Id == id);
        }
    }
}