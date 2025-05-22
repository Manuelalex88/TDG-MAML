using Npgsql;
using Prueba.data;
using Prueba.model;
using Prueba.view.childViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        ON CONFLICT (nombre) DO NOTHING;", conn))
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

                    // Insertar en repuesto_usado si no existe ya
                    using (var cmdInsertUsado = new NpgsqlCommand(@"
                        INSERT INTO repuesto_usado (reparacion_id, repuesto_id)
                        SELECT @reparacion_id, @repuesto_id
                        WHERE NOT EXISTS (
                            SELECT 1 FROM repuesto_usado
                            WHERE reparacion_id = @reparacion_id AND repuesto_id = @repuesto_id
                        );", conn))
                    {
                        cmdInsertUsado.Parameters.AddWithValue("reparacion_id", reparacionId);
                        cmdInsertUsado.Parameters.AddWithValue("repuesto_id", repuestoId);
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
        public void FinalizarReparacionActual(int reparacionId)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                UPDATE reparacion
                SET estado = @estado, fecha_fin = @fechaFin
                WHERE id = @id;", conn);

            cmd.Parameters.AddWithValue("estado", DatosConstantes.Estado5);
            cmd.Parameters.AddWithValue("fechaFin", DateTime.Now);
            cmd.Parameters.AddWithValue("id", reparacionId);

            cmd.ExecuteNonQuery();
        }
    }
}
