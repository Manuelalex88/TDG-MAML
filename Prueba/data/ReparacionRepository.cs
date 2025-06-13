using Npgsql;
using Prueba.data;
using Prueba.model;
using Prueba.view.childViews;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Prueba.data
{
    public class ReparacionRepository : Conexion
    {
        public void GuardarCambiosReparacion(VehiculoReparacionDTO vehiculo, string trabajo, string estado, List<RepuestoUsadoDTO> repuestos)
        {
            using var conn = GetConexion();
            conn.Open();

            using var transaction = conn.BeginTransaction();
            try
            {
                // Obtener ID de reparación actual
                int reparacionId;
                using (var cmdGetId = new NpgsqlCommand("SELECT id FROM reparacion WHERE matricula_vehiculo = @matricula AND estado <> @estadoFinalizado;", conn, transaction))
                {
                    cmdGetId.Parameters.AddWithValue("matricula", vehiculo.Matricula);
                    cmdGetId.Parameters.AddWithValue("estadoFinalizado", DatosConstantes.Estado6);
                    reparacionId = Convert.ToInt32(cmdGetId.ExecuteScalar());
                }

                

                // Actualizar reparación
                string updateReparacion = @"
                            UPDATE reparacion
                            SET trabajo_a_realizar = @trabajo, estado = @estado
                            WHERE id = @reparacionId;";

                using (var cmdUpdate = new NpgsqlCommand(updateReparacion, conn, transaction))
                {
                    cmdUpdate.Parameters.AddWithValue("trabajo", trabajo ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("estado", estado ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("reparacionId", reparacionId);
                    cmdUpdate.ExecuteNonQuery();
                }

                

                foreach (var repuesto in repuestos)
                {
                    int repuestoId;
                    var nombreMayus = repuesto.Nombre.Trim().ToUpperInvariant();

                    // Insertar o actualizar repuesto y obtener ID
                    using (var cmdInsertRepuesto = new NpgsqlCommand(@"
                            INSERT INTO repuesto (nombre, precio)
                            VALUES (@nombre, @precio)
                            ON CONFLICT (nombre)
                            DO UPDATE SET precio = EXCLUDED.precio
                            RETURNING id;", conn, transaction))
                    {
                        cmdInsertRepuesto.Parameters.AddWithValue("nombre", nombreMayus);
                        cmdInsertRepuesto.Parameters.AddWithValue("precio", repuesto.Precio);
                        repuestoId = Convert.ToInt32(cmdInsertRepuesto.ExecuteScalar());
                    }

                    // Insertar o actualizar repuesto_usado cambiando la cantidad
                    using (var cmdInsertUsado = new NpgsqlCommand(@"
                            INSERT INTO repuesto_usado (reparacion_id, repuesto_id, cantidad, pagado)
                            VALUES (@reparacion_id, @repuesto_id, @cantidad, FALSE)
                            ON CONFLICT (reparacion_id, repuesto_id)
                            DO UPDATE SET cantidad = EXCLUDED.cantidad;", conn, transaction))
                    {
                        cmdInsertUsado.Parameters.AddWithValue("reparacion_id", reparacionId);
                        cmdInsertUsado.Parameters.AddWithValue("repuesto_id", repuestoId);
                        cmdInsertUsado.Parameters.AddWithValue("cantidad", repuesto.Cantidad);
                        cmdInsertUsado.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Error al guardar cambios en la reparación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FinalizarReparacionActual(VehiculoReparacionDTO vehiculo, int reparacionId)
        {
            using var conn = GetConexion();
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // Actualizar reparación
                using (var cmd = new NpgsqlCommand(@"
                        UPDATE reparacion
                        SET estado = @estado, fecha_fin = @fechaFin
                        WHERE id = @id;", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("estado", DatosConstantes.Estado6);
                    cmd.Parameters.AddWithValue("fechaFin", DateTime.Now);
                    cmd.Parameters.AddWithValue("id", reparacionId);
                    cmd.ExecuteNonQuery();
                }
                //Actualizar el vehiculo (Ya no esta asignado)
                using (var cmd = new NpgsqlCommand(@"
                        UPDATE vehiculo
                        SET asignado = FALSE,
                            salida_taller = TRUE
                        WHERE matricula = @matricula;", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("matricula", vehiculo.Matricula);
                    cmd.ExecuteNonQuery();
                }

                // Calcular _total de la reparación
                decimal total = CalcularTotal(reparacionId, conn, transaction);

                // Insertar factura
                using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO factura (id_reparacion, fecha_emision, pagado, total)
                        VALUES (@idReparacion, @fechaEmision, FALSE, @total);", conn, transaction))
                {
                    cmd.Parameters.AddWithValue("idReparacion", reparacionId);
                    cmd.Parameters.AddWithValue("total", total);
                    cmd.Parameters.AddWithValue("fechaEmision", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                MessageBox.Show($"Error al finalizar reparación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<RepuestoUsadoDTO> ObtenerRepuestosUsados(int reparacionId)
        {
            var repuestos = new List<RepuestoUsadoDTO>();

            try
            {
                using var conn = GetConexion();
                conn.Open();

                string query = @"
                        SELECT ru.id, r.nombre, r.precio, ru.cantidad
                        FROM repuesto_usado ru
                        JOIN repuesto r ON r.id = ru.repuesto_id
                        JOIN reparacion rep ON rep.id = ru.reparacion_id
                        WHERE ru.reparacion_id = @reparacionId
                        AND ru.pagado = false";

                

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("reparacionId", reparacionId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    repuestos.Add(new RepuestoUsadoDTO
                    {
                        Id = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        Precio = reader.GetDecimal(2),
                        Cantidad = reader.GetInt32(3)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener repuestos usados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return repuestos;
        }

        public int ObtenerIdReparacionPorMatricula(string matricula)
        {
            try
            {
                using var conn = GetConexion();
                conn.Open();

                using var cmd = new NpgsqlCommand("SELECT id FROM reparacion WHERE matricula_vehiculo = @matricula AND estado <> @estadoFinalizado;", conn);
                cmd.Parameters.AddWithValue("matricula", matricula);
                cmd.Parameters.AddWithValue("estadoFinalizado", DatosConstantes.Estado6);
                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return -1; // o valor que indique no encontrado

                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener ID de reparación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        private decimal CalcularTotal(int reparacionId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            try
            {
                // Obtener el total de repuestos usados
                string queryRepuestos = @"
                            SELECT COALESCE(SUM(ru.cantidad * r.precio), 0)
                            FROM repuesto_usado ru
                            JOIN repuesto r ON ru.repuesto_id = r.id
                            WHERE ru.reparacion_id = @reparacionId;";

                using var cmdRepuestos = new NpgsqlCommand(queryRepuestos, conn, transaction);
                cmdRepuestos.Parameters.AddWithValue("reparacionId", reparacionId);

                object result = cmdRepuestos.ExecuteScalar();
                decimal totalRepuestos = Convert.ToDecimal(result ?? 0m);

                //Calculo del total mas mano de obra
                decimal subtotal = totalRepuestos + DatosConstantes.ManoDeObra;
                //Calculo iva
                decimal iva = subtotal * 0.21m;
                //Total 
                decimal totalConIva = subtotal + iva;

                return totalConIva;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular _total: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0m;
            }
        }

        public void EliminarRepuestoDeReparacion(int repuestoUsadoId)
        {

            try
            {
                using var connection = GetConexion();
                connection.Open();

                using var command = new NpgsqlCommand("DELETE FROM repuesto_usado WHERE id = @id", connection);
                command.Parameters.AddWithValue("@id", repuestoUsadoId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el repuesto usado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
