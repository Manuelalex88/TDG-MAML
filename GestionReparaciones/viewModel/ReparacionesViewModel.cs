using GestionReparaciones.data;
using GestionReparaciones.model;
using GestionReparaciones.view.childViews;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace GestionReparaciones.viewModel
{
    public class ReparacionesViewModel : BaseViewModel
    {
        #region Listas
        // Lista de posibles estados de una reparacion
        private List<string> _listaEstadoReparacion { get; set; } = new()
        {
            "Problema sin identificar", "Diagnosticando", "Esperando Repuesto", "En Reparacion"
        };
        // Lista de vehiculos actualmente asignados al mecanico
        public ObservableCollection<VehiculoReparacionDTO> VehiculosAsignados { get; set; } = new();
        // Lista de repuestos usados para la reparacion del vehiculo seleccionado
        public ObservableCollection<RepuestoUsadoDTO> RepuestosUsados { get; set; } = new();

        // Propiedades publicas de solo lectura para enlazar en la vista
        public List<string> ListaEstadoReparacion => _listaEstadoReparacion;
        #endregion
        #region Campos
        private VehiculoReparacionDTO? _vehiculoSeleccionado;
        private RepuestoUsadoDTO? _repuestoSeleccionado;
        private bool _mantenimientoAgregado = false;
        private string _trabajoRealizar = string.Empty;
        private string _estadoSeleccionado = string.Empty;
        private string _nuevoRepuesto = string.Empty;
        private decimal _repuestoPrecio = 0;
        private int _cantidad = 0;

        // Repositorios de acceso a datos
        private readonly ReparacionRepository _reparacionRepository;
        private readonly VehiculoRepository _vehiculoRepository;
        #endregion
        #region Propiedades

        public string TrabajoRealizar
        {
            get => _trabajoRealizar;
            set => SetProperty(ref _trabajoRealizar, value);
        }
        public string EstadoSeleccionado
        {
            get => _estadoSeleccionado;
            set => SetProperty(ref _estadoSeleccionado, value);
        }
        public string NuevoRepuesto
        {
            get => _nuevoRepuesto;
            set => SetProperty(ref _nuevoRepuesto, value.ToUpperInvariant());
        }
        public decimal RepuestoPrecio
        {
            get => _repuestoPrecio;
            set => SetProperty(ref _repuestoPrecio, value);
        }
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
            //Instaciar

            _reparacionRepository = new ReparacionRepository();
            _vehiculoRepository = new VehiculoRepository();
            // Inicializacion de comandos
            AgregarMantenimientoCommand = new comandoViewModel(AgregarMantenimientoBasico);
            AgregarRepuestoCommand = new comandoViewModel(AgregarPieza, PuedeAgregarPieza);
            CancelarReparacionCommand = new comandoViewModel(CancelarReparacion, PuedeCancelar);
            FinalizarReparacionCommand = new comandoViewModel(FinalizarReparacion, PuedeFinalizar);
            GuardarCambiosCommand = new comandoViewModel(GuardarCambios);
            BorrarRepuestoCommand = new comandoViewModel(BorrarRepuesto);

            // Carga inicial de vehiculos
            VehiculosAsignadosActualmente();
        }

        #region Metodos 
        // Metodo auxiliar para simplificar el OnPropertyChanged (No agregar lo mismo en todas las propiedades)
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

                // Limpiar los campos despues de guardar
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
        private void FinalizarReparacion(object obj)
        {

            if (VehiculoSeleccionado == null)
            {
                MessageBox.Show("Selecciona una reparación para finalizar.");
                return;
            }

            try
            {

                // Finalizar reparacion
                var reparacionId = VehiculoSeleccionado.IdReparacion;
                _reparacionRepository.FinalizarReparacionActual(VehiculoSeleccionado, reparacionId);

                MessageBox.Show("Reparación finalizada correctamente.");

                // Actualizar la interfaz
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
                // Obtener el indice del repuesto seleccionado en la coleccion
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

                // Limpiar seleccion y campos
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
            var nombresMantenimiento = new[] { "FILTROS", "ACEITE", "ANTICONGELANTE" };

            if (!_mantenimientoAgregado)
            {
                // Agregar mantenimiento basico
                RepuestosUsados.Add(new RepuestoUsadoDTO { Nombre = "FILTROS", Precio = 20, Cantidad = 3 });
                RepuestosUsados.Add(new RepuestoUsadoDTO { Nombre = "ACEITE", Precio = 40, Cantidad = 1 });
                RepuestosUsados.Add(new RepuestoUsadoDTO { Nombre = "ANTICONGELANTE", Precio = 20, Cantidad = 1 });

                _mantenimientoAgregado = true;
            }
            else
            {
                // Quitar mantenimiento basico
                var itemsAEliminar = RepuestosUsados.Where(r => nombresMantenimiento.Contains(r.Nombre)).ToList();
                foreach (var item in itemsAEliminar)
                {
                    RepuestosUsados.Remove(item);
                }

                _mantenimientoAgregado = false;
            }
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
                // Cancelamos la reparacion
                _vehiculoRepository.CancelarReparacionPorMatricula(matricula);

                // Limpia la interfaz
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
                // Obtenemos los vehiculos asignados
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
                //Borramos el repuesto
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

        //Metodos de verificacion/validacion
        private bool PuedeCancelar(object? obj)
        {
            return VehiculoSeleccionado != null;
        }
        private bool PuedeAgregarPieza(object? obj)
        {
            return !string.IsNullOrWhiteSpace(NuevoRepuesto) && RepuestoPrecio > 0 && CantidadPieza > 0;
        }
        private bool PuedeFinalizar(object? obj)
        {
            if (VehiculoSeleccionado == null || EstadoSeleccionado != "En Reparacion")
                return false;

            int reparacionId = _reparacionRepository.ObtenerIdReparacionPorMatricula(VehiculoSeleccionado.Matricula);
            var repuestosGuardados = _reparacionRepository.ObtenerRepuestosUsados(reparacionId);

            return repuestosGuardados.Any();
        }

    }
    #endregion
}

