using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.model
{
    public class ConfiguracionApp
    {
        public string NombreTaller { get; set; } = "Taller Mecánico";
        public string CadenaConexion { get; set; } = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=TallerMecanico";
    }
}
