using Prueba.model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prueba.data
{
    public class FacturaDocument : IDocument
    {
        public Cliente Cliente { get; set; } = new Cliente();
        public Vehiculo Vehiculo { get; set; } = new Vehiculo();
        public Mecanico Mecanico { get; set; } = new Mecanico();
        public List<Repuesto> Repuestos { get; set; } = new List<Repuesto>();
        public decimal Total { get; set; }
        public void Compose(IDocumentContainer container)
        {

            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().Column(column =>
                {
                    column.Spacing(15);

                    // Título y Fecha
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("FACTURA TALLER MANUEL")
                            .Bold().FontSize(20).AlignLeft();

                        row.ConstantItem(200).AlignRight().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}")
                            .FontSize(12);
                    });

                    // Línea separadora azul
                    column.Item().LineHorizontal(1).LineColor(Colors.Blue.Medium);

                    // Cliente
                    column.Item().Text("Cliente").Bold().FontSize(14);
                    column.Item().Text(text =>
                    {
                        text.Span($"Nombre: {Cliente?.Nombre}    ");
                        text.Span($"DNI: {Cliente?.Dni}    ");
                        text.Span($"Teléfono: {Cliente?.Telefono}");
                    });

                    // Vehículo
                    column.Item().Text("Vehículo").Bold().FontSize(14);
                    column.Item().Text(text =>
                    {
                        text.Span($"Matrícula: {Vehiculo?.Matricula}    ");
                        text.Span($"Marca: {Vehiculo?.Marca}    ");
                        text.Span($"Modelo: {Vehiculo?.Modelo}");
                    });

                    // Piezas utilizadas
                    if (Repuestos?.Any() == true)
                    {
                        column.Item().PaddingTop(15).Text("Piezas Utilizadas").Bold().FontSize(14);
                        foreach (var pieza in Repuestos)
                        {
                            column.Item().Text($"{pieza.Nombre} ------- {pieza.Precio:C}");
                        }
                    }

                    // Total y Mecánico al final
                    column.Item().PaddingTop(30).Row(row =>
                    {
                        row.RelativeItem().Text($"TOTAL A PAGAR: {Total:C}")
                            .FontSize(14).Bold().FontColor(Colors.Green.Medium);

                        row.RelativeItem().AlignRight().Text($"Mecánico Asignado: {Mecanico?.Nombre}")
                            .FontSize(12);
                    });
                });
            });
        }
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
