using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.data
{
    public static class DatosConstantes
    {
        //Nombre Taller
        public const string NombreTaller = "TALLER MANUEL S.A";

        // Precios constantes
        public const decimal MantenimientoBasico = 120.00m;
        public const decimal ManoDeObra = 50.00m;
        // Ruta del resources interno
        public static string rutaConfiguracion => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "configuracion.json");
        //Estados constantes
        public const string Estado1 = "Problema sin identificar";
        public const string Estado2 = "Diagnosticando";
        public const string Estado3 = "Esperando Repuesto";
        public const string Estado4 = "En Reparacion"; 
        public const string Estado5 = "Pendiente de Factura";
        public const string Estado6 = "Finalizada";

    }
}
