using GestionReparaciones.model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GestionReparaciones.data
{
    public class FacturaDocument : IDocument
    {
        #region Campos
        public Cliente Cliente { get; set; } = new Cliente();
        public bool DesdeHistorial { get; set; } = false;

        public Vehiculo Vehiculo { get; set; } = new Vehiculo();
        public Mecanico Mecanico { get; set; } = new Mecanico();
        public List<RepuestoUsadoDTO> RepuestosUsados { get; set; } = new List<RepuestoUsadoDTO>();
        #endregion

        public decimal Total { get; set; }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Height(80)
                    .Row(row =>
                    {
                        row.ConstantItem(100).Height(60).AlignMiddle().AlignCenter().Background("#2A4759").Element(container =>
                        {
                            container.Text("🛠")
                                     .FontSize(30)
                                     .FontColor("#F79B72")
                                     .AlignCenter();
                        });
                        // Datos del taller
                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(DatosConstantesEstaticos.NombreTaller).Bold().FontSize(18);
                            column.Item().Text($"{DatosConstantesEstaticos.Calle}, {DatosConstantesEstaticos.Ciudad}, {DatosConstantesEstaticos.Municipio}");
                            column.Item().Text($"Tel:{DatosConstantesEstaticos.Telefono} || Email:{DatosConstantesEstaticos.Email}");
                            column.Item().Text($"CIF: {DatosConstantesEstaticos.CIF}");
                        });

                        row.ConstantItem(120).AlignRight().Column(col =>
                        {
                            col.Item().Text("FACTURA").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}");
                            col.Item().Text($"Factura Nº: {Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}");
                        });
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Datos Cliente y Vehiculo
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Cliente").Bold().FontSize(14).Underline();
                                col.Item().Text($"Nombre: {Cliente?.Nombre}");
                                col.Item().Text($"DNI: {Cliente?.Dni}");
                                col.Item().Text($"Teléfono: {Cliente?.Telefono}");
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Vehículo").Bold().FontSize(14).Underline();
                                col.Item().Text($"Matrícula: {Vehiculo?.Matricula}");
                                col.Item().Text($"MarcaVehiculo: {Vehiculo?.Marca}");
                                col.Item().Text($"ModeloVehiculo: {Vehiculo?.Modelo}");
                            });
                        });

                        // Tabla de repuestos
                        if (RepuestosUsados?.Any() == true)
                        {
                            column.Item().PaddingTop(15).Text("Piezas Utilizadas").Bold().FontSize(14);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(5);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Descripción").Bold();
                                    header.Cell().Text("Cantidad").Bold().AlignRight();
                                    header.Cell().Text("Precio Unitario").Bold().AlignRight();
                                    header.Cell().Text("Total").Bold().AlignRight();
                                });

                                foreach (var pieza in RepuestosUsados)
                                {
                                    table.Cell().Text(pieza.Nombre);
                                    table.Cell().Text(pieza.Cantidad.ToString()).AlignRight();
                                    table.Cell().Text($"{pieza.Precio:C}").AlignRight();
                                    table.Cell().Text($"{(pieza.Precio * pieza.Cantidad):C}").AlignRight();
                                }
                            });
                        }
                        else
                        {
                            // Mensaje si no hay repuestos usados
                            column.Item().PaddingTop(15)
                                  .Text("Los repuestos utilizados en esta factura han sido eliminados")
                                  .Italic()
                                  .FontSize(12)
                                  .FontColor(Colors.Grey.Darken1);
                        }

                        // Resumen financiero
                        var subtotalRepuestos = RepuestosUsados?.Sum(r => r.Precio * r.Cantidad) ?? 0m;
                        var manoDeObra = DatosConstantesEstaticos.ManoObra;
                        var baseImponible = subtotalRepuestos + manoDeObra;
                        var iva = baseImponible * (DatosConstantesEstaticos.Iva / 100m);
                        decimal totalFinal = Total;
                        
                        column.Item().PaddingTop(20).AlignRight().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal piezas:");
                                row.ConstantItem(100).Text($"{subtotalRepuestos:C}");
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Mano de Obra:");
                                row.ConstantItem(100).Text($"{manoDeObra:C}");
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"IVA ({DatosConstantesEstaticos.Iva})");
                                row.ConstantItem(100).Text($"{iva:C}");
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL A PAGAR:").Bold().FontSize(14).FontColor(Colors.Green.Darken1);
                                row.ConstantItem(100).Text($"{totalFinal:C}").Bold().FontSize(14).FontColor(Colors.Green.Darken1);
                            });
                            // Mensaje adicional si no hay repuestos usados
                            if (RepuestosUsados == null || !RepuestosUsados.Any())
                            {
                                col.Item().PaddingTop(5)
                                    .Text("(El total no se ha cambiado inclusive los repuestos esten borrados)")
                                    .Italic()
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken2);
                            }
                        });

                        // Mecanico asignado
                        column.Item().PaddingTop(20).Text($"Mecánico asignado: {Mecanico?.Nombre}")
                            .Italic()
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Footer() //Pie de pagina
                    .AlignCenter()
                    .Text($"Gracias por confiar en {DatosConstantesEstaticos.NombreTaller}. Para dudas o reclamaciones, contacte al +34 666 666 666.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
