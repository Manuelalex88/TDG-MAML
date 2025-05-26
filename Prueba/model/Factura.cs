using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.model
{
    public class Factura
    {
        public int Id { get; set; }
        public int IdReparacion { get; set; }
        public DateTime? FechaEmision { get; set; }
        public bool Pagado { get; set; }
        public decimal Total { get; set; }
    }
}
