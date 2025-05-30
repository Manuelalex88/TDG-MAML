using Prueba.data;
using Prueba.model;
using Prueba.view.childViews;
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
        #region Campos
        
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
        #endregion
        #region Propiedades
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

        private int _cantidad = 0;
        public int CantidadPieza
        {
            get => _cantidad;
            set => SetProperty(ref _cantidad, value);
        }

        public VehiculoReparacionDTO? VehiculoSeleccionado
        {
            get => _vehiculoSeleccionado;
            set
            {
                if (SetProperty(ref _vehiculoSeleccionado, value))
                {
                    TrabajoRealizar = value?.TrabajoARealizar ?? string.Empty;
                    EstadoSeleccionado = value?.Estado ?? string.Empty;
                    RepuestosSeleccionados.Clear();

                    if (value != null)
                    {
                        // Obtener ID de la reparacion actual
                        int reparacionId = _reparacionRepository.ObtenerIdReparacionPorMatricula(value.Matricula);

                        // Cargar repuestos usados en la reparacion
                        var repuestos = _reparacionRepository.ObtenerRepuestosUsados(reparacionId);

                        foreach (var r in repuestos)
                        {
                            RepuestosSeleccionados.Add(r);
                        }
                        // Actualizar estado de _mantenimientoAgregado para que se elimine correctamente
                        var nombresMantenimiento = new[] { "Filtros", "Aceite", "Anticongelante" };
                        _mantenimientoAgregado = RepuestosSeleccionados.Any(r => nombresMantenimiento.Contains(r.Nombre));
                    }
                }
            }
        }
        private Repuesto? _repuestoSeleccionado;
        public Repuesto? RepuestoSeleccionado
        {
            get => _repuestoSeleccionado;
            set
            {
                if (SetProperty(ref _repuestoSeleccionado, value))
                {
                    if (value != null)
                    {
                        NuevoRepuesto = value.Nombre;
                        RepuestoPrecio = value.Precio;
                        CantidadPieza = value.Cantidad;
                    }
                }
            }
        }
        #endregion
        #region Comandos
        public ICommand AgregarMantenimientoCommand { get; }
        public ICommand AgregarRepuestoCommand { get; }
        public ICommand CancelarReparacionCommand { get; }
        public ICommand FinalizarReparacionCommand { get; set; }
        public ICommand GuardarCambiosCommand { get; set; }
        #endregion
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
            //Hacemos que se rellene la lista llamando a la funcion
            VehiculosAsignadosActualmente();
        }

        #region Metodos 
        //Metodo para no poner set { _loquesea = value; OnPropertyChanged(loquesea) y poner simplemente SetProperty(ref Loquesea, value)
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
                CantidadPieza = 0;
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
            if (VehiculoSeleccionado == null)
            {
                MessageBox.Show("Selecciona una reparación para finalizar.");
                return;
            }

            try
            {
                var reparacionId = VehiculoSeleccionado.Id;

                _reparacionRepository.FinalizarReparacionActual(VehiculoSeleccionado,reparacionId);

                MessageBox.Show("Reparación finalizada correctamente.");

                // Actualizar la UI
                VehiculosAsignados.Clear();
                VehiculosAsignadosActualmente();

                // Limpieza selección y datos
                VehiculoSeleccionado = null;
                TrabajoRealizar = string.Empty;
                EstadoSeleccionado = string.Empty;
                RepuestosSeleccionados.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al finalizar la reparación: " + ex.Message);
            }
        }
        private void AgregarPieza(object? obj)
        {
            // Si se está editando un repuesto seleccionado
            if (RepuestoSeleccionado != null)
            {
                RepuestoSeleccionado.Nombre = NuevoRepuesto;
                RepuestoSeleccionado.Precio = RepuestoPrecio;
                RepuestoSeleccionado.Cantidad = CantidadPieza;

                // Notificar que cambió (si el Repuesto implementa INotifyPropertyChanged)
                OnPropertyChanged(nameof(RepuestosSeleccionados));

                // Limpiar selección y campos
                RepuestoSeleccionado = null;
                NuevoRepuesto = string.Empty;
                RepuestoPrecio = 0;
                CantidadPieza = 0;
                return;
            }

            // Si no hay uno seleccionado, agregar nuevo o sumar cantidad
            var existente = RepuestosSeleccionados.FirstOrDefault(r => r.Nombre == NuevoRepuesto);
            if (existente != null)
            {
                existente.Cantidad = CantidadPieza;
                existente.Precio = RepuestoPrecio;
                OnPropertyChanged(nameof(RepuestosSeleccionados));
            }
            else
            {
                RepuestosSeleccionados.Add(new Repuesto
                {
                    Nombre = NuevoRepuesto,
                    Precio = RepuestoPrecio,
                    Cantidad = CantidadPieza
                });
            }

            // Limpiar campos
            NuevoRepuesto = string.Empty;
            RepuestoPrecio = 0;
            CantidadPieza = 0;
        }

        private void AgregarMantenimientoBasico(object? obj)
        {
            var nombresMantenimiento = new[] { "Filtros", "Aceite", "Anticongelante" };

            if (!_mantenimientoAgregado)
            {
                // Agregar mantenimiento básico
                RepuestosSeleccionados.Add(new Repuesto { Nombre = "Filtros", Precio = 20, Cantidad = 3 });
                RepuestosSeleccionados.Add(new Repuesto { Nombre = "Aceite", Precio = 40, Cantidad = 1 });
                RepuestosSeleccionados.Add(new Repuesto { Nombre = "Anticongelante", Precio = 20, Cantidad = 1 });

                _mantenimientoAgregado = true;
            }
            else
            {
                // Quitar mantenimiento básico
                var itemsAEliminar = RepuestosSeleccionados.Where(r => nombresMantenimiento.Contains(r.Nombre)).ToList();
                foreach (var item in itemsAEliminar)
                {
                    RepuestosSeleccionados.Remove(item);
                }

                _mantenimientoAgregado = false;
            }
        }

        private bool PuedeAgregarPieza(object? obj)
        {
            return !string.IsNullOrWhiteSpace(NuevoRepuesto) && RepuestoPrecio > 0 && CantidadPieza > 0;
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
