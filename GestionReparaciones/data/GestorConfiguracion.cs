using GestionReparaciones.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GestionReparaciones.data
{
    public static class GestorConfiguracion
    {
        public static ConfiguracionApp CargarConfiguracion()
        {
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "configuracion.json");

            if (!File.Exists(ruta))
                throw new FileNotFoundException("Archivo de configuración no encontrado.", ruta);

            string json = File.ReadAllText(ruta);
            return JsonSerializer.Deserialize<ConfiguracionApp>(json)
                   ?? throw new Exception("No se pudo deserializar la configuración.");
        }
    }
}
