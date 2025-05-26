using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.data
{
    //Los DTO (data transfer object) sirve para ensanblar datos combinados de dos tablas en mi caso
    public class VehiculoReparacionDTO
    {
        public int Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string TrabajoRealizar { get; set; } = string.Empty;
        public DateTime FechaFin {  get; set; } = DateTime.Now;
    }

}
