using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionReparaciones.data
{
    public class RepuestoUsadoDTO
    {
        //Repuesto
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        //Repuesto_usado
        public int Id { get; set; }
        public int Cantidad { get; set; }
        public bool Pagado { get; set; }
    }

}
