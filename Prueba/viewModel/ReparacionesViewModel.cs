using Prueba.data;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class ReparacionesViewModel : BaseViewModel
    {
        // Propiedades públicas
        public List<string> ListaEstadoReparacion { get; set; } = new()
        {
            "Problema sin identificar", "Diagnosticando", "Esperando Repuesto", "En Reparacion"
        };

        public ObservableCollection<VehiculoReparacionDTO> VehiculosAsignados { get; set; } = new();
        public ObservableCollection<Repuesto> RepuestosSeleccionados { get; set; } = new();

        // Campos privados
        private readonly ReparacionRepository _reparacionRepository;
        private readonly VehiculoRepository _vehiculoRepository;
        private VehiculoReparacionDTO? _vehiculoSeleccionado;
        private bool _mantenimientoAgregado = false;

        // Propiedades de binding
        private string _trabajoRealizar = string.Empty;
        public string TrabajoRealizar
        {
            get => _trabajoRealizar;
            set => SetProperty(ref _trabajoRealizar, value);
        }

        private string _estadoSeleccionado = string.Empty;
        public string EstadoSeleccionado
        {
            get => _estadoSeleccionado;
            set => SetProperty(ref _estadoSeleccionado, value);
        }

        private string _nuevoRepuesto = string.Empty;
        public string NuevoRepuesto
        {
            get => _nuevoRepuesto;
            set => SetProperty(ref _nuevoRepuesto, value);
        }

        private decimal _repuestoPrecio = 0;
        public decimal RepuestoPrecio
        {
            get => _repuestoPrecio;
            set => SetProperty(ref _repuestoPrecio, value);
        }

        public VehiculoReparacionDTO? VehiculoSeleccionado
        {
            get => _vehiculoSeleccionado;
            set
            {
                if (SetProperty(ref _vehiculoSeleccionado, value))
                {
                    TrabajoRealizar = value?.TrabajoRealizar ?? string.Empty;
                    EstadoSeleccionado = value?.Estado ?? string.Empty;
                }
            }
        }

        // Comandos
        public ICommand AgregarMantenimientoCommand { get; }
        public ICommand AgregarRepuestoCommand { get; }
        public ICommand CancelarReparacionCommand { get; }
        public ICommand FinalizarReparacionCommand { get; set; }
        public ICommand GuardarCambiosCommand { get; set; }

        // Constructor
        public ReparacionesViewModel()
        {
            //Instaciar la lista
            _reparacionRepository = new ReparacionRepository();
            _vehiculoRepository = new VehiculoRepository();
            //Comandos
            AgregarMantenimientoCommand = new comandoViewModel(AgregarMantenimientoBasico);
            AgregarRepuestoCommand = new comandoViewModel(AgregarPieza, PuedeAgregarPieza);
            CancelarReparacionCommand = new comandoViewModel(CancelarReparacion, CanExecuteDeleteCommand);
            FinalizarReparacionCommand = new comandoViewModel(FinalizarReparacion,CanExecuteFinalizarCommand);
            GuardarCambiosCommand = new comandoViewModel(GuardarCambios);

            VehiculosAsignadosActualmente();
        }

        



        #region Métodos de comando
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;

            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
        }

        private void GuardarCambios(object obj)
        {
            if (VehiculoSeleccionado == null)
            {
                
                MessageBox.Show("Selecciona un vehículo para guardar cambios.");
                return;
            }

            try
            {
                
                _reparacionRepository.GuardarCambiosReparacion(
                    VehiculoSeleccionado,
                    TrabajoRealizar,
                    EstadoSeleccionado,
                    RepuestosSeleccionados.ToList()
                );

                MessageBox.Show("Cambios guardados correctamente.");

                // Limpiar los campos después de guardar
                TrabajoRealizar = string.Empty;
                EstadoSeleccionado = string.Empty;
                NuevoRepuesto = string.Empty;
                RepuestoPrecio = 0;
                RepuestosSeleccionados.Clear();
                VehiculoSeleccionado = null;
                //Refrescar la lista
                VehiculosAsignados.Clear();
                VehiculosAsignadosActualmente();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cambios: " + ex.Message);
            }
        }
        private bool CanExecuteFinalizarCommand(object? obj)
        {
            return EstadoSeleccionado == "En Reparacion";
        }

        private void FinalizarReparacion(object obj)
        {
            throw new NotImplementedException();
        }
        private void AgregarPieza(object? obj)
        {
            RepuestosSeleccionados.Add(new Repuesto { Nombre = NuevoRepuesto, Precio = RepuestoPrecio });
        }

        private void AgregarMantenimientoBasico(object? obj)
        {
            var nombresMantenimiento = new[] { "Filtros", "Aceite", "Anticongelante" };

            if (!_mantenimientoAgregado)
            {
                RepuestosSeleccionados.Add(new Repuesto { Nombre = "Filtros", Precio = 40 });
                RepuestosSeleccionados.Add(new Repuesto { Nombre = "Aceite", Precio = 50 });
                RepuestosSeleccionados.Add(new Repuesto { Nombre = "Anticongelante", Precio = 30 });
                _mantenimientoAgregado = true;
            }
            else
            {
                foreach (var nombre in nombresMantenimiento)
                {
                    var item = RepuestosSeleccionados.FirstOrDefault(r => r.Nombre == nombre);
                    if (item != null)
                        RepuestosSeleccionados.Remove(item);
                }
                _mantenimientoAgregado = false;
            }
        }

        private bool PuedeAgregarPieza(object? obj)
        {
            return !string.IsNullOrWhiteSpace(NuevoRepuesto) && RepuestoPrecio > 0;
        }

        private void CancelarReparacion(object? obj)
        {
            if (VehiculoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un vehículo para cancelar la reparación.");
                return;
            }

            try
            {
                string matricula = VehiculoSeleccionado.Matricula;

                _vehiculoRepository.CancelarReparacionPorMatricula(matricula);

                // Limpia UI
                RepuestosSeleccionados.Clear();
                VehiculosAsignados.Remove(VehiculoSeleccionado);
                VehiculoSeleccionado = null;

                MessageBox.Show("Reparación cancelada correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar la reparación: " + ex.Message);
            }
        }

        private bool CanExecuteDeleteCommand(object? obj)
        {
            return VehiculoSeleccionado != null;
        }

        private void VehiculosAsignadosActualmente()
        {
            if (string.IsNullOrEmpty(UserData.id_mecanico))
            {
                MessageBox.Show("El ID del mecánico no está definido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var lista = _vehiculoRepository.ObtenerVehiculosAsignados(UserData.id_mecanico);
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
        #endregion
    }
}
