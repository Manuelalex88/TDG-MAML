using Npgsql;
using Prueba.data;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class RegistrarVehiculosViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Listas
        public List<string> ListaMarcas { get; set; } = new List<string>
            {
                "Seat", "Mercedes", "BMW", "FIAT", "FERRARI"
            };
        public List<string> ListaMotivoIngreso { get; set; } = new List<string>
             {
                 "Revisión General","Cambio de aceite", "Cambio de filtros", "Diagnóstico de motor","Problema sin identificar"
             };
        #endregion
        #region Campos

        private string _marca = String.Empty;
        private string _modelo = string.Empty;
        private string _matricula = String.Empty;
        private string _motivoIngreso = String.Empty;
        private string _descripcion = String.Empty;
        private string _nombreCliente = String.Empty;
        private string _anio = String.Empty;
        private string _dniCliente = String.Empty;
        private string _telefonoCliente = String.Empty;
        private Boolean _asignar = false;
        private bool _mostrarDescripcion = false;
        public string Error => null!;

        private readonly VehiculoRepository _vehiculoRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly ClienteVehiculoRepository _CVRepository;
        #endregion
        //No son el error
        #region Propiedades
        public string Marca
        {
            get => _marca;
            set => SetProperty(ref _marca, value);
        }
        public string Modelo
        {
            get => _modelo;
            set => SetProperty(ref _modelo, value);
        }

        public string Matricula
        {
            get => _matricula;
            set => SetProperty(ref _matricula, value);
        }
        public bool MostrarDescripcion
        {
            get => _mostrarDescripcion;
            set => SetProperty(ref _mostrarDescripcion, value);
        }
        public string MotivoIngreso
        {
            get => _motivoIngreso;
            set
            {
                if (SetProperty(ref _motivoIngreso, value))
                {
                    MostrarDescripcion = (_motivoIngreso == "Problema sin identificar");
                }
            }
        }
        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public string NombreCliente
        {
            get => _nombreCliente;
            set => SetProperty(ref _nombreCliente, value);
        }

        public string Anio
        {
            get => _anio;
            set => SetProperty(ref _anio, value);
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
        public Boolean Asignar
        {
            get => _asignar;
            set
            {
                _asignar = value;

            }
        }
        #endregion
        #region Formatos
        //Para el formato del dni/matricula este correcto 
        public string this[string propertyName]
        {
            get
            {
                string result = string.Empty;

                if (propertyName == nameof(DniCliente))
                {
                    if (string.IsNullOrWhiteSpace(DniCliente))
                    {
                        result = "El DNI es obligatorio.";
                    }
                    else if (!Regex.IsMatch(DniCliente, @"^\d{8}[A-Z]$"))
                    {
                        result = "El DNI debe tener 8 números seguidos de una letra mayúscula. Ej: 12345678A";
                    }
                    else if (!LetraDniValida(DniCliente))
                    {
                        result = "La letra del DNI no es válida para los números proporcionados.";
                    }
                }
                else if (propertyName == nameof(Matricula))
                {
                    if (string.IsNullOrWhiteSpace(Matricula))
                    {
                        result = "La matrícula es obligatoria.";
                    }
                    else if (!MatriculaValida(Matricula))
                    {
                        result = "Formato de matrícula inválido. Ejemplo correcto: 1234BCD. Sin vocales ni Ñ.";
                    }
                }

                return result;
            }
        }



        //Para la validacion de la letra correcta del DNI
        private bool LetraDniValida(string dni)
        {
            string letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            if (!Regex.IsMatch(dni, @"^\d{8}[A-Z]$")) return false;

            int numero = int.Parse(dni[..8]);
            char letraEsperada = letras[numero % 23];

            return dni[8] == letraEsperada;
        }
        private bool MatriculaValida(string matricula)
        {
            string letrasProhibidas = "AEIOUÑ";

            if (!Regex.IsMatch(matricula, @"^\d{4}[A-Z]{3}$"))
                return false;

            string letras = matricula.Substring(4);

            // Comprobar que no hay letras prohibidas por la DGT española 
            foreach (char c in letras)
            {
                if (letrasProhibidas.Contains(c))
                    return false;
            }

            return true;
        }
        #endregion
        #region Comandos

        public ICommand BuscarVehiculoCommand { get; }
        public ICommand BuscarClienteCommand { get; }
        public ICommand AgregarVehiculoClienteCommand { get; set; }
        #endregion
        //Constructor
        public RegistrarVehiculosViewModel()
        {
            //Inicializar campos
            _vehiculoRepository = new VehiculoRepository();
            _clienteRepository = new ClienteRepository();
            _CVRepository = new ClienteVehiculoRepository();
            //Inicializar comandos
            BuscarVehiculoCommand = new comandoViewModel(BuscarVehiculo);
            BuscarClienteCommand = new comandoViewModel(BuscarCliente);
            AgregarVehiculoClienteCommand = new comandoViewModel(AgregarVehiculoCliente, PuedeAgregar);

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
        private void BuscarVehiculo(object obj)
        {
            if (string.IsNullOrWhiteSpace(Matricula))
            {
                MessageBox.Show("Introduce una matricula para buscar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var vehiculo = _vehiculoRepository.BuscarPorMatricula(Matricula);
                if (vehiculo != null)
                {
                    Marca = vehiculo.Marca;
                    Modelo = vehiculo.Modelo;

                    // Notificar cambios a la UI
                    OnPropertyChanged(nameof(Marca));
                    OnPropertyChanged(nameof(Modelo));

                    MessageBox.Show("Vehiculo encontrado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró ningún vehiculo con esa matricula.", "No encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar vehiculo: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BuscarCliente(object obj)
        {
            if (string.IsNullOrWhiteSpace(DniCliente))
            {
                MessageBox.Show("Introduce un DNI para buscar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var cliente = _clienteRepository.ObtenerPorDni(DniCliente!);

                if (cliente != null)
                {
                    NombreCliente = cliente.Nombre;
                    TelefonoCliente = cliente.Telefono;

                    // Notificar cambios a la UI
                    OnPropertyChanged(nameof(NombreCliente));
                    OnPropertyChanged(nameof(TelefonoCliente));

                    MessageBox.Show("Cliente encontrado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No se encontró ningún cliente con ese DNI.", "No encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar cliente: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool PuedeAgregar(object? obj)
        {
            // Validar que matricula y nombre no estén vacíos
            bool camposObligatorios = !string.IsNullOrWhiteSpace(Matricula) &&
                              !string.IsNullOrWhiteSpace(NombreCliente) &&
                              !string.IsNullOrWhiteSpace(Modelo) &&
                              !string.IsNullOrEmpty(Marca) &&
                              !string.IsNullOrEmpty(MotivoIngreso) &&
                              !string.IsNullOrEmpty(TelefonoCliente);
            // Validar el DNI y la MAtricula
            bool dniValido = string.IsNullOrEmpty(this[nameof(DniCliente)]);
            bool matriculaValida = string.IsNullOrEmpty(this[nameof(Matricula)]);

            return camposObligatorios && dniValido && matriculaValida;
        }
        private void AgregarVehiculoCliente(object obj)
        {
            // Obtener el ID del mecánico desde el hilo actual
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["PostgreSqlConnection"].ConnectionString;
            #region Comprobaciones
            // Validar DNI antes de continuar
            string errorDni = this[nameof(DniCliente)];

            if (!string.IsNullOrEmpty(errorDni))
            {
                MessageBox.Show(errorDni, "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Sale del método, no guarda nada
            }
            //Comprobar el id_mecanico
            if (string.IsNullOrEmpty(idMecanico))
            {
                MessageBox.Show("El ID del mecánico no está definido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion
            try
            {
                _CVRepository.GuardarClienteVehiculoYAsignar(DniCliente, NombreCliente, TelefonoCliente,
                                                       Matricula, Marca, Modelo,
                                                       MotivoIngreso, Descripcion, Asignar,
                                                       idMecanico);
                MessageBox.Show("Cliente y vehiculo registrados/activados correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        #endregion

    }
}
