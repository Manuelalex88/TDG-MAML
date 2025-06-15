using Npgsql;
using System;
using System.Windows;

namespace GestionReparaciones.data
{
    public class DataBaseService
    {
        // Cadena de conexion obtenida desde el archivo de configuracion App.config
        private readonly string _conectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["PostgreSqlConnection"].ConnectionString;

        //Metodo que valida al usuario
        public bool ValidateUser(string username, string password)
        {
            try
            {
                using var conn = new NpgsqlConnection(_conectionString);
                conn.Open();
                //Crud para buscar al usuario
                using var cmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM mecanico WHERE nombre=@n AND contraseña=@c", conn);

                cmd.Parameters.AddWithValue("n", username);
                cmd.Parameters.AddWithValue("c", password);

                object? result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return false;

                // Si el conteo es mayor que cero, el usuario es valido
                return Convert.ToInt64(result) > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al validar usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }

}
