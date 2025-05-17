using Npgsql;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.data
{
    public class ClienteRepository
    {
        private readonly string _connectionString;

        public ClienteRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString ?? throw new InvalidOperationException("Cadena de conexión no encontrada");
        }

        public Cliente? ObtenerPorDni(string dni)
        {
         
            if (string.IsNullOrWhiteSpace(dni))
                throw new ArgumentException("El DNI no puede ser nulo o vacío", nameof(dni));

            Cliente? cliente = null;

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                const string sql = "SELECT dni, nombre, telefono FROM cliente WHERE dni = @dni";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("dni", dni);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cliente = new Cliente
                            {
                                Dni = reader["dni"]?.ToString() ?? string.Empty,
                                Nombre = reader["nombre"]?.ToString() ?? string.Empty,
                                Telefono = reader["telefono"]?.ToString() ?? string.Empty
                            };
                        }
                    }
                }
            }

            return cliente;
        }
    }
}
