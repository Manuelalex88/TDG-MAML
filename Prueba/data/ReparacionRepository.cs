using Npgsql;
using Prueba.data;
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
    public class ReparacionRepository
    {
        private readonly string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString;

        public void GuardarCambiosReparacion(VehiculoReparacionDTO vehiculo, string trabajo, string estado, List<Repuesto> repuestos)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();
            try
            {
                // Actualizar reparación
                string updateReparacion = @"
                    UPDATE reparacion
                    SET trabajo_a_realizar = @trabajo, estado = @estado
                    WHERE matricula_vehiculo = @matricula;";

                using (var cmdUpdate = new NpgsqlCommand(updateReparacion, conn))
                {
                    cmdUpdate.Parameters.AddWithValue("trabajo", trabajo ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("estado", estado ?? (object)DBNull.Value);
                    cmdUpdate.Parameters.AddWithValue("matricula", vehiculo.Matricula);
                    cmdUpdate.ExecuteNonQuery();
                }

                // Obtener ID de reparación actual
                int reparacionId;
                using (var cmdGetId = new NpgsqlCommand("SELECT id FROM reparacion WHERE matricula_vehiculo = @matricula", conn))
                {
                    cmdGetId.Parameters.AddWithValue("matricula", vehiculo.Matricula);
                    reparacionId = Convert.ToInt32(cmdGetId.ExecuteScalar());
                }

                foreach (var repuesto in repuestos)
                {
                    int repuestoId;

                    // Insertar repuesto si no existe
                    using (var cmdInsertRepuesto = new NpgsqlCommand(@"
                            INSERT INTO repuesto (nombre, precio)
                            VALUES (@nombre, @precio)
                            ON CONFLICT (nombre) DO UPDATE SET precio = EXCLUDED.precio;", conn))
                    {
                        cmdInsertRepuesto.Parameters.AddWithValue("nombre", repuesto.Nombre);
                        cmdInsertRepuesto.Parameters.AddWithValue("precio", repuesto.Precio);
                        cmdInsertRepuesto.ExecuteNonQuery();
                    }

                    // Obtener ID del repuesto
                    using (var cmdGetRepuestoId = new NpgsqlCommand("SELECT id FROM repuesto WHERE nombre = @nombre", conn))
                    {
                        cmdGetRepuestoId.Parameters.AddWithValue("nombre", repuesto.Nombre);
                        repuestoId = Convert.ToInt32(cmdGetRepuestoId.ExecuteScalar());
                    }

                    // Insertar en repuesto_usado (acumular cantidad si ya existe)
                    using (var cmdInsertUsado = new NpgsqlCommand(@"
                            INSERT INTO repuesto_usado (reparacion_id, repuesto_id, cantidad)
                            VALUES (@reparacion_id, @repuesto_id, @cantidad)
                            ON CONFLICT (reparacion_id, repuesto_id)
                            DO UPDATE SET cantidad = EXCLUDED.cantidad;", conn))
                    {
                        cmdInsertUsado.Parameters.AddWithValue("reparacion_id", reparacionId);
                        cmdInsertUsado.Parameters.AddWithValue("repuesto_id", repuestoId);
                        cmdInsertUsado.Parameters.AddWithValue("cantidad", repuesto.Cantidad); 
                        cmdInsertUsado.ExecuteNonQuery();
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
        public void FinalizarReparacionActual(VehiculoReparacionDTO vehiculo,int reparacionId)
        {
            using var conn = new NpgsqlConnection(connectionString);
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
                    cmd.Parameters.AddWithValue("estado", DatosConstantes.Estado5);
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

                // Calcular total de la reparación
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
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public List<Repuesto> ObtenerRepuestosUsados(int reparacionId)
        {
            var repuestos = new List<Repuesto>();

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            string query = @"
                    SELECT r.nombre, r.precio, ru.cantidad
                    FROM repuesto_usado ru
                    JOIN repuesto r ON r.id = ru.repuesto_id
                    WHERE ru.reparacion_id = @reparacionId;";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("reparacionId", reparacionId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                repuestos.Add(new Repuesto
                {
                    Nombre = reader.GetString(0),
                    Precio = reader.GetDecimal(1),
                    Cantidad = reader.GetInt32(2)
                });
            }

            return repuestos;
        }
        public int ObtenerIdReparacionPorMatricula(string matricula)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT id FROM reparacion WHERE matricula_vehiculo = @matricula", conn);
            cmd.Parameters.AddWithValue("matricula", matricula);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }
        private decimal CalcularTotal(int reparacionId, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            string query = @"
                    SELECT COALESCE(SUM(ru.cantidad * r.precio), 0)
                    FROM repuesto_usado ru
                    JOIN repuesto r ON ru.repuesto_id = r.id
                    WHERE ru.reparacion_id = @reparacionId;";

            using var cmd = new NpgsqlCommand(query, conn, transaction);
            cmd.Parameters.AddWithValue("reparacionId", reparacionId);

            object result = cmd.ExecuteScalar();
            return Convert.ToDecimal(result);
        }
    }
}
