using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zoologico.modelo
{
    public class Raza
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        public List<Animal>? Animales { get; set; }
    }
}
