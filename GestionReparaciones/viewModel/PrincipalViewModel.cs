using GestionReparaciones.view;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GestionReparaciones.model;
using GestionReparaciones.view.childViews;
using System.Collections.ObjectModel;
using GestionReparaciones.data;
using System.Windows.Input;
using System.Windows;
using System.Runtime.CompilerServices;
using QuestPDF.Fluent;
using System.Diagnostics;
using System.IO;


namespace GestionReparaciones.viewModel
{
    public class PrincipalViewModel : BaseViewModel
    {


        #region Listas
        public ObservableCollection<VehiculoReparacionDTO> ReparacionesAsignadas { get; set; }
        private ObservableCollection<HistorialFactura> _facturasMecanico;
        #endregion
        #region Campos      
        private string _textoEntrada;
        private string _nombreMecanico;
        // Repositorios de acceso a datos
        private readonly VehiculoRepository _vehiculoRepository;
        private readonly FacturaRepository _facturaRepository;
        private readonly HistorialFacturaRepository _historialFacturaRepository;
        #endregion
        #region Propiedades
        public ObservableCollection<HistorialFactura> FacturasMecanico
        {
            get => _facturasMecanico;
            set => SetProperty(ref _facturasMecanico, value);
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
            _vehiculoRepository = new VehiculoRepository();
            _facturasMecanico = new ObservableCollection<HistorialFactura>();
            _facturaRepository = new FacturaRepository();
            _historialFacturaRepository = new HistorialFacturaRepository();

            TextoEntrada = "Bienvenido, " + _nombreMecanico;
            ReparacionesAsignadas = new ObservableCollection<VehiculoReparacionDTO>();

            //Instanciar comandos

            DescargarFacturaCommand = new comandoViewModel(DescargarFactura);

            //Metodos
            CargarReparacionesAsignadas(idMecanico);
            CargarFacturasFinalizas(idMecanico);
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
        
        public void DescargarFactura(object obj)
        {
            if (obj is not HistorialFactura facturaSeleccionada)
            {
                MessageBox.Show("No se pudo determinar la factura a descargar.");
                return;
            }

            try
            {
                //Licencia para usarlo gratis el QuestPDF
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                
                // Datos necesarios para el PDF
                var cliente = new Cliente
                {
                    Nombre = facturaSeleccionada.NombreCliente,
                    Dni = facturaSeleccionada.DniCliente,
                    Telefono = facturaSeleccionada.TelefonoCliente
                };

                var vehiculo = new Vehiculo
                {
                    Matricula = facturaSeleccionada.VehiculoMatricula,
                    Marca = facturaSeleccionada.VehiculoMarca,
                    Modelo = facturaSeleccionada.VehiculoModelo
                };

                var mecanico = new Mecanico
                {
                    Nombre = _nombreMecanico
                };

                // Obtener todos los repuestos usados para esta factura
                var repuestosUsados = _facturaRepository.ObtenerRepuestosUsadosPorReparacion(facturaSeleccionada.Id) ?? new List<RepuestoUsadoDTO>();
                decimal total = facturaSeleccionada.Total;

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

                // Abrir el archivo generado (Uso QuestPDF)
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
            if(FacturasMecanico == null)
            {
                FacturasMecanico = new ObservableCollection<HistorialFactura>();
            }

            try
            {
                FacturasMecanico.Clear();

                // Mostrar las facturas
                var lista = _historialFacturaRepository.MostrarFacturasMecanico(idMecanico);
                foreach (var v in lista)
                {
                    FacturasMecanico.Add(v);
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
                // Cargamos las reparaciones que el mecanico tiene asignada
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
