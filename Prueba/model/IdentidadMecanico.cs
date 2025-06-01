using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.model
{
    public class IdentidadMecanico : IIdentity
    {
        //Para guardar la identidad del mecanico y usarlo en todos lados
        public string Name { get; }
        public string NombreCompleto { get; }

        public string AuthenticationType => "Custom";
        public bool IsAuthenticated => true;

        public IdentidadMecanico(string id, string nombreCompleto)
        {
            Name = id;
            NombreCompleto = nombreCompleto;
        }
    }
}
