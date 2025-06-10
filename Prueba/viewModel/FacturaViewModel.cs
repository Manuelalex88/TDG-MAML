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
using System.Threading;
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

        private FacturaVehiculoClienteDTO _facturaSeleccionada;
        public string _nombreMecanico { get; set; } = string.Empty;
        public string IdMecanico { get; set; }
        private readonly FacturaRepository _facturaRepository;
        private readonly HistorialFacturaRepository _historialFacturaRepository;
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
                    // Notificar cambio en Total
                    OnPropertyChanged(nameof(Total));
                }
            }
        }

        public decimal Total
        {
            //Si no esta seleccionada la factura no suma la mano de obra
            get => (FacturaSeleccionada?.Id != null && FacturaSeleccionada.Id > 0)
           ? FacturaSeleccionada.Total + DatosConstantes.ManoDeObra
           : 0m;
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
            IdMecanico = idMecanico ?? string.Empty;

            //Instancias
            _facturasPendientes = new ObservableCollection<FacturaVehiculoClienteDTO>();
            _facturaSeleccionada = CrearFacturaPorDefecto();
            _facturaRepository = new FacturaRepository();
            _historialFacturaRepository = new HistorialFacturaRepository();
            FacturasPendientes = new ObservableCollection<FacturaVehiculoClienteDTO>();

            //Comandos
            ConfirmarFacturaCommand = new comandoViewModel(ConfirmarFactura, PuedeGenerarFactura);
            MostrarFacturasPendientesCommand = new comandoViewModel(MostrarFacturasPendientes);
            EliminarFacturaCommand = new comandoViewModel(EliminarLaFactura, PuedeEliminar);

            //Metodos
            MostrarFacturasPendientes(null);
        }

        #region Metodos
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;

            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
        }
        private FacturaVehiculoClienteDTO CrearFacturaPorDefecto()
        {
            //Creamos una factura por defecto para las comprobaciones y que esta no sea null
            //Por defecto Postgresql no inicia el ID secuencial con 0 sino con 1
            return new FacturaVehiculoClienteDTO
            {
                Id = 0,
                FechaEmision = DateTime.MinValue,
                Total = 0m,
                Pagado = false,
                Matricula = string.Empty,
                ClienteNombre = string.Empty,
                Dni = string.Empty,
                Telefono = string.Empty,
                Marca = string.Empty,
                Modelo = string.Empty
            };
        }


        private void MostrarFacturasPendientes(object? obj)
        {
            if (FacturasPendientes == null)
                FacturasPendientes = new ObservableCollection<FacturaVehiculoClienteDTO>();

            try
            {
                FacturasPendientes.Clear();

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
            return FacturaSeleccionada != null && FacturaSeleccionada.Id != 0;
        }

        private bool PuedeEliminar(object? obj)
        {
            return FacturaSeleccionada != null && FacturaSeleccionada.Id != 0;
        }

        private void EliminarLaFactura(object obj)
        {
            
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
        private void ConfirmarFactura(object obj){

            try
            {
                if (FacturaSeleccionada == null || FacturaSeleccionada.Id == 0)
                {
                    MessageBox.Show("Selecciona una factura válida antes de confirmar.");
                    return;
                }

                
                var historial = new HistorialFactura
                {
                    IdFactura = FacturaSeleccionada.Id,
                    DniCliente = FacturaSeleccionada.Dni,
                    NombreCliente = FacturaSeleccionada.ClienteNombre,
                    TelefonoCliente = FacturaSeleccionada.Telefono,
                    VehiculoMatricula = FacturaSeleccionada.Matricula,
                    VehiculoMarca = FacturaSeleccionada.Marca,
                    VehiculoModelo = FacturaSeleccionada.Modelo,
                    MecanicoNombre = NombreMecanico,
                    MecanicoId = IdMecanico,
                    FechaEmision = DateTime.Now,
                    Total = FacturaSeleccionada.Total + DatosConstantes.ManoDeObra
                };

                _historialFacturaRepository.GuardarFacturaEnHistorial(historial);

                
                GenerarFacturaPDF();
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                MessageBox.Show("Error al confirmar la factura:\n\n" + ex.Message);
            }
        }

        private void GenerarFacturaPDF()
        {
            
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

                var repuestosUsados = _facturaRepository.ObtenerRepuestosUsadosPorReparacion(FacturaSeleccionada.Id);
                if (repuestosUsados == null || !repuestosUsados.Any())
                {
                    MessageBox.Show("No se encontraron repuestos usados para esta reparación.(Una reparacion sin repuestos no tiene sentido)");
                    return;
                }
                decimal total = repuestosUsados.Sum(r => r.Precio * r.Cantidad)+DatosConstantes.ManoDeObra;

                var factura = new FacturaDocument
                {
                    Cliente = cliente,
                    Vehiculo = vehiculo,
                    Mecanico = mecanico,
                    RepuestosUsados = repuestosUsados,
                    Total = total
                };

                #endregion
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
                string fileName = $"Factura_{timestamp}.pdf";
                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                factura.GeneratePdf(ruta);

                _facturaRepository.MarcarRepuestosComoFacturados(FacturaSeleccionada.Id);
                _facturaRepository.MarcarFacturaComoPagada(FacturaSeleccionada.Id);

                MostrarFacturasPendientes(null);

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
