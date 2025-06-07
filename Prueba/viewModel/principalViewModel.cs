using Prueba.view;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prueba.model;
using Prueba.view.childViews;
using System.Collections.ObjectModel;
using Prueba.data;
using System.Windows.Input;
using System.Windows;
using System.Runtime.CompilerServices;
using QuestPDF.Fluent;
using System.Diagnostics;
using System.IO;


namespace Prueba.viewModel
{
    public class PrincipalViewModel : BaseViewModel
    {


        #region Listas
        public ObservableCollection<VehiculoReparacionDTO> ReparacionesAsignadas { get; set; }
        private ObservableCollection<FacturaVehiculoClienteDTO> _misFacturas;
        #endregion
        #region Campos
        private readonly VehiculoRepository _vehiculoRepository;
        private readonly FacturaRepository _facturaRepository;
        public string _textoEntrada;
        private string _nombreMecanico;

        #endregion
        #region Propiedades
        public ObservableCollection<FacturaVehiculoClienteDTO> MisFacturas
        {
            get => _misFacturas;
            set => SetProperty(ref _misFacturas, value);
        }
        public string TextoEntrada
        {
            get => _textoEntrada;
            set => SetProperty(ref _textoEntrada, value);
        }
        #endregion
        #region Comandos
        public ICommand DescargarFacturaCommand { get; }
        
        #endregion
        public PrincipalViewModel()
        {
            //Identidad Mecanico
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name ?? "Desconocido";
            
            //Instanciar
            _textoEntrada = string.Empty;
            _nombreMecanico = identity?.NombreCompleto ?? "Desconocido";
            TextoEntrada = "Bienvenido, " + _nombreMecanico;
            ReparacionesAsignadas = new ObservableCollection<VehiculoReparacionDTO>();
            _vehiculoRepository = new VehiculoRepository();
            _misFacturas = new ObservableCollection<FacturaVehiculoClienteDTO>();
            _facturaRepository = new FacturaRepository();

            
            DescargarFacturaCommand = new comandoViewModel(DescargarFactura);
            //Metodos
            CargarReparacionesAsignadas(idMecanico);
            CargarFacturasFinalizas(idMecanico);
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
        
        
        public void DescargarFactura(object obj)
        {
            if (obj is not FacturaVehiculoClienteDTO facturaSeleccionada)
            {
                MessageBox.Show("No se pudo determinar la factura a descargar.");
                return;
            }

            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                
                // Datos necesarios para el PDF
                var cliente = new Cliente
                {
                    Nombre = facturaSeleccionada.ClienteNombre,
                    Dni = facturaSeleccionada.Dni,
                    Telefono = facturaSeleccionada.Telefono
                };

                var vehiculo = new Vehiculo
                {
                    Matricula = facturaSeleccionada.Matricula,
                    Marca = facturaSeleccionada.Marca,
                    Modelo = facturaSeleccionada.Modelo
                };

                var mecanico = new Mecanico
                {
                    Nombre = _nombreMecanico
                };

                // Obtener todos los repuestos usados para esta factura
                var repuestosUsados = _facturaRepository.ObtenerRepuestosUsadosPorReparacion(facturaSeleccionada.Id) ?? new List<RepuestoUsadoDTO>();
                decimal total = repuestosUsados.Sum(r => r.Precio * r.Cantidad);

                var factura = new FacturaDocument
                {
                    Cliente = cliente,
                    Vehiculo = vehiculo,
                    Mecanico = mecanico,
                    RepuestosUsados = repuestosUsados,
                    Total = total
                };

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
                string fileName = $"Factura_{timestamp}.pdf";
                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                factura.GeneratePdf(ruta);

                _facturaRepository.MarcarRepuestosComoFacturados(facturaSeleccionada.Id);
                _facturaRepository.MarcarFacturaComoPagada(facturaSeleccionada.Id);

                // Actualizamos la lista
                CargarFacturasFinalizas(Thread.CurrentPrincipal?.Identity?.Name ?? "");

                // Abrir el archivo generado
                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                MessageBox.Show($"Error al generar o abrir el PDF:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CargarFacturasFinalizas(string idMecanico)
        {
            if(MisFacturas == null)
            {
                MisFacturas = new ObservableCollection<FacturaVehiculoClienteDTO>();
            }

            try
            {
                MisFacturas.Clear();


                var lista = _facturaRepository.ObtenerFacturasPagadasPorMecanico(idMecanico);
                foreach (var v in lista)
                {
                    MisFacturas.Add(v);
                }
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Error al cargar los vehículos asignados:\n" +
                "Mensaje: " + ex.Message + "\n" +
                "Fuente: " + ex.Source + "\n" +
                "StackTrace: " + ex.StackTrace);
            }
        }
        private void CargarReparacionesAsignadas(string idMecanico)
        {

            try
            {
                var lista = _vehiculoRepository.ObtenerVehiculosAsignados(idMecanico);
                foreach (var v in lista)
                {
                    ReparacionesAsignadas.Add(v);
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
