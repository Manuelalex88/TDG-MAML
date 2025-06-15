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
            // Construye la ruta completa al archivo "configuracion.json" (Donde lo tengamos)
            string ruta = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "configuracion.json");

            // Verifica si el archivo existe, si no, lanza una excepción indicando que no se encontro.
            if (!File.Exists(ruta))
                throw new FileNotFoundException("Archivo de configuración no encontrado.", ruta);

            // Lee todo el contenido del archivo JSON en una cadena.
            string json = File.ReadAllText(ruta);

            // Intenta deserializar el contenido JSON a un objeto de tipo ConfiguracionApp.
            // Si la deserialización falla (retorna null), lanza una excepcion.
            return JsonSerializer.Deserialize<ConfiguracionApp>(json)
                   ?? throw new Exception("No se pudo deserializar la configuración.");
        }
    }
}
