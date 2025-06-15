using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionReparaciones.model
{
    public class Reparacion
    {
        public int Id { get; set; }
        public int MecanicoId { get; set; }
        public int VehiculoId { get; set; } 
        public string Matricula { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string TrabajoARealizar { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;

    }
}
