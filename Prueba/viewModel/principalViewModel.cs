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
        #region Campos
        private string _nombreUsuario= String.Empty;

        public string TextoEntrada => "Bienvenido, " + _nombreUsuario;
        #endregion
        
        public principalViewModel()
        {
            CargarUsuario();
        }
        #region Metodos
        private void CargarUsuario()
        {
            _nombreUsuario = UserData.Nombre;
            OnPropertyChanged(nameof(TextoEntrada));
        }

        // Si algún día cambias UserData.Nombre en tiempo real y quieres refrescar la UI:
        public void ActualizarNombre()
        {
            _nombreUsuario = UserData.Nombre;
            OnPropertyChanged(nameof(TextoEntrada));
        }
        #endregion
    }
}
