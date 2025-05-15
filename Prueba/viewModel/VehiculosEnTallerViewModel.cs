using Npgsql;
using Prueba.data;
using Prueba.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class VehiculosEnTallerViewModel : BaseViewModel
    {
        //Campos
        private VehiculoRepository _vehiculoRepository;
        public ObservableCollection<Vehiculo> VehiculosEnTaller { get; set; }

        //Constructor
        public VehiculosEnTallerViewModel()
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


    }
}
