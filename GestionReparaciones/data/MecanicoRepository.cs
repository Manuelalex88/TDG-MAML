using Npgsql;
using GestionReparaciones.model;
using GestionReparaciones.view.childViews;
using System;
using System.Windows;

namespace GestionReparaciones.repository
{
    public class MecanicoRepository : Conexion
    {
        public Mecanico? Login(string id, string password)
        {
            try
            {
                using (var conn = GetConexion())
                {
                    conn.Open();
                    // Para la comprobacion del login
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
                using (var connection = GetConexion())
                {
                    connection.Open();
                    // Para obtener los mecanicos (todos)
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
        public Mecanico? ObtenerMecanicoPorId(string idMecanico)
        {
            
            try
            {
                using var conn = GetConexion();
                conn.Open();
                // Obtener los mecanicos por su id
                string query = @"SELECT id, nombre, contrasena FROM mecanico WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idMecanico);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Mecanico
                    {
                        Id = reader.GetString(reader.GetOrdinal("id")),
                        Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                        Contrasena = reader.GetString(reader.GetOrdinal("contrasena"))
                    };
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar mecánico por ID: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return null;
        }
        public void EliminarMecanico(string idMecanico)
        {
            try
            {
                using var conn = GetConexion();
                conn.Open();
                // Eliminar al mecanico
                string query = @"DELETE FROM mecanico WHERE id = @id";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("id", idMecanico);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: Eliminacion del mecanico fallida: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void RegistrarMecanico(Mecanico mec)
        {
            try
            {
                using var conn = GetConexion();
                conn.Open();
                // Registrar un nuevo mecanico si no esta repetido el id
                string query = @"INSERT INTO mecanico (id,nombre,contrasena) VALUES (@id, @nombre, @contrasena) ON CONFLICT (id) DO NOTHING";

                using( var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("id", mec.Id ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("nombre", mec.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("contrasena", mec.Contrasena ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

            }catch (Exception ex)
            {
                MessageBox.Show($"Error: Creacion del mecanico fallida: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ModificarMecanico(Mecanico mec, string idMecanico)
        {
            try
            {
                using var conn = GetConexion();
                conn.Open();
                // Modificamos un mecanico 
                string query = @"UPDATE mecanico SET id = @nuevoId, nombre = @nombre, contrasena = @contrasena WHERE id = @idOriginal;"; 

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@idOriginal", idMecanico); 
                    cmd.Parameters.AddWithValue("@nuevoId", mec.Id);         
                    cmd.Parameters.AddWithValue("@nombre", mec.Nombre);
                    cmd.Parameters.AddWithValue("@contrasena", mec.Contrasena);

                    cmd.ExecuteNonQuery();
                }
            }catch(Exception ex)
            {
                MessageBox.Show($"Error: Guardado del mecanico fallido: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public bool TestConexion()
        {
            using var conexion = GetConexion();
            // Es un test para la conexion que se usa para el icono
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
        public bool EstaMecanicoRelacionado(string idMecanico)
        {
            try
            {
                using var conn = GetConexion();
                conn.Open();
                // Verifica si el mecánico esta relacionado a reparaciones que NO esten finalizadas
                string query = @"
                        SELECT EXISTS (
                            SELECT 1 FROM reparacion 
                            WHERE mecanico_id = @id 
                            AND estado != 'Finalizada'
                
                            UNION
                

                            SELECT 1 FROM factura f
                            INNER JOIN reparacion r ON f.id_reparacion = r.id
                            WHERE r.mecanico_id = @id
                            AND f.pagado = false
                        );";

                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", idMecanico);

                var result = cmd.ExecuteScalar();
                return result != null && (bool)result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al verificar relaciones del mecánico: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return true; // Por seguridad, si hay error asumimos que esta relacionado
            }
        }
    }
}
