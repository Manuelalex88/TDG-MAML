using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.data
{
    public class DataBaseService
    {
        private readonly string _conectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString;

        public bool ValidateUser(string username, string password)
        {
            using var conn = new NpgsqlConnection(_conectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM mecanico WHERE nombre=@n AND contraseña=@c", conn);
            cmd.Parameters.AddWithValue("n", username);
            cmd.Parameters.AddWithValue("c", password);

            object? result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return false;

            return Convert.ToInt64(result) > 0;
        }
    }
}
