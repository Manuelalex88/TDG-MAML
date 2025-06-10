using Npgsql;
using Prueba.data;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class VehiculosEnTallerViewModel : BaseViewModel
    {
        #region Listas
        public ObservableCollection<Vehiculo> VehiculosEnTaller { get; set; }
        #endregion

        #region Campos
        private VehiculoRepository _vehiculoRepository;
        public bool EsAdmin => Thread.CurrentPrincipal?.IsInRole("admin") == true;
        #endregion
        
        #region Comandos
        public ICommand AsignarVehiculoCommand { get; set; }
        public ICommand MarcarSalidaCommand { get; set; }

        #endregion
        //Constructor
        public VehiculosEnTallerViewModel()
        {
            //Inicializar
            _vehiculoRepository = new VehiculoRepository();
            VehiculosEnTaller = new ObservableCollection<Vehiculo>();
            //Comandos
            AsignarVehiculoCommand = new comandoViewModel(AsignarVehiculo);
            MarcarSalidaCommand = new comandoViewModel(MarcarSalidaVehiculo);
            RellenarListaVehiculosTaller();
        }


        #region Metodos
        private void MarcarSalidaVehiculo(object obj)
        {
            if (obj is Vehiculo vehiculo)
            {
                try
                {
                    _vehiculoRepository.MarcarSalidaTaller(vehiculo.Matricula);
                    VehiculosEnTaller.Remove(vehiculo);
                    MessageBox.Show("El vehiculo a salido", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al marcar salida: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void AsignarVehiculo(object obj)
        {
            //Identidad Mecanico
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name;

            if (obj is Vehiculo vehiculo && !string.IsNullOrEmpty(idMecanico))
            {           
                try
                {
                    _vehiculoRepository.AsignarVehiculoAVista(vehiculo.Matricula, idMecanico);
                    MessageBox.Show("Vehículo asignado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    VehiculosEnTaller.Remove(vehiculo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al asignar el vehículo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Error: Vehículo o ID del mecánico no válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void RellenarListaVehiculosTaller()
        {
            try
            {
                _vehiculoRepository = new VehiculoRepository();
                VehiculosEnTaller = new ObservableCollection<Vehiculo>();
                CargarVehiculosEnTaller();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en ViewModel: " + ex.Message);
            }
        }
        private void CargarVehiculosEnTaller()
        {
            try
            {
                var vehiculos = _vehiculoRepository.ObtenerVehiculosEnTaller();
                VehiculosEnTaller.Clear();
                foreach (var vehiculo in vehiculos)
                {
                    VehiculosEnTaller.Add(vehiculo);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al cargar los vehículos: " + ex.Message);
            }
        }
        #endregion



    }
}
