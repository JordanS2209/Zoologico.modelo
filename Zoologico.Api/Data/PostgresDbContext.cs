using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zoologico.modelo;

    public class PostgresDbContext : DbContext
    {
        public PostgresDbContext (DbContextOptions<PostgresDbContext> options)
            : base(options)
        {
        }

        public DbSet<Zoologico.modelo.Animal> Animales { get; set; } = default!;
        public DbSet<Zoologico.modelo.Especie> Especies { get; set; } = default!;

        public DbSet<Zoologico.modelo.Raza> Razas { get; set; } = default!;
}
