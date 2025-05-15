using Prueba.data;
using Prueba.DTO;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Prueba.viewModel
{
    public class ReparacionesViewModel : BaseViewModel
    {
        public List<string> ListaEstadoReparacion { get; set; } = new List<string>
        {
            "Problema sin identificar", "Diagnosticando", "Esperando Repuesto", "En Reparacion"
        };

        private VehiculoRepository _vehiculoRepository;
        public ObservableCollection<VehiculoReparacionDTO> VehiculosAsignados { get; set; }

        //Constructor
        public ReparacionesViewModel()
        {
            _vehiculoRepository = new VehiculoRepository();
            VehiculosAsignados = new ObservableCollection<VehiculoReparacionDTO>();

            try
            {
                string idMecanico = UserData.id_mecanico; // Usa tu propia lógica aquí
                var lista = _vehiculoRepository.ObtenerVehiculosAsignados(idMecanico);

                foreach (var v in lista)
                {
                    VehiculosAsignados.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los vehículos asignados: " + ex.Message);
            }
        }


    }
    
}
