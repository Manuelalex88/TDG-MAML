using Npgsql;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class registrarVehiculosViewModel : BaseViewModel
    {
        //Lista para los datos
        public List<string> ListaMarcas { get; set; } = new List<string>
        {
            "Seat", "Mercedes", "BMW", "FIAT", "FERRARI"
        };

        public List<string> ListaMotivoIngreso { get; set; } = new List<string>
        {
            "Revisión General","Cambio de aceite", "Cambio de filtros", "Diagnóstico de motor","Problema sin identificar"
        };
        //Campos

        public string? _marca { get; set; }
        public string? _modelo { get; set; }
        public string? _matricula { get; set; }
        public string? _motivoIngreso { get; set; }
        public string? _descripcion { get; set; }
        public string? _nombreCliente { get; set; }
        public string? _anio { get; set; }
        public string? _dniCliente { get; set; }
        public string? _telefonoCliente { get; set; }
        public Boolean? _asignar {  get; set; }

        // Propiedades
        public string? Marca
        {
            get => _marca;
            set
            {
                _marca = value;
                cambioPropiedad(nameof(Marca));
            }
        }

        public string? Modelo
        {
            get => _modelo;
            set
            {
                _modelo = value;
                cambioPropiedad(nameof(Modelo));
            }
        }

        public string? Matricula
        {
            get => _matricula;
            set
            {
                _matricula = value;
                cambioPropiedad(nameof(Matricula));
            }
        }

        public string? MotivoIngreso
        {
            get => _motivoIngreso;
            set
            {
                _motivoIngreso = value;
                cambioPropiedad(nameof(MotivoIngreso));
            }
        }

        public string? Descripcion
        {
            get => _descripcion;
            set
            {
                _descripcion = value;
                cambioPropiedad(nameof(Descripcion));
            }
        }

        public string? NombreCliente
        {
            get => _nombreCliente;
            set
            {
                _nombreCliente = value;
                cambioPropiedad(nameof(NombreCliente));
            }
        }

        public string? Anio
        {
            get => _anio;
            set
            {
                _anio = value;
                cambioPropiedad(nameof(Anio));
            }
        }

        public string? DniCliente
        {
            get => _dniCliente;
            set
            {
                _dniCliente = value;
                cambioPropiedad(nameof(DniCliente));
            }
        }

        public string? TelefonoCliente
        {
            get => _telefonoCliente;
            set
            {
                _telefonoCliente = value;
                cambioPropiedad(nameof(TelefonoCliente));
            }
        }
        public Boolean? Asignar
        {
            get => _asignar;
            set
            {
                _asignar = value;
                
            }
        }
        //Comandos
        public ICommand command { get; set; }
        public registrarVehiculosViewModel()
        {
            command = new comandoViewModel(ExecuteSaveCommand, CanExecuteSaveCommand);
        }

        private bool CanExecuteSaveCommand(object obj)
        {
            return !string.IsNullOrWhiteSpace(Matricula) &&
           !string.IsNullOrWhiteSpace(NombreCliente);
        }

        private void ExecuteSaveCommand(object obj)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString;
            
            using (var conn = new NpgsqlConnection(connectionString))
            {
                
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    
                    try
                    {
                        
                        // 1. Insertar cliente (si no existe)
                        string insertCliente = @"
                            INSERT INTO cliente (dni, nombre, telefono)
                            VALUES (@dni, @nombre, @telefono)
                            ON CONFLICT (dni) DO NOTHING;";

                        using (var cmdCliente = new NpgsqlCommand(insertCliente, conn))
                        {
                            cmdCliente.Parameters.AddWithValue("dni", DniCliente ?? (object)DBNull.Value);
                            cmdCliente.Parameters.AddWithValue("nombre", NombreCliente ?? (object)DBNull.Value);
                            cmdCliente.Parameters.AddWithValue("telefono", TelefonoCliente ?? (object)DBNull.Value);
                            cmdCliente.ExecuteNonQuery();
                        }

                        // 2. Insertar vehículo
                        string insertVehiculo = @"
                            INSERT INTO vehiculo (matricula, marca, modelo, motivo_ingreso, descripcion, asignado)
                            VALUES (@matricula, @marca, @modelo, @motivo_ingreso, @descripcion, @asignado)
                            ON CONFLICT (matricula) DO NOTHING;";

                        using (var cmdVehiculo = new NpgsqlCommand(insertVehiculo, conn))
                        {
                            cmdVehiculo.Parameters.AddWithValue("matricula", Matricula ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("marca", Marca ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("modelo", Modelo ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("motivo_ingreso", MotivoIngreso ?? (object)DBNull.Value);
                            cmdVehiculo.Parameters.AddWithValue("descripcion", Descripcion ?? (object)DBNull.Value);
                            // Si el checkbox no está marcado, asignar false
                            cmdVehiculo.Parameters.AddWithValue("asignado", Asignar ?? false);
                            cmdVehiculo.ExecuteNonQuery();
                        }

                        // 3. Insertar relación cliente-vehículo
                        string insertRelacion = @"
                            INSERT INTO cliente_vehiculo (dni_cliente, matricula_vehiculo)
                            VALUES (@dni, @matricula)
                            ON CONFLICT DO NOTHING;";

                        using (var cmdRelacion = new NpgsqlCommand(insertRelacion, conn))
                        {
#pragma warning disable CS8604 // Posible argumento de referencia nulo
                            cmdRelacion.Parameters.AddWithValue("dni", DniCliente);
#pragma warning disable CS8604 // Posible argumento de referencia nulo
                            cmdRelacion.Parameters.AddWithValue("matricula", Matricula);
                            cmdRelacion.ExecuteNonQuery();
                        }
                        // Metemos una comprobacion por si el mecanico marca el check para "asignarse" el vehiculo
                        if (Asignar == true)
                        {
                            // Determinar estado según el motivo de ingreso
                            string estadoReparacion = MotivoIngreso == "Problema sin identificar" ? "Diagnóstico" : "Diagnosticando";

                            string insertReparacion = @"
                                INSERT INTO reparacion (matricula_vehiculo, fecha_inicio, descripcion, mecanico_id, estado)
                                VALUES (@matricula, @fecha_inicio, @descripcion, @mecanico_id, @estado);";

                            using (var cmdReparacion = new NpgsqlCommand(insertReparacion, conn))
                            {
                                cmdReparacion.Parameters.AddWithValue("matricula", Matricula ?? (object)DBNull.Value);
                                cmdReparacion.Parameters.AddWithValue("fecha_inicio", DateTime.Now);
                                cmdReparacion.Parameters.AddWithValue("descripcion", Descripcion ?? (object)DBNull.Value);
                                string mecanicoId = UserData.id_mecanico; 
                                cmdReparacion.Parameters.AddWithValue("mecanico_id", mecanicoId);
                                cmdReparacion.Parameters.AddWithValue("estado", estadoReparacion);
                                cmdReparacion.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine("Cliente y vehiculo registrados correctamente.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine("Error: " + ex.Message);
                    }
                }
                conn.Close();
            }
        }
    }
}
