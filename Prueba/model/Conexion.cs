using Npgsql;
using Prueba.data;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Prueba.model
{
    public abstract class Conexion
    {

        protected NpgsqlConnection GetConexion()
        {
            var config = GestorConfiguracion.CargarConfiguracion();
            string connectionString = config?.CadenaConexion ?? throw new InvalidOperationException("Cadena de conexión no configurada.");
            return new NpgsqlConnection(connectionString);
        }

    }
}
