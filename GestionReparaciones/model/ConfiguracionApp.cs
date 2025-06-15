using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionReparaciones.model
{
    public class ConfiguracionApp
    {
        public string NombreTaller { get; set; } = "Taller Mecánico";
        public string CadenaConexion { get; set; } = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=TallerMecanico";
        public int IVA { get; set; } = 21;
        public decimal ManoObra { get; set; } = 0;
        // Datos de la factura
        public string Calle { get; set; } = string.Empty;
        public string Municipio { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CIF { get; set; } = string.Empty;
    }
}
