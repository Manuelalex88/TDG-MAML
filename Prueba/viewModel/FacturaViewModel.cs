using Prueba.data;
using Prueba.model;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class FacturaViewModel : BaseViewModel
    {   // Campos
        public string _nombreMecanico { get; set; } = string.Empty;
        // Propiedades
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

        // Comandos
        public ICommand ConfirmarFacturaCommand { get; }

        public FacturaViewModel()
        {
            ConfirmarFacturaCommand = new comandoViewModel(GenerarFacturaPDF);
            NombreMecanico = UserData.Nombre ?? string.Empty;
        }

        private void GenerarFacturaPDF(object obj)
        {
            try
            {
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                #region Datos
                var cliente = new Cliente
                {
                    Nombre = "Juan Pérez",
                    Dni = "12345678A",
                    Telefono = "654 321 987"
                };

                var vehiculo = new Vehiculo
                {
                    Matricula = "1234-ABC",
                    Marca = "Toyota",
                    Modelo = "Corolla"
                };

                var mecanico = new Mecanico
                {
                    Nombre = "Manuel García"
                };

                var Repuesto = new List<Repuesto>
                {
                    new Repuesto { Nombre = "Filtro de aceite", Precio = 15.99m },
                    new Repuesto { Nombre = "Aceite 5W30", Precio = 29.50m },
                    new Repuesto { Nombre = "Pastillas de freno", Precio = 45.00m }
                };

                var total = Repuesto.Sum(p => p.Precio);
                #endregion
                var factura = new FacturaDocument
                {
                    Cliente = cliente,
                    Vehiculo = vehiculo,
                    Mecanico = mecanico,
                    Repuestos = Repuesto,
                    Total = total
                };

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
                string fileName = $"Factura_{timestamp}.pdf";
                string ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                factura.GeneratePdf(ruta);

                // Abrir PDF
                Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                // Muestra el error en una ventana emergente
                MessageBox.Show($"Error al generar o abrir el PDF:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
