using Npgsql;
using Prueba.model;
using Prueba.view.childViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba.data
{
    public class HistorialFacturaRepository : Conexion
    {
        public void GuardarFacturaEnHistorial(HistorialFactura historial)
        {
            using (var connection = GetConection())
            {
                connection.Open();
                using (var command = new NpgsqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                                INSERT INTO historial_factura (
                                    factura_id,
                                    cliente_dni,
                                    cliente_nombre,
                                    cliente_telefono,
                                    vehiculo_matricula,
                                    vehiculo_marca,
                                    vehiculo_modelo,
                                    mecanico_id,
                                    mecanico_nombre,
                                    fecha_emision,
                                    total
                                ) VALUES (@facturaId, @dni, @nombre, @telefono, @matricula, @marca, @modelo,@mecanicoId, @mecanico, @fecha, @total);";

                    command.Parameters.AddWithValue("@facturaId", historial.IdFactura);
                    command.Parameters.AddWithValue("@dni", historial.DniCliente);
                    command.Parameters.AddWithValue("@nombre", historial.NombreCliente);
                    command.Parameters.AddWithValue("@telefono", historial.TelefonoCliente);
                    command.Parameters.AddWithValue("@matricula", historial.VehiculoMatricula);
                    command.Parameters.AddWithValue("@marca", historial.VehiculoMarca);
                    command.Parameters.AddWithValue("@modelo", historial.VehiculoModelo);
                    command.Parameters.AddWithValue("@mecanico", historial.MecanicoNombre);
                    command.Parameters.AddWithValue("@mecanicoId", historial.MecanicoId);
                    command.Parameters.AddWithValue("@fecha", historial.FechaEmision);
                    command.Parameters.AddWithValue("@total", historial.Total);

                    command.ExecuteNonQuery();
                }
            }
        }
        public List<HistorialFactura> MostrarFacturasMecanico(string idMecanico)
        {
            var lista = new List<HistorialFactura>();
            string idAdmin = "h0";
            try
            {
                using (var connection = GetConection())
                {
                    connection.Open();
                    string query;

                    if (idMecanico == idAdmin)  
                    {
                        query = @"SELECT id, factura_id, cliente_dni, cliente_nombre, cliente_telefono, 
                            vehiculo_matricula, vehiculo_marca, vehiculo_modelo, mecanico_nombre, total, mecanico_id
                          FROM historial_factura";
                    }
                    else  
                    {
                        query = @"SELECT id, factura_id, cliente_dni, cliente_nombre, cliente_telefono, 
                            vehiculo_matricula, vehiculo_marca, vehiculo_modelo, mecanico_nombre, total, mecanico_id
                          FROM historial_factura
                          WHERE mecanico_id = @idFactura";
                    }

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (idMecanico != idAdmin)
                        {
                            command.Parameters.AddWithValue("@idFactura", idMecanico);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var historial = new HistorialFactura
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    IdFactura = reader.GetInt32(reader.GetOrdinal("factura_id")),
                                    DniCliente = reader.GetString(reader.GetOrdinal("cliente_dni")),
                                    NombreCliente = reader.GetString(reader.GetOrdinal("cliente_nombre")),
                                    TelefonoCliente = reader.GetString(reader.GetOrdinal("cliente_telefono")),
                                    VehiculoMatricula = reader.GetString(reader.GetOrdinal("vehiculo_matricula")),
                                    VehiculoMarca = reader.GetString(reader.GetOrdinal("vehiculo_marca")),
                                    VehiculoModelo = reader.GetString(reader.GetOrdinal("vehiculo_modelo")),
                                    MecanicoNombre = reader.GetString(reader.GetOrdinal("mecanico_nombre")),
                                    Total = reader.GetDecimal(reader.GetOrdinal("total")),
                                    MecanicoId = reader.GetString(reader.GetOrdinal("mecanico_id"))
                                };

                                lista.Add(historial);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener historial de facturas:\n" +
                                "Mensaje: " + ex.Message + "\n" +
                                "Fuente: " + ex.Source + "\n" +
                                "StackTrace: " + ex.StackTrace);
            }

            return lista;
        }
        public void EliminarFactura(int idFactura)
        {
            try
            {
                using var conn = GetConection();
                conn.Open();

                string query = "DELETE FROM historial_factura WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", idFactura);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar la factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
