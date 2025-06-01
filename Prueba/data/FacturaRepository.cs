using Npgsql;
using Prueba.model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba.data
{
    public class FacturaRepository : Conexion
    {
        

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

            using var conn = GetConection();
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

            using var conn = GetConection();
            conn.Open();

            var query = @"
                    SELECT r.nombre, r.precio, ru.cantidad
                    FROM repuesto_usado ru
                    JOIN repuesto r ON ru.repuesto_id = r.id
                    WHERE ru.reparacion_id = @reparacionId AND ru.facturado = FALSE";

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
                    Precio = precio,
                    Cantidad = cantidad,
                });
            }

            return lista;
        }
        public void MarcarFacturaComoPagada(int facturaId)
        {
            string query = "UPDATE factura SET pagado = TRUE WHERE id = @id";

            using var conn = GetConection();
            conn.Open();

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", facturaId);

            cmd.ExecuteNonQuery();
        }
        public void MarcarRepuestosComoFacturados(int idReparacion)
        {
            using var conn = GetConection();
            conn.Open();

            var query = "UPDATE repuesto_usado SET facturado = TRUE WHERE reparacion_id = @id";
            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", idReparacion);
            cmd.ExecuteNonQuery();
        }
        public void EliminarFacturaSeleccionada(int facturaId)
        {
            using var conn = GetConection();
            conn.Open();

            string query = "DELETE FROM factura WHERE id = @id";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("id", facturaId);

            cmd.ExecuteNonQuery();
        }
        public List<FacturaVehiculoClienteDTO> ObtenerFacturasPorMecanico(string idMecanico)
        {
            var lista = new List<FacturaVehiculoClienteDTO>();

            using (var connection = GetConection())
            {
                connection.Open();

                string query = @"
                            SELECT f.id, f.fecha_emision, f.total,
                           v.marca, v.modelo, v.matricula,
                           c.nombre AS cliente_nombre, c.dni, c.telefono
                        FROM factura f
                        INNER JOIN reparacion r ON f.id_reparacion = r.id
                        INNER JOIN vehiculo v ON r.matricula_vehiculo = v.matricula
                        INNER JOIN cliente_vehiculo cv ON cv.vehiculo_id = v.matricula
                        INNER JOIN cliente c ON cv.cliente_id = c.dni
                        WHERE r.mecanico_id = @idMecanico
                          AND f.pagado = TRUE
                        ORDER BY f.fecha_emision DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@idMecanico", idMecanico);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dto = new FacturaVehiculoClienteDTO
                            {
                                Id = reader.GetInt32(0),
                                FechaEmision = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                                Total = reader.GetDecimal(2),
                                Marca = reader.GetString(3),
                                Modelo = reader.GetString(4),
                                Matricula = reader.GetString(5),
                                ClienteNombre = reader.GetString(6),
                                Dni = reader.GetString(7),
                                Telefono = reader.GetString(8)
                            };

                            lista.Add(dto);
                        }
                    }
                }
            }

            return lista;
        }
    }
}
