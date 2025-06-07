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
        public List<Mecanico> ObtenerMecanicos()
        {
            var lista = new List<Mecanico>();
            try
            {
                using (var connection = GetConection())
                {
                    connection.Open();
                    string query = @"SELECT id, nombre, contrasena FROM mecanico";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var mecanico = new Mecanico
                                {
                                    Id = reader.GetString(reader.GetOrdinal("id")),
                                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                                    Contrasena = reader.GetString(reader.GetOrdinal("contrasena"))
                                };

                                lista.Add(mecanico);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener los mecanicos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return lista;
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
