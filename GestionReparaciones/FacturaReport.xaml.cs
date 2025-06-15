using PdfSharp.Drawing;
using PdfSharp.Pdf;
using GestionReparaciones.data;
using GestionReparaciones.model;
using QuestPDF.Fluent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Shapes;

namespace GestionReparaciones
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FacturaReport : Window
    {
        public FacturaReport()
        {
            InitializeComponent();
            GenerarFacturaPDF();
        }
        private void GenerarFacturaPDF()
        {
            // Datos de GestionReparaciones
            var cliente = new Cliente { Nombre = "Juan Pérez", Dni = "12345678A", Telefono = "654321987" };
            var vehiculo = new Vehiculo { Marca = "Toyota", Modelo = "Corolla", Matricula = "1234ABC" };
            var mecanico = new Mecanico { Nombre = "Luis Gómez"};

            decimal total = 345.50m;

            var factura = new FacturaDocument
            {
                Cliente = cliente,
                Vehiculo = vehiculo,
                Mecanico = mecanico,
                Total = total
            };

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
            string fileName = $"Factura_{timestamp}.pdf";
            string ruta = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            factura.GeneratePdf(ruta);

            Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
        }
    }
}