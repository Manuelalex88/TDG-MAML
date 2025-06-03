using Prueba.data;
using Prueba.model;
using Prueba.view.childViews;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class FacturaViewModel : BaseViewModel
    {
        #region Listas
        private ObservableCollection<FacturaVehiculoClienteDTO> _facturasPendientes;
        #endregion
        #region Campos
        private string _modeloVehiculo = string.Empty;
        private string _marcaVehiculo = string.Empty;
        private string _matriculaVehiculo = string.Empty;
        private string _nombreCliente = string.Empty;
        private string _dniCliente = string.Empty;
        private string _telefonoCliente = string.Empty;
        private decimal _precioTotal = decimal.Zero;

        
        private FacturaVehiculoClienteDTO _facturaSeleccionada = new();
        public string _nombreMecanico { get; set; } = string.Empty;
        private readonly FacturaRepository _facturaRepository;
        #endregion
        #region Propiedades
        public string ModeloVehiculo
        {
            get => _modeloVehiculo;
            set => SetProperty(ref _modeloVehiculo, value);
        }
        public string MarcaVehiculo
        {
            get => _marcaVehiculo;
            set => SetProperty(ref _marcaVehiculo, value);
        }
        public string NombreCliente
        {
            get => _nombreCliente;
            set => SetProperty(ref _nombreCliente, value);
        }
        public string NombreMecanico
        {
            get => _nombreMecanico;
            set
            {
                if (_nombreMecanico != value)
                {
                    _nombreMecanico = value;
                    OnPropertyChanged(nameof(NombreMecanico));
                }
            }
        }
        public string DniCliente
        {
            get => _dniCliente;
            set => SetProperty(ref _dniCliente, value);
        }
        public string TelefonoCliente
        {
            get => _telefonoCliente;
            set => SetProperty(ref _telefonoCliente, value);
        }
        public decimal PrecioTotal
        {
            get => _precioTotal;
            set => SetProperty(ref _precioTotal, value);
        }

        public string MatriculaVehiculo
        {
            get => _matriculaVehiculo;
            set => SetProperty(ref _matriculaVehiculo, value);
        }
        public FacturaVehiculoClienteDTO FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set
            {
                if (SetProperty(ref _facturaSeleccionada, value))
                {
                    if (_facturaSeleccionada != null)
                    {
                        ModeloVehiculo = _facturaSeleccionada.Modelo;
                        MarcaVehiculo = _facturaSeleccionada.Marca;
                        MatriculaVehiculo = _facturaSeleccionada.Matricula;
                        NombreCliente = _facturaSeleccionada.ClienteNombre;
                        DniCliente = _facturaSeleccionada.Dni;
                        TelefonoCliente = _facturaSeleccionada.Telefono;
                        CalcularTotalFactura();
                    }
                    else
                    {
                        
                        ModeloVehiculo = string.Empty;
                        MarcaVehiculo = string.Empty;
                        NombreCliente = string.Empty;
                        DniCliente = string.Empty;
                        TelefonoCliente = string.Empty;
                        MatriculaVehiculo = string.Empty;
                    }
                }
            }
        }
        public ObservableCollection<FacturaVehiculoClienteDTO> FacturasPendientes
        {
            get => _facturasPendientes;
            set => SetProperty(ref _facturasPendientes, value);
        }
        #endregion
        #region Comandos
        public ICommand ConfirmarFacturaCommand { get; }
        public ICommand MostrarFacturasPendientesCommand { get; }
        public ICommand EliminarFacturaCommand { get; set; }
        #endregion
        public FacturaViewModel()
        {
            //Identidad Mecanico
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name;
            NombreMecanico = identity?.NombreCompleto ?? "Desconocido";
            //Instancias
            _facturasPendientes = new ObservableCollection<FacturaVehiculoClienteDTO>();
            _facturaSeleccionada = new FacturaVehiculoClienteDTO();
            _facturaRepository = new FacturaRepository();
            FacturasPendientes = new ObservableCollection<FacturaVehiculoClienteDTO>();
            //Comandos
            ConfirmarFacturaCommand = new comandoViewModel(GenerarFacturaPDF, PuedeGenerarFactura);
            MostrarFacturasPendientesCommand = new comandoViewModel(MostrarFacturasPendientes);
            EliminarFacturaCommand = new comandoViewModel(EliminarLaFactura, PuedeEliminar);
            //Metodos
            MostrarFacturasPendientes(null);
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
        private void MostrarFacturasPendientes(object? obj)
        {
            if (FacturasPendientes == null)
                FacturasPendientes = new ObservableCollection<FacturaVehiculoClienteDTO>();
            try
            {
                FacturasPendientes.Clear();

                // Obtener el ID del mecánico desde el hilo actual
                var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
                var idMecanico = identity?.Name;

                if (string.IsNullOrWhiteSpace(idMecanico))
                {
                    return;
                }

                var lista = _facturaRepository.ObtenerFacturasPendientesPorMecanico(idMecanico);
                foreach (var v in lista)
                {
                    FacturasPendientes.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los vehículos asignados:\n" +
                "Mensaje: " + ex.Message + "\n" +
                "Fuente: " + ex.Source + "\n" +
                "StackTrace: " + ex.StackTrace);

            }
        }

        private bool PuedeGenerarFactura(object? obj)
        {
            return FacturaSeleccionada != null && PrecioTotal > 0;
        }
        private bool PuedeEliminar(object? obj)
        {
            return FacturaSeleccionada != null && PrecioTotal > 0;
        }
        private void CalcularTotalFactura()
        {
            if (FacturaSeleccionada == null)
                return;

            var repuestosUsados = _facturaRepository.ObtenerRepuestosUsadosPorReparacion(FacturaSeleccionada.Id);

            var total = repuestosUsados.Sum(p => p.Precio * p.Cantidad);
            PrecioTotal = total;
        }
        private void EliminarLaFactura(object obj)
        {
            if (FacturaSeleccionada == null)
            {
                MessageBox.Show("Selecciona una factura para eliminar.");
                return;
            }

            try
            {
                _facturaRepository.EliminarFacturaSeleccionada(FacturaSeleccionada.Id);

                FacturasPendientes.Remove(FacturaSeleccionada);
                FacturaSeleccionada = null;

                MessageBox.Show("Factura eliminada correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar factura: " + ex.Message);
            }
        }
        private void GenerarFacturaPDF(object obj)
        {
            if (FacturaSeleccionada == null)
            {
                MessageBox.Show("Por favor selecciona una factura antes de generar el PDF.");
                return;
            }
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

                #region Datos
                var cliente = new Cliente
                {
                    Nombre = FacturaSeleccionada.ClienteNombre,
                    Dni = FacturaSeleccionada.Dni,
                    Telefono = FacturaSeleccionada.Telefono
                };

                var vehiculo = new Vehiculo
                {
                    Matricula = FacturaSeleccionada.Matricula,
                    Marca = FacturaSeleccionada.Marca,
                    Modelo = FacturaSeleccionada.Modelo
                };

                var mecanico = new Mecanico
                {
                    Nombre = NombreMecanico
                };

                // Obtén la lista real de repuestos usados para la reparación asociada a la factura seleccionada
                var repuestosUsados = _facturaRepository.ObtenerRepuestosUsadosPorReparacion(FacturaSeleccionada.Id) ?? new List<Repuesto>();
                var total = repuestosUsados.Sum(p => p.Precio);
                #endregion

                var factura = new FacturaDocument
                {
                    Cliente = cliente,
                    Vehiculo = vehiculo,
                    Mecanico = mecanico,
                    Repuestos = repuestosUsados,
                    Total = total
                };

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
                string fileName = $"Factura_{timestamp}.pdf";
                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                factura.GeneratePdf(ruta);
                // Marcamos los repuestos como facturados
                _facturaRepository.MarcarRepuestosComoFacturados(FacturaSeleccionada.Id);

                // Marcamos como pagada la factura
                _facturaRepository.MarcarFacturaComoPagada(FacturaSeleccionada.Id);

                // Actualizamos las facturas para que no salga más
                MostrarFacturasPendientes(null);

                // Abrir PDF
                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                MessageBox.Show($"Error al generar o abrir el PDF:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
