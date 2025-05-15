using Prueba.view;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prueba.model;


namespace Prueba.viewModel
{
    public class principalViewModel : BaseViewModel
    {
        //Campos
        private string _nombreUsuario;

        public string TextoEntrada => "Bienvenido, " + _nombreUsuario;

        //Propiedades
        public principalViewModel()
        {
            CargarUsuario();
        }

        private void CargarUsuario()
        {
            _nombreUsuario = UserData.Nombre;
            cambioPropiedad(nameof(TextoEntrada));
        }

        // Si algún día cambias UserData.Nombre en tiempo real y quieres refrescar la UI:
        public void ActualizarNombre()
        {
            _nombreUsuario = UserData.Nombre;
            cambioPropiedad(nameof(TextoEntrada));
        }

    }
}
