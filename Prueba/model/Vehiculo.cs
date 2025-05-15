using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.DTO
{
    public class Vehiculo
    {
        public string Matricula { get; set; }             // Clave primaria
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public string MotivoIngreso { get; set; }
        public string Descripcion { get; set; }
        public Boolean Asignado { get; set; }              

        // Propiedad auxiliar para saber si el vehículo ya está asignado
        public bool EstaAsignado => Asignado;
    }
}
