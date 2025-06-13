using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.model
{
    public class Vehiculo
    {
        public string Matricula { get; set; } = String.Empty; // Clave primaria
        public string Marca { get; set; } = String.Empty;
        public string Modelo { get; set; } = String.Empty;
        public string MotivoIngreso { get; set; } = String.Empty;
        public string Descripcion { get; set; } = String.Empty;
        public Boolean Asignado { get; set; }   
        public Boolean salidataller { get; set; }

        // Propiedad auxiliar para saber si el vehículo ya está asignado
        public bool EstaAsignado => Asignado;
    }
}
