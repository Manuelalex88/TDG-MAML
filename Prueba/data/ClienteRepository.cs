using Npgsql;
using Prueba.model;
using System;
using System.Windows;

namespace Prueba.data
{
    public class ClienteRepository : Conexion
    {
        public Cliente? ObtenerPorDni(string dni)
        {
            if (string.IsNullOrWhiteSpace(dni))
            {
                return null;
            }

            Cliente? cliente = null;

            try
            {
                using (var conn = GetConexion())
                {
                    conn.Open();
                    const string sql = "SELECT dni, nombre, telefono FROM cliente WHERE dni = @dni";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("dni", dni);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                cliente = new Cliente
                                {
                                    Dni = reader["dni"]?.ToString() ?? string.Empty,
                                    Nombre = reader["nombre"]?.ToString() ?? string.Empty,
                                    Telefono = reader["telefono"]?.ToString() ?? string.Empty
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener cliente por DNI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return cliente;
        }
    }
}
