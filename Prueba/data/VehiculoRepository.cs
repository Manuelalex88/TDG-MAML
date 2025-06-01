using Npgsql;
using Prueba.data;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba.data
{
    public class VehiculoRepository : Conexion
    {
        

        public List<Vehiculo> ObtenerVehiculosEnTaller()
        {
            var vehiculos = new List<Vehiculo>();

            using (var connection = GetConection())
            {
                connection.Open();

                string query = "SELECT matricula, marca, modelo FROM vehiculo WHERE asignado = false AND salida_taller = false";


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

            using (var conn  = GetConection())
            {
                conn.Open();
                var cmd = new NpgsqlCommand(@"
            SELECT r.id, v.marca, v.matricula, r.estado, r.trabajo_a_realizar, r.fecha_inicio
            FROM vehiculo v
            JOIN reparacion r ON v.matricula = r.matricula_vehiculo
            WHERE r.mecanico_id = @id 
              AND v.salida_taller = false
              AND r.estado NOT IN (@estado5, @estado6)", conn);

                cmd.Parameters.AddWithValue("@id", idMecanico);
                cmd.Parameters.AddWithValue("@estado5", DatosConstantes.Estado5);
                cmd.Parameters.AddWithValue("@estado6", DatosConstantes.Estado6);

                using (var reader = cmd.ExecuteReader())
                {
                    int idIndex = reader.GetOrdinal("id");
                    int marcaIndex = reader.GetOrdinal("marca");
                    int matriculaIndex = reader.GetOrdinal("matricula");
                    int estadoIndex = reader.GetOrdinal("estado");
                    int trabajoIndex = reader.GetOrdinal("trabajo_a_realizar");
                    int fechaInicioIndex = reader.GetOrdinal("fecha_inicio");

                    while (reader.Read())
                    {
                        lista.Add(new VehiculoReparacionDTO
                        {
                            Id = reader.GetInt32(idIndex),
                            Marca = reader.IsDBNull(marcaIndex) ? string.Empty : reader.GetString(marcaIndex),
                            Matricula = reader.IsDBNull(matriculaIndex) ? string.Empty : reader.GetString(matriculaIndex),
                            Estado = reader.IsDBNull(estadoIndex) ? string.Empty : reader.GetString(estadoIndex),
                            TrabajoARealizar = reader.IsDBNull(trabajoIndex) ? string.Empty : reader.GetString(trabajoIndex),
                            Fecha_Inicio = reader.IsDBNull(fechaInicioIndex) ? DateTime.MinValue : reader.GetDateTime(fechaInicioIndex)
                        });
                    }
                }
            }

            return lista;
        }


        // Elimina todos los repuestos asociados a una reparación de un vehículo por matrícula
        public void CancelarReparacionPorMatricula(string matricula)
        {
            using (var connection = GetConection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Obtener ID de la reparación
                        var getIdCmd = new NpgsqlCommand("SELECT id FROM reparacion WHERE matricula_vehiculo = @matricula", connection);
                        getIdCmd.Parameters.AddWithValue("@matricula", matricula);
                        var reparacionId = getIdCmd.ExecuteScalar();

                        if (reparacionId == null)
                            throw new Exception("No se encontró la reparación para la matrícula dada.");

                        int repId = Convert.ToInt32(reparacionId);

                        // Eliminar repuestos usados
                        var deleteRepuestosCmd = new NpgsqlCommand("DELETE FROM repuesto_usado WHERE reparacion_id = @repId", connection);
                        deleteRepuestosCmd.Parameters.AddWithValue("@repId", repId);
                        deleteRepuestosCmd.ExecuteNonQuery();

                        // Marcar vehículo como no asignado
                        var updateVehiculoCmd = new NpgsqlCommand("UPDATE vehiculo SET asignado = false WHERE matricula = @matricula", connection);
                        updateVehiculoCmd.Parameters.AddWithValue("@matricula", matricula);
                        updateVehiculoCmd.ExecuteNonQuery();

                        // También podrías eliminar la reparación si lo deseas:
                        var deleteReparacionCmd = new NpgsqlCommand("DELETE FROM reparacion WHERE id = @repId", connection);
                        deleteReparacionCmd.Parameters.AddWithValue("@repId", repId);
                        deleteReparacionCmd.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw; // vuelve a lanzar el error
                    }
                }
            }
        }
        public void AsignarVehiculoAVista(string matricula, string mecanicoId)
        {
            using var conn = GetConection();
            conn.Open();

            using var trans = conn.BeginTransaction();
            try
            {
                // Actualizar vehículo como asignado
                var updateCmd = new NpgsqlCommand("UPDATE vehiculo SET asignado = true WHERE matricula = @matricula", conn);
                updateCmd.Parameters.AddWithValue("@matricula", matricula);
                updateCmd.ExecuteNonQuery();

                // Consultar motivo_ingreso del vehículo
                string motivoIngreso = "Problema sin identificar"; // Valor por defecto
                var motivoCmd = new NpgsqlCommand("SELECT motivo_ingreso FROM vehiculo WHERE matricula = @matricula", conn);
                motivoCmd.Parameters.AddWithValue("@matricula", matricula);
                var result = motivoCmd.ExecuteScalar();


                // Determinar el estado inicial según el motivo
                string estadoReparacion = motivoIngreso == "Problema sin identificar" ? "Diagnosticando" : "En Reparacion";

                // Insertar reparación
                var insertCmd = new NpgsqlCommand(@"
                    INSERT INTO reparacion (matricula_vehiculo, mecanico_id, trabajo_a_realizar, estado, fecha_inicio)
                    VALUES (@matricula, @mecanicoId, @trabajo, @estado, @fechaInicio)", conn);

                insertCmd.Parameters.AddWithValue("@matricula", matricula);
                insertCmd.Parameters.AddWithValue("@mecanicoId", mecanicoId);
                insertCmd.Parameters.AddWithValue("@trabajo", motivoIngreso);
                insertCmd.Parameters.AddWithValue("@estado", estadoReparacion);
                insertCmd.Parameters.AddWithValue("@fechaInicio", DateTime.Now);

                insertCmd.ExecuteNonQuery();

                trans.Commit();
            }
            catch (Exception)
            {
                trans.Rollback();
                throw;
            }
        }
        public void MarcarSalidaTaller(string matricula)
        {
            using var conn = GetConection();
            conn.Open();

            using var cmd = new NpgsqlCommand("UPDATE vehiculo SET salida_taller = true WHERE matricula = @matricula", conn);
            cmd.Parameters.AddWithValue("matricula", matricula);
            cmd.ExecuteNonQuery();
        }
        public Vehiculo? BuscarPorMatricula(string matricula)
        {
            
            Vehiculo? vehiculo = null;

            using (var conn = GetConection())
            {
                conn.Open();
                const string sql = "SELECT marca, modelo FROM vehiculo WHERE matricula = @matricula";
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("matricula", matricula);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vehiculo = new Vehiculo
                            {
                                Marca = reader["marca"]?.ToString() ?? string.Empty,
                                Modelo = reader["modelo"]?.ToString() ?? string.Empty,
                            };
                        }
                    }
                }
            }
            return vehiculo;
        }


    }
}

