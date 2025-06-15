using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionReparaciones.data
{
    public class FacturaVehiculoClienteDTO
    {
        //Factura
        public int Id { get; set; }
        public int IdReparacion { get; set; }
        public DateTime? FechaEmision { get; set; }
        public decimal Total { get; set; }
        public bool Pagado { get; set; }
        //Vehiculo
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        //Cliente
        public string ClienteNombre { get; set; } = string.Empty;
        public string Dni {  get; set; } = string.Empty;
        public string Telefono {  get; set; } = string.Empty;

        
    }
}
