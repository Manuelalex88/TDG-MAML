﻿using Npgsql;
using GestionReparaciones.data;
using GestionReparaciones.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GestionReparaciones.data
{
    public class VehiculoRepository : Conexion
    {


        public List<Vehiculo> ObtenerVehiculosEnTaller()
        {
            var vehiculos = new List<Vehiculo>();

            try
            {
                using (var connection = GetConexion())
                {
                    connection.Open();
                    // Obtenemos los vehiculos que estan en el taller
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener vehículos en taller: " + ex.Message);
            }

            return vehiculos;
        }
        public List<VehiculoReparacionDTO> ObtenerVehiculosAsignados(string idMecanico)
        {
            var lista = new List<VehiculoReparacionDTO>();

            try
            {
                using (var conn = GetConexion())
                {
                    conn.Open();
                    // Obtenemos los vehiculos que tenemos asignados
                    var query = new NpgsqlCommand(@"
                            SELECT DISTINCT ON (v.matricula) r.id, v.marca, v.modelo, v.matricula,  
                                   r.estado, r.trabajo_a_realizar, r.fecha_inicio
                            FROM vehiculo v
                            JOIN reparacion r ON v.matricula = r.matricula_vehiculo
                            WHERE r.mecanico_id = @idMecanico
                              AND v.salida_taller = false
                              AND r.estado <> @estado5 AND r.estado <> @estado6
                            ORDER BY v.matricula, r.fecha_inicio DESC;", conn);

                    query.Parameters.AddWithValue("@idMecanico", idMecanico);
                    query.Parameters.AddWithValue("@estado5", DatosConstantesEstaticos.Estado5);
                    query.Parameters.AddWithValue("@estado6", DatosConstantesEstaticos.Estado6);

                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new VehiculoReparacionDTO
                            {
                                IdReparacion = reader.GetInt32(reader.GetOrdinal("id")),
                                Marca = reader.IsDBNull(reader.GetOrdinal("marca")) ? string.Empty : reader.GetString(reader.GetOrdinal("marca")),
                                Modelo = reader.IsDBNull(reader.GetOrdinal("modelo")) ? string.Empty : reader.GetString(reader.GetOrdinal("modelo")),
                                Matricula = reader.IsDBNull(reader.GetOrdinal("matricula")) ? string.Empty : reader.GetString(reader.GetOrdinal("matricula")),
                                Estado = reader.IsDBNull(reader.GetOrdinal("estado")) ? string.Empty : reader.GetString(reader.GetOrdinal("estado")),
                                TrabajoARealizar = reader.IsDBNull(reader.GetOrdinal("trabajo_a_realizar")) ? string.Empty : reader.GetString(reader.GetOrdinal("trabajo_a_realizar")),
                                Fecha_Inicio = reader.IsDBNull(reader.GetOrdinal("fecha_inicio")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("fecha_inicio"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener vehículos asignados: " + ex.Message);
            }

            return lista;
        }


        
        public void CancelarReparacionPorMatricula(string matricula)
        {
            try
            {
                using (var connection = GetConexion())
                {
                    connection.Open();
                    // Elimina todos los repuestos asociados a una reparacion de un vehículo por matricula
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            var getIdCmd = new NpgsqlCommand("SELECT id FROM reparacion WHERE matricula_vehiculo = @matricula", connection);
                            getIdCmd.Parameters.AddWithValue("@matricula", matricula);
                            var reparacionId = getIdCmd.ExecuteScalar();

                            if (reparacionId == null)
                                throw new Exception("No se encontró la reparación para la matrícula dada.");

                            int repId = Convert.ToInt32(reparacionId);

                            var deleteRepuestosCmd = new NpgsqlCommand("DELETE FROM repuesto_usado WHERE reparacion_id = @reparacion_id", connection);
                            deleteRepuestosCmd.Parameters.AddWithValue("reparacion_id", repId);
                            deleteRepuestosCmd.ExecuteNonQuery();

                            var updateVehiculoCmd = new NpgsqlCommand("UPDATE vehiculo SET asignado = false WHERE matricula = @matricula", connection);
                            updateVehiculoCmd.Parameters.AddWithValue("@matricula", matricula);
                            updateVehiculoCmd.ExecuteNonQuery();

                            var deleteReparacionCmd = new NpgsqlCommand("DELETE FROM reparacion WHERE id = @repId", connection);
                            deleteReparacionCmd.Parameters.AddWithValue("@repId", repId);
                            deleteReparacionCmd.ExecuteNonQuery();

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar reparación: " + ex.Message);
            }
        }

        public void AsignarVehiculoAVista(string matricula, string mecanicoId)
        {
            using var conn = GetConexion();
            conn.Open();

            using var trans = conn.BeginTransaction();
            try
            {
                // Actualizar vehiculo como asignado
                var updateCmd = new NpgsqlCommand("UPDATE vehiculo SET asignado = true WHERE matricula = @matricula", conn);
                updateCmd.Parameters.AddWithValue("@matricula", matricula);
                updateCmd.ExecuteNonQuery();

                // Consultar motivo_ingreso y descripcion del vehiculo
                string motivoIngreso = "Problema sin identificar"; // Valor por defecto
                string? descripcionProblema = null;

                var motivoCmd = new NpgsqlCommand("SELECT motivo_ingreso, descripcion FROM vehiculo WHERE matricula = @matricula", conn);
                motivoCmd.Parameters.AddWithValue("@matricula", matricula);

                using (var reader = motivoCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        motivoIngreso = reader["motivo_ingreso"] as string ?? motivoIngreso;
                        descripcionProblema = reader["descripcion"] as string;
                    }
                } 

                string trabajo = string.IsNullOrWhiteSpace(descripcionProblema) ? motivoIngreso : descripcionProblema;

                // Determinar el estado inicial segun el motivo
                string estadoReparacion = motivoIngreso == "Problema sin identificar" ? "Diagnosticando" : "En Reparacion";

                // Insertar reparacion
                var insertCmd = new NpgsqlCommand(@"
                            INSERT INTO reparacion (matricula_vehiculo, mecanico_id, trabajo_a_realizar, estado, fecha_inicio)
                            VALUES (@matricula, @mecanicoId, @trabajo, @estado, @fechaInicio)", conn);

                insertCmd.Parameters.AddWithValue("@matricula", matricula);
                insertCmd.Parameters.AddWithValue("@mecanicoId", mecanicoId);
                insertCmd.Parameters.AddWithValue("@trabajo", trabajo);
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
            try
            {
                using var conn = GetConexion();
                conn.Open();
                // Marcamos la salida del taller
                using var cmd = new NpgsqlCommand("UPDATE vehiculo SET salida_taller = true WHERE matricula = @matricula", conn);
                cmd.Parameters.AddWithValue("matricula", matricula);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al marcar salida del taller: " + ex.Message);
            }
        }
        public Vehiculo? BuscarPorMatricula(string matricula)
        {
            Vehiculo? vehiculo = null;

            try
            {
                using (var conn = GetConexion())
                {
                    conn.Open();
                    // Buscamos el vehiculo por matricula
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar vehículo: " + ex.Message);
            }

            return vehiculo;
        }


    }
}

