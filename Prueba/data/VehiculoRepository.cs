using Npgsql;
using Prueba.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba.data
{
    public class VehiculoRepository 
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString;

        public List<Vehiculo> ObtenerVehiculosEnTaller()
        {
            var vehiculos = new List<Vehiculo>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT matricula, marca, modelo FROM vehiculo WHERE asignado = false";


                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var vehiculo = new Vehiculo
                        {
                            Matricula = reader.GetString(reader.GetOrdinal("matricula")),
                            Marca = reader.GetString(reader.GetOrdinal("marca")),
                            Modelo = reader.GetString(reader.GetOrdinal("modelo")),
                        };
                        vehiculos.Add(vehiculo);
                    }
                }
            }

            return vehiculos;
        }
        public List<VehiculoReparacionDTO> ObtenerVehiculosAsignados(string idMecanico)
        {
            var lista = new List<VehiculoReparacionDTO>();

            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new NpgsqlCommand(@"
                    SELECT v.marca, v.matricula, r.estado 
                    FROM vehiculo v
                    JOIN reparacion r ON v.matricula = r.matricula_vehiculo
                    WHERE r.mecanico_id = @id", conn);

                cmd.Parameters.AddWithValue("@id", idMecanico);

                using (var reader = cmd.ExecuteReader())
                {
                    // Obtener los índices de las columnas por nombre
                    int marcaIndex = reader.GetOrdinal("marca");
                    int matriculaIndex = reader.GetOrdinal("matricula");
                    int estadoIndex = reader.GetOrdinal("estado");

                    while (reader.Read())
                    {
                        lista.Add(new VehiculoReparacionDTO
                        {
                            Marca = reader.IsDBNull(marcaIndex) ? null : reader.GetString(marcaIndex),
                            Matricula = reader.IsDBNull(matriculaIndex) ? null : reader.GetString(matriculaIndex),
                            Estado = reader.IsDBNull(estadoIndex) ? null : reader.GetString(estadoIndex) // Asegúrate de que 'estado' puede ser nulo
                        });
                    }
                }
            }

            return lista;
        }
    }
}

