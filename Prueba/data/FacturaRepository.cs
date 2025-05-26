using Npgsql;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.data
{
    public class FacturaRepository
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager
                                                           .ConnectionStrings["PostgreSqlConnection"]
                                                           .ConnectionString;

        public List<FacturaVehiculoClienteDTO> ObtenerFacturasPendientesPorMecanico(string mecanicoId)
        {
            var lista = new List<FacturaVehiculoClienteDTO>();

            string query = @"SELECT 
                            f.id AS factura_id,
                            f.fecha_emision,
                            f.total,
                            v.marca,
                            v.modelo,
                            v.matricula,
                            c.nombre AS cliente_nombre,
                            c.dni AS cliente_dni,
                            c.telefono AS cliente_telefono
                        FROM factura f
                        JOIN reparacion r ON f.id_reparacion = r.id
                        JOIN vehiculo v ON r.matricula_vehiculo = v.matricula
                        JOIN cliente_vehiculo cv ON v.matricula = cv.vehiculo_id
                        JOIN cliente c ON cv.cliente_id = c.dni
                        WHERE r.mecanico_id = @mecanico_id
                          AND f.pagado = FALSE;";

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("mecanico_id", mecanicoId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new FacturaVehiculoClienteDTO
                {
                    Id = reader.GetInt32(0),
                    FechaEmision = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                    Total = reader.GetDecimal(2),
                    Marca = reader.GetString(3),
                    Modelo = reader.GetString(4),
                    Matricula = reader.GetString(5),
                    ClienteNombre = reader.GetString(6),
                    Dni = reader.GetString(7),
                    Telefono = reader.IsDBNull(8) ? "Error: No hay telefono disponible" : reader.GetString(8)
                });
            }

            return lista;
        }
        public List<Repuesto> ObtenerRepuestosUsadosPorReparacion(int reparacionId)
        {
            var lista = new List<Repuesto>();

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = @"SELECT 
                        r.nombre,
                        r.precio,
                        ru.cantidad
                    FROM repuesto_usado ru
                    JOIN repuesto r ON ru.repuesto_id = r.id
                    WHERE ru.reparacion_id = @reparacionId;";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("reparacionId", reparacionId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string nombre = reader.GetString(0);
                decimal precio = reader.GetDecimal(1);
                int cantidad = reader.GetInt32(2);

                lista.Add(new Repuesto
                {
                    Nombre = nombre,
                    Precio = precio * cantidad
                });
            }

            return lista;
        }
        public void MarcarFacturaComoPagada(int facturaId)
        {
            string query = "UPDATE factura SET pagado = TRUE WHERE id = @id";

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", facturaId);

            cmd.ExecuteNonQuery();
        }

    }
}
