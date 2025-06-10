using Prueba.data;
using Prueba.model;
using Prueba.repository;
using Prueba.view.adminChildViews;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static QuestPDF.Helpers.Colors;

namespace Prueba.viewModel.viewModelAdmin
{
    public class FacturasAdminViewModel : BaseViewModel
    {
        #region Lista
        public ObservableCollection<FacturaVehiculoClienteDTO> _facturasList;
        #endregion

        #region Campos
        private FacturaRepository _facturaRepository;
        private FacturaVehiculoClienteDTO _facturaSeleccionada;
        private string _nombreMecanico;

        #endregion

        #region Propiedades
        public ObservableCollection<FacturaVehiculoClienteDTO> FacturaList
        {
            get => _facturasList;
            set => SetProperty(ref _facturasList, value);
        }
        public FacturaVehiculoClienteDTO FacturaSeleccionada
        {
            get => _facturaSeleccionada;
            set => SetProperty(ref _facturaSeleccionada, value);
        }

        
        #endregion

        #region Comandos
        public ICommand MostrarFacturasAdminCommand { get;}
        public ICommand BorrarFacturaCommand { get; }
        public ICommand DescargarFacturaCommand { get; }
        #endregion



        public FacturasAdminViewModel()
        {
            //Identidad Mecanico
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name ?? "Desconocido";
            //Instanciar
            _facturasList = new ObservableCollection<FacturaVehiculoClienteDTO>();
            _facturaRepository = new FacturaRepository();
            _facturaSeleccionada = new FacturaVehiculoClienteDTO();
            _nombreMecanico = identity?.NombreCompleto ?? "Desconocido";

            //Comando
            BorrarFacturaCommand = new comandoViewModel(BorrarFactura);
            MostrarFacturasAdminCommand = new comandoViewModel(MostrarFacturasAdmin);
            DescargarFacturaCommand = new comandoViewModel(DescargarFacturaAdmin);

            //Mostrar la lista
            MostrarFacturasAdminCommand.Execute(null);
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
        public void MostrarFacturasAdmin (object obj)
        {
            try
            {
                FacturaList.Clear();

                var lista = _facturaRepository.MostrarFacturaAdmin();
                foreach (var v in lista)
                {
                    FacturaList.Add(v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar a las Facturas:\n" +
                    "Mensaje: " + ex.Message + "\n" +
                    "Fuente: " + ex.Source + "\n" +
                    "StackTrace: " + ex.StackTrace);
            }
        }
        public void BorrarFactura(Object obj)
        {
            if(obj is not FacturaVehiculoClienteDTO facturaEliminar)
            {
                MessageBox.Show("No se pudo determinar la facturaEliminar a borrar.");
                return;
            }

            try
            {
                _facturaRepository.EliminarFacturaSeleccionada(facturaEliminar.Id);
                FacturaList.Remove(facturaEliminar);

                MessageBox.Show("Factura eliminada correctamente");
            }catch (Exception ex)
            {
                MessageBox.Show("Error al borrar a las Facturas:\n" +
                    "Mensaje: " + ex.Message + "\n" +
                    "Fuente: " + ex.Source + "\n" +
                    "StackTrace: " + ex.StackTrace);
            }
        }
        public void DescargarFacturaAdmin(Object obj)
        {
            if (obj is not FacturaVehiculoClienteDTO facturaSeleccionada)
            {
                MessageBox.Show("No se pudo determinar la factura a descargar.");
                return;
            }

            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

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
               

                var factura = new FacturaDocument
                {
                    Cliente = cliente,
                    Vehiculo = vehiculo,
                    Mecanico = mecanico,
                    RepuestosUsados = repuestosUsados,
                    Total = facturaSeleccionada.Total,
                };
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
                string fileName = $"Factura_{timestamp}.pdf";
                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                factura.GeneratePdf(ruta);

                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error al descargar la factura:\n" +
                    "Mensaje: " + ex.Message + "\n" +
                    "Fuente: " + ex.Source + "\n" +
                    "StackTrace: " + ex.StackTrace);
            }
        }
        #endregion
    }
}
