using Npgsql;
using Prueba.model;
using System;
using System.Windows;

namespace Prueba.repository
{
    public class MecanicoRepository : Conexion
    {
        public Mecanico? Login(string id, string password)
        {
            try
            {
                using (var conn = GetConection())
                {
                    conn.Open();

                    string query = "SELECT id, nombre FROM Mecanico WHERE id = @id AND contrasena = @contrasena";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@contrasena", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Mecanico
                                {
                                    Id = reader.GetString(0),
                                    Nombre = reader.GetString(1)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }

        public bool TestConexion()
        {
            using var conexion = GetConection();
            try
            {
                conexion.Open();
                return conexion.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
