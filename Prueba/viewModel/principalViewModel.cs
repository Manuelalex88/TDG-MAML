using Prueba.view;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prueba.model;
using Prueba.view.childViews;
using System.Collections.ObjectModel;
using Prueba.data;
using System.Windows.Input;
using System.Windows;


namespace Prueba.viewModel
{
    public class PrincipalViewModel : BaseViewModel
    {


        #region Listas
        public ObservableCollection<VehiculoReparacionDTO> ReparacionesAsignadas { get; set; }
        #endregion
        #region Campos
        private readonly VehiculoRepository _vehiculoRepository;

        #endregion

        public PrincipalViewModel()
        {
            //Instanciar
            ReparacionesAsignadas = new ObservableCollection<VehiculoReparacionDTO>();
            _vehiculoRepository = new VehiculoRepository();

            //Metodos
            CargarReparacionesAsignadas();
        }
        #region Metodos
        private void CargarReparacionesAsignadas()
        {
            var idMecanico = Thread.CurrentPrincipal?.Identity?.Name;

            if (string.IsNullOrEmpty(idMecanico))
            {
                MessageBox.Show("El ID del mecánico no está definido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var lista = _vehiculoRepository.ObtenerVehiculosAsignados(idMecanico);
                foreach (var v in lista)
                {
                    ReparacionesAsignadas.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los vehículos asignados: " + ex.Message);
            }
        }
        #endregion

    }
}
