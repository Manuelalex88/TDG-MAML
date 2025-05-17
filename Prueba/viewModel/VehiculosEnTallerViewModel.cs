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
        //Campos
        private VehiculoRepository _vehiculoRepository;
        public ObservableCollection<Vehiculo> VehiculosEnTaller { get; set; } = new();
        #region Comandos
        public ICommand AsignarVehiculoCommand { get; set; }
        #endregion
        //Constructor
        public VehiculosEnTallerViewModel()
        {
            //Comandos
            AsignarVehiculoCommand = new comandoViewModel(AsignarVehiculo);

            RellenarListaVehiculosTaller();
        }


        #region Metodos
        private void AsignarVehiculo(object obj)
        {
    
            if (obj is Vehiculo vehiculo && !string.IsNullOrEmpty(UserData.id_mecanico))
            {           
                try
                {
                    _vehiculoRepository.AsignarVehiculoAVista(vehiculo.Matricula, UserData.id_mecanico);
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
                System.Windows.MessageBox.Show("Error en ViewModel: " + ex.Message);
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
