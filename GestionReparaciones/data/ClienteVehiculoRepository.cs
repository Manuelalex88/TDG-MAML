using Npgsql;
using GestionReparaciones.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace GestionReparaciones.data
{
    public class ClienteVehiculoRepository : Conexion
    {


        public void GuardarClienteVehiculoYAsignar(Cliente c, Vehiculo v, string idMecanico, bool asignar)
        {

            using (var conn = GetConexion())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Insertar cliente (si no existe)
                        string insertCliente = @"
                                INSERT INTO cliente (dni, nombre, telefono)
                                VALUES (@dni, @nombre, @telefono)
                                ON CONFLICT (dni) DO NOTHING;";

                        using (var cmdCliente = new NpgsqlCommand(insertCliente, conn))
                        {
                            cmdCliente.Parameters.AddWithValue("dni", c.Dni ?? (object)DBNull.Value);
                            cmdCliente.Parameters.AddWithValue("nombre", c.Nombre ?? (object)DBNull.Value);
                            cmdCliente.Parameters.AddWithValue("telefono", c.Telefono ?? (object)DBNull.Value);
                            cmdCliente.ExecuteNonQuery();
                        }

                        // Insertar vehículo
                        string insertVehiculo = @"
                                INSERT INTO vehiculo (matricula, marca, modelo, motivo_ingreso, descripcion, asignado, salida_taller)
                                VALUES (@matricula, @marca, @modelo, @motivo_ingreso, @descripcion, @asignado, @salida_taller)
                                ON CONFLICT (matricula) DO UPDATE SET salida_taller = FALSE, descripcion = EXCLUDED.descripcion;;";

                        using (var cmdVehiculo = new NpgsqlCommand(insertVehiculo, conn))
                        {
                            cmdVehiculo.Parameters.AddWithValue("matricula", v.Matricula ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("marca", v.Marca ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("modelo", v.Modelo ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("motivo_ingreso", v.MotivoIngreso ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("descripcion", v.Descripcion ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("asignado", asignar);
                            cmdVehiculo.Parameters.AddWithValue("salida_taller", false);
                            cmdVehiculo.ExecuteNonQuery();
                        }
                        // Eliminar relación anterior (si la hay)
                        string deleteRelacion = @"
                                DELETE FROM cliente_vehiculo 
                                WHERE vehiculo_id = @matricula;";

                        using (var cmdDeleteRelacion = new NpgsqlCommand(deleteRelacion, conn))
                        {
                            cmdDeleteRelacion.Parameters.AddWithValue("matricula", v.Matricula ?? (object)DBNull.Value);
                            cmdDeleteRelacion.ExecuteNonQuery();
                        }
                        // Insertar relación cliente-vehículo
                        string insertRelacion = @"
                                INSERT INTO cliente_vehiculo (cliente_id, vehiculo_id)
                                VALUES (@dni, @matricula)
                                ON CONFLICT DO NOTHING;";
                        using (var cmdRelacion = new NpgsqlCommand(insertRelacion, conn))
                        {
                            cmdRelacion.Parameters.AddWithValue("dni", c.Dni ?? (object)DBNull.Value);
                            cmdRelacion.Parameters.AddWithValue("matricula", v.Matricula ?? (object)DBNull.Value);
                            cmdRelacion.ExecuteNonQuery();
                        }

                        if (asignar)
                        {
                            string estadoReparacion = v.MotivoIngreso == "Problema sin identificar" ? "Diagnosticando" : "En Reparacion";
                            string insertReparacion = @"
                                    INSERT INTO reparacion (matricula_vehiculo, fecha_inicio, trabajo_a_realizar, mecanico_id, estado)
                                    VALUES (@matricula, @fecha_inicio, @trabajo_a_realizar, @mecanico_id, @estado);";
                            using (var cmdReparacion = new NpgsqlCommand(insertReparacion, conn))
                            {
                                var trabajo = string.IsNullOrWhiteSpace(v.Descripcion) ? v.MotivoIngreso : v.Descripcion;
                                cmdReparacion.Parameters.AddWithValue("matricula", v.Matricula ?? (object)DBNull.Value);
                                cmdReparacion.Parameters.AddWithValue("fecha_inicio", DateTime.Now);
                                cmdReparacion.Parameters.AddWithValue("trabajo_a_realizar", trabajo ?? (object)DBNull.Value);
                                cmdReparacion.Parameters.AddWithValue("mecanico_id", idMecanico);
                                cmdReparacion.Parameters.AddWithValue("estado", estadoReparacion);
                                cmdReparacion.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                conn.Close();
            }
        }

        public bool BuscarVehiculoEnTaller(string matricula)
        {
            using (var conn = GetConexion())
            {
                conn.Open();

                string query = @"
                        SELECT COUNT(*) 
                        FROM vehiculo 
                        WHERE matricula = @matricula AND salida_taller = FALSE;";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("matricula", matricula);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0; 
                }
            }
        }
        public bool VehiculoTieneFacturaPendiente(string matricula)
        {
            try
            {
                using var conn = GetConexion();
                conn.Open();

                string query = @"
                        SELECT COUNT(*) 
                        FROM factura f
                        JOIN reparacion r ON f.id_reparacion = r.id
                        WHERE r.matricula_vehiculo = @matricula AND f.pagado = FALSE;";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("matricula", matricula);

                var count = (long?)cmd.ExecuteScalar();
                return count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al comprobar facturas pendientes: " + ex.Message);
                return true; 
            }
        }
    }
}
