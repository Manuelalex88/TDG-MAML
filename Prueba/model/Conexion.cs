using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.model
{
    public abstract class Conexion //USAR
    {
        private readonly string _conectionString;

        public Conexion()
        {
            _conectionString = "Host=localhost;Port=5432;Username=postgres;Password=12345;Database=TallerMecanico";
        }
        protected NpgsqlConnection GetConection()
        {
            return new NpgsqlConnection(_conectionString);
        }

    }
}
