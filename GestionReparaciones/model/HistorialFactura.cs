using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionReparaciones.model
{
    public class HistorialFactura
    {
        public int Id { get; set; }
        public int IdFactura { get; set; }
        public string DniCliente { get; set; } = string.Empty;
        public string NombreCliente {  get; set; } = string.Empty;
        public string TelefonoCliente {  get; set; } = string.Empty;
        public string VehiculoMatricula {  get; set; } = string.Empty;
        public string VehiculoMarca {  get; set; } = string.Empty;
        public string VehiculoModelo {  get; set; } = string.Empty;
        public string MecanicoNombre {  get; set; } = string.Empty;
        public string MecanicoId {  get; set; } = string.Empty;
        public DateTime FechaEmision {  get; set; } = DateTime.Now;
        public decimal Total {  get; set; }



    }
}
