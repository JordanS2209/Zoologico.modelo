using System.ComponentModel.DataAnnotations;

namespace Zoologico.modelo
{
    public class Animal
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int año { get; set; }
        public string color { get; set; }

        // claves foraneas 
        public int EspecieId { get; set; }
        public int RazaId { get; set; }

        // propiedades de navegacion
        public Especie? Especie { get; set; }
        public Raza? Raza { get; set; }

    }
}
