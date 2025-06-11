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
        public ObservableCollection<RepuestoUsadoDTO> RepuestosUsados { get; set; } = new();

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
                    RepuestosUsados.Clear();

                    if (value != null)
                    {
                        // Obtener ID de la reparacion actual
                        int reparacionId = _reparacionRepository.ObtenerIdReparacionPorMatricula(value.Matricula);
                        MessageBox.Show(reparacionId.ToString());
                        // Cargar repuestos usados en la reparacion
                        var repuestos = _reparacionRepository.ObtenerRepuestosUsados(reparacionId);

                        foreach (var r in repuestos)
                        {
                            RepuestosUsados.Add(r);
                        }
                        // Actualizamos el estado de _mantenimientoAgregado para que se elimine correctamente
                        var nombresMantenimiento = new[] { "FILTROS", "ACEITE", "ANTICONGELANTE" };
                        _mantenimientoAgregado = RepuestosUsados.Any(r => nombresMantenimiento.Contains(r.Nombre));
                    }
                }
            }
        }
        private RepuestoUsadoDTO? _repuestoSeleccionado;
        public RepuestoUsadoDTO? RepuestoSeleccionado
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
        public ICommand BorrarRepuestoCommand { get; }
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
            CancelarReparacionCommand = new comandoViewModel(CancelarReparacion, PuedeCancelar);
            FinalizarReparacionCommand = new comandoViewModel(FinalizarReparacion, PuedeFinalizar);
            GuardarCambiosCommand = new comandoViewModel(GuardarCambios);
            BorrarRepuestoCommand = new comandoViewModel(BorrarRepuesto);
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
            //Si no hay nada que guardar no se guarda
            bool hayCambios = RepuestosUsados.Any();

            if (!hayCambios)
            {
                MessageBox.Show("No hay piezas para guardar.");
                return;
            }

            try
            {
                // Para que no haya duplicados
                var repuestosNoRepetidos = RepuestosUsados
                    .GroupBy(r => r.Nombre)
                    .Select(g => new RepuestoUsadoDTO
                    {
                        Nombre = g.Key,
                        Precio = g.Last().Precio,  
                        Cantidad = g.Last().Cantidad 
                    }).ToList();

                _reparacionRepository.GuardarCambiosReparacion(
                    VehiculoSeleccionado,
                    TrabajoRealizar,
                    EstadoSeleccionado,
                    repuestosNoRepetidos
                );

                MessageBox.Show("Cambios guardados correctamente.");

                // Limpiar los campos después de guardar
                TrabajoRealizar = string.Empty;
                EstadoSeleccionado = string.Empty;
                NuevoRepuesto = string.Empty;
                RepuestoPrecio = 0;
                CantidadPieza = 0;
                RepuestosUsados.Clear();
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
        private bool PuedeFinalizar(object? obj)
        {
            if (VehiculoSeleccionado == null || EstadoSeleccionado != "En Reparacion")
                return false;

            int reparacionId = _reparacionRepository.ObtenerIdReparacionPorMatricula(VehiculoSeleccionado.Matricula);
            var repuestosGuardados = _reparacionRepository.ObtenerRepuestosUsados(reparacionId);

            return repuestosGuardados.Any();
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

                // Finalizar reparación
                var reparacionId = VehiculoSeleccionado.IdReparacion;
                _reparacionRepository.FinalizarReparacionActual(VehiculoSeleccionado, reparacionId);

                MessageBox.Show("Reparación finalizada correctamente.");

                // Actualizar UI
                VehiculosAsignados.Clear();
                VehiculosAsignadosActualmente();

                VehiculoSeleccionado = null;
                TrabajoRealizar = string.Empty;
                EstadoSeleccionado = string.Empty;
                RepuestosUsados.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al finalizar la reparación: " + ex.Message);
            }

        }
        private void AgregarPieza(object? obj)
        {

            if (RepuestoSeleccionado != null)
            {
                // Obtén índice del repuesto seleccionado en la colección
                int index = RepuestosUsados.IndexOf(RepuestoSeleccionado);
                if (index >= 0)
                {
                    // Reemplaza el objeto con uno nuevo con los valores editados
                    RepuestosUsados[index] = new RepuestoUsadoDTO
                    {
                        Nombre = NuevoRepuesto,
                        Precio = RepuestoPrecio,
                        Cantidad = CantidadPieza
                    };
                }

                // Limpiar selección y campos
                RepuestoSeleccionado = null;
                NuevoRepuesto = string.Empty;
                RepuestoPrecio = 0;
                CantidadPieza = 0;
                return;
            }

            // Si no hay seleccionado, agregar nuevo o sumar cantidad
            var existente = RepuestosUsados.FirstOrDefault(r => r.Nombre == NuevoRepuesto);
            if (existente != null)
            {
                existente.Cantidad = CantidadPieza;
                existente.Precio = RepuestoPrecio;
            }
            else
            {
                RepuestosUsados.Add(new RepuestoUsadoDTO
                {
                    Nombre = NuevoRepuesto,
                    Precio = RepuestoPrecio,
                    Cantidad = CantidadPieza
                });
            }

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
                RepuestosUsados.Add(new RepuestoUsadoDTO { Nombre = "Filtros", Precio = 20, Cantidad = 3 });
                RepuestosUsados.Add(new RepuestoUsadoDTO { Nombre = "Aceite", Precio = 40, Cantidad = 1 });
                RepuestosUsados.Add(new RepuestoUsadoDTO { Nombre = "Anticongelante", Precio = 20, Cantidad = 1 });

                _mantenimientoAgregado = true;
            }
            else
            {
                // Quitar mantenimiento básico
                var itemsAEliminar = RepuestosUsados.Where(r => nombresMantenimiento.Contains(r.Nombre)).ToList();
                foreach (var item in itemsAEliminar)
                {
                    RepuestosUsados.Remove(item);
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
                RepuestosUsados.Clear();
                VehiculosAsignados.Remove(VehiculoSeleccionado);
                VehiculoSeleccionado = null;

                MessageBox.Show("Reparación cancelada correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cancelar la reparación: " + ex.Message);
            }
        }

        private bool PuedeCancelar(object? obj)
        {
            return VehiculoSeleccionado != null;
        }

        private void VehiculosAsignadosActualmente()
        {
            // Obtener el ID del mecánico desde el hilo actual
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name ?? "Desconocido";

            if (string.IsNullOrEmpty(idMecanico))
            {
                MessageBox.Show("El ID del mecánico no está definido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                VehiculosAsignados.Clear();
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
        private void BorrarRepuesto(object obj)
        {
            if (obj is not RepuestoUsadoDTO repuestoBorrar)
            {
                MessageBox.Show("No se pudo determinar el repuesto a borrar.");
                return;
            }
            try
            {
                RepuestosUsados.Remove(repuestoBorrar);

                if (VehiculoSeleccionado != null)
                {
                    _reparacionRepository.EliminarRepuestoDeReparacion(repuestoBorrar.Id);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al borrar el repuesto:\n" +
                                    "Mensaje: " + ex.Message + "\n" +
                                    "Fuente: " + ex.Source + "\n" +
                                    "StackTrace: " + ex.StackTrace);
            }

        }
    }
    #endregion
}

