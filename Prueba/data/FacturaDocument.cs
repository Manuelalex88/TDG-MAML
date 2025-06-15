using Prueba.model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prueba.data
{
    public class FacturaDocument : IDocument
    {
        public Cliente Cliente { get; set; } = new Cliente();
        string mensaje = $"Gracias por confiar en {DatosConstantes.NombreTaller}. Para dudas o reclamaciones, contacte al +34 666 666 666.";
        string direccion = $"{DatosConstantes.Calle} , {DatosConstantes.Ciudad} , {DatosConstantes.Municipio}"; 
        string telefonoEmail = $"Tel:{DatosConstantes.Telefono} || Email:{DatosConstantes.Email}";
        string ivatext = $"IVA ({DatosConstantes.Iva})";
        public Vehiculo Vehiculo { get; set; } = new Vehiculo();
        public Mecanico Mecanico { get; set; } = new Mecanico();
        public List<RepuestoUsadoDTO> RepuestosUsados { get; set; } = new List<RepuestoUsadoDTO>();

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

                        row.RelativeItem().Column(column =>
                        {
                            column.Item().Text(DatosConstantes.NombreTaller).Bold().FontSize(18);
                            column.Item().Text(direccion);
                            column.Item().Text(telefonoEmail);
                            column.Item().Text($"CIF: {DatosConstantes.CIF}");
                        });

                        row.ConstantItem(120).AlignRight().Column(col =>
                        {
                            col.Item().Text("FACTURA").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}");
                            col.Item().Text($"Factura Nº: {Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"); //Numero aleatorio por GUID
                        });
                    });

                page.Content()
                    .PaddingVertical(10)
                    .Column(column =>
                    {
                        column.Spacing(10);

                        // Datos del Cliente y Vehículo lado a lado
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
                                    columns.RelativeColumn(5); // Nombre pieza
                                    columns.RelativeColumn(2); // Cantidad
                                    columns.RelativeColumn(3); // Precio unitario
                                    columns.RelativeColumn(3); // Total línea
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

                        // Resumen financiero
                        var subtotalRepuestos = RepuestosUsados?.Sum(r => r.Precio * r.Cantidad) ?? 0m;
                        var manoDeObra = DatosConstantes.ManoObra;
                        var baseImponible = subtotalRepuestos + manoDeObra;
                        var iva = baseImponible * (DatosConstantes.Iva / 100m);
                        var totalFinal = baseImponible + iva;

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
                                row.ConstantItem(100).Text($"{DatosConstantes.ManoObra:C}");
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text(ivatext);
                                row.ConstantItem(100).Text($"{iva:C}");
                            });
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL A PAGAR:").Bold().FontSize(14).FontColor(Colors.Green.Darken1);
                                row.ConstantItem(100).Text($"{totalFinal:C}").Bold().FontSize(14).FontColor(Colors.Green.Darken1);
                            });
                        });

                        // Mecanico asignado
                        column.Item().PaddingTop(20).Text($"Mecánico asignado: {Mecanico?.Nombre}")
                            .Italic()
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(mensaje)
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
