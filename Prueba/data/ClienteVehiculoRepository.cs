using Npgsql;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba.data
{
    public class ClienteVehiculoRepository : Conexion
    {
        

        public void GuardarClienteVehiculoYAsignar(string dniCliente, string nombreCliente, string telefonoCliente,
                                                   string matricula, string marca, string modelo,
                                                   string motivoIngreso, string descripcion, bool asignar,
                                                   string mecanicoId)

        {
            using (var conn = GetConection())
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
                            cmdCliente.Parameters.AddWithValue("dni", dniCliente ?? (object)DBNull.Value);
                            cmdCliente.Parameters.AddWithValue("nombre", nombreCliente ?? (object)DBNull.Value);
                            cmdCliente.Parameters.AddWithValue("telefono", telefonoCliente ?? (object)DBNull.Value);
                            cmdCliente.ExecuteNonQuery();
                        }

                        // Insertar vehículo
                        string insertVehiculo = @"
                        INSERT INTO vehiculo (matricula, marca, modelo, motivo_ingreso, descripcion, asignado, salida_taller)
                        VALUES (@matricula, @marca, @modelo, @motivo_ingreso, @descripcion, @asignado, @salida_taller)
                        ON CONFLICT (matricula) DO UPDATE SET salida_taller = FALSE;";
                        using (var cmdVehiculo = new NpgsqlCommand(insertVehiculo, conn))
                        {
                            cmdVehiculo.Parameters.AddWithValue("matricula", matricula ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("marca", marca ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("modelo", modelo ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("motivo_ingreso", motivoIngreso ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("descripcion", descripcion ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("asignado", asignar);
                            cmdVehiculo.Parameters.AddWithValue("salida_taller", false); 
                            cmdVehiculo.ExecuteNonQuery();
                        }

                        // Insertar relación cliente-vehículo
                        string insertRelacion = @"
                        INSERT INTO cliente_vehiculo (cliente_id, vehiculo_id)
                        VALUES (@dni, @matricula)
                        ON CONFLICT DO NOTHING;";
                        using (var cmdRelacion = new NpgsqlCommand(insertRelacion, conn))
                        {
                            cmdRelacion.Parameters.AddWithValue("dni", dniCliente);
                            cmdRelacion.Parameters.AddWithValue("matricula", matricula);
                            cmdRelacion.ExecuteNonQuery();
                        }

                        if (asignar)
                        {
                            string estadoReparacion = motivoIngreso == "Problema sin identificar" ? "Diagnosticando" : "En Reparacion";
                            string insertReparacion = @"
                            INSERT INTO reparacion (matricula_vehiculo, fecha_inicio, trabajo_a_realizar, mecanico_id, estado)
                            VALUES (@matricula, @fecha_inicio, @trabajo_a_realizar, @mecanico_id, @estado);";
                            using (var cmdReparacion = new NpgsqlCommand(insertReparacion, conn))
                            {
                                var trabajo = string.IsNullOrWhiteSpace(descripcion) ? motivoIngreso : descripcion;
                                cmdReparacion.Parameters.AddWithValue("matricula", matricula ?? (object)DBNull.Value);
                                cmdReparacion.Parameters.AddWithValue("fecha_inicio", DateTime.Now);
                                cmdReparacion.Parameters.AddWithValue("trabajo_a_realizar", trabajo ?? (object)DBNull.Value);
                                cmdReparacion.Parameters.AddWithValue("mecanico_id", mecanicoId);
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
    }
}
