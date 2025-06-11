using Npgsql;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Prueba.data
{
    public class FacturaRepository : Conexion
    {
        public List<FacturaVehiculoClienteDTO> MostrarFacturaAdmin()
        {
            var lista = new List<FacturaVehiculoClienteDTO>();
            try
            {
                using(var conn = GetConection())
                {
                    conn.Open();
                    string query = @"SELECT 
                                    f.id AS factura_id,
                                    f.fecha_emision,
                                    f.total,
                                    f.pagado,
                                    v.matricula,
                                    c.nombre AS cliente_nombre,
                                    c.dni AS cliente_dni
                                FROM factura f
                                JOIN reparacion r ON f.id_reparacion = r.id
                                JOIN vehiculo v ON r.matricula_vehiculo = v.matricula
                                JOIN cliente_vehiculo cv ON v.matricula = cv.vehiculo_id
                                JOIN cliente c ON cv.cliente_id = c.dni";

                    using (var command = new NpgsqlCommand(query, conn))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var factura = new FacturaVehiculoClienteDTO
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("factura_id")),
                                    FechaEmision = reader.GetDateTime(reader.GetOrdinal("fecha_emision")),
                                    Total = reader.GetDecimal(reader.GetOrdinal("total")),
                                    Pagado = reader.GetBoolean(reader.GetOrdinal("pagado")),
                                    Matricula = reader.GetString(reader.GetOrdinal("matricula")),
                                    ClienteNombre = reader.GetString(reader.GetOrdinal("cliente_nombre")),
                                    Dni = reader.GetString(reader.GetOrdinal("cliente_dni")),
                                };

                                lista.Add(factura);
                            }
                        }
                    }
                }
               
            }catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener las facturas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            return lista;
        }

        public void MarcarRepuestosComoFacturados(int idReparacion)
        {
            
            using (var connection = GetConection())
            {
                connection.Open();

                string sql = "UPDATE repuesto_usado SET Pagado = TRUE WHERE reparacion_id = @idReparacion;";

                using (var cmd = new Npgsql.NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("idReparacion", idReparacion);
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    
                    if (filasAfectadas == 0)
                    {
                        
                        Console.WriteLine("No se actualizó ningún registro.");
                    }
                    else
                    {
                        Console.WriteLine($"{filasAfectadas} registro(s) actualizado(s) correctamente.");
                    }
                }
            }
        }
        public List<FacturaVehiculoClienteDTO> ObtenerFacturasPendientesPorMecanico(string mecanicoId)
        {
            var lista = new List<FacturaVehiculoClienteDTO>();

            try
            {
                string query = @"SELECT 
                                f.id AS factura_id,
                                f.id_reparacion,
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
                              AND f.pagado = FALSE
                            ORDER BY f.fecha_emision DESC;";

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
                        IdReparacion = reader.GetInt32(1), 
                        FechaEmision = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        Total = reader.GetDecimal(3),
                        Marca = reader.GetString(4),
                        Modelo = reader.GetString(5),
                        Matricula = reader.GetString(6),
                        ClienteNombre = reader.GetString(7),
                        Dni = reader.GetString(8),
                        Telefono = reader.IsDBNull(9) ? "Error: No hay telefono disponible" : reader.GetString(9)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener facturas pendientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lista;
        }

        public List<RepuestoUsadoDTO> ObtenerRepuestosUsadosPorReparacion(int facturaID)
        {
            var lista = new List<RepuestoUsadoDTO>();

            try
            {
                using var conn = GetConection();
                conn.Open();

                // Obtenemos el reparacion_id de la factura
                int reparacionId;

                var queryReparacionId = "SELECT id_reparacion FROM factura WHERE id = @facturaID";

                using (var cmdReparacion = new NpgsqlCommand(queryReparacionId, conn))
                {
                    cmdReparacion.Parameters.AddWithValue("facturaID", facturaID);
                    var result = cmdReparacion.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return lista;

                    reparacionId = Convert.ToInt32(result);
                }

                // Obtenemos los repuestos usados
                var queryRepuestos = @"
                    SELECT ru.id, r.nombre, r.precio, ru.cantidad
                    FROM repuesto_usado ru
                    JOIN repuesto r ON ru.repuesto_id = r.id
                    WHERE ru.reparacion_id = @reparacionId";

                using (var cmdRepuestos = new NpgsqlCommand(queryRepuestos, conn))
                {
                    cmdRepuestos.Parameters.AddWithValue("reparacionId", reparacionId);

                    using var reader = cmdRepuestos.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new RepuestoUsadoDTO
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Precio = reader.GetDecimal(2),
                            Cantidad = reader.GetInt32(3),
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener repuestos usados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lista;
        }

        public void MarcarFacturaComoPagada(int facturaId)
        {
            
            try
            {
                string query = "UPDATE factura SET pagado = TRUE WHERE id = @id";

                using var conn = GetConection();
                conn.Open();

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", facturaId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al marcar factura como pagada: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void EliminarFacturaSeleccionada(int facturaId)
        {
            try
            {
                using var conn = GetConection();
                conn.Open();

                string query = "DELETE FROM factura WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", facturaId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
