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
        // Lista de marcas disponibles
        private readonly List<string> _listaMarcas = new() 
        { "SEAT", "MERCEDES", "BMW", "FIAT", "FERRARI" };

        // Lista de motivos posibles de ingreso al taller
        private readonly List<string> _listaMotivoIngreso = new()
        { "Revisión General", "Cambio de aceite", "Cambio de filtros", "Diagnóstico de motor", "Problema sin identificar" };

        // Propiedades publicas de solo lectura para enlazar en la vista
        public List<string> ListaMarcas => _listaMarcas;
        public List<string> ListaMotivoIngreso => _listaMotivoIngreso;
        #endregion


        #region Campos

        private string _marca = String.Empty;
        private string _modelo = string.Empty;
        private string _matricula = String.Empty;
        private string _motivoIngreso = String.Empty;
        private string _descripcion = String.Empty;
        private string _nombreCliente = String.Empty;
        private string _mensajeError;
        private bool _vehiculoEnTaller;
        private bool _vehiculoEditable;
        private bool _clienteEditable;
        private string _anio = String.Empty;
        private string _dniCliente = String.Empty;
        private string _telefonoCliente = String.Empty;
        private Boolean _asignar = false;
        private bool _mostrarDescripcion = false;
        public string Error => null!;
        public bool EsAdmin => Thread.CurrentPrincipal?.IsInRole("admin") == true;

        // Repositorios de acceso a datos
        private readonly VehiculoRepository _vehiculoRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly ClienteVehiculoRepository _CVRepository;
        #endregion

        #region Propiedades
        public string MarcaVehiculo
        {
            get => _marca;
            set => SetProperty(ref _marca, value.ToUpperInvariant());
        }
        public string ModeloVehiculo
        {
            get => _modelo;
            set => SetProperty(ref _modelo, value.ToUpperInvariant());
        }

        public string MatriculaVehiculo
        {
            get => _matricula;
            set
            {
                if (SetProperty(ref _matricula, value.ToUpperInvariant()))
                {
                    VerificarVehiculoEnTaller(); 
                    BuscarVehiculoEnBD();
                }
            }
        }
        public bool MostrarDescripcion
        {
            get => _mostrarDescripcion;
            set => SetProperty(ref _mostrarDescripcion, value);
        }
        public string MotivoIngresoVehiculo
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
        public string DescripcionVehiculo
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public string NombreCliente
        {
            get => _nombreCliente;
            set => SetProperty(ref _nombreCliente, value);
        }
        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }
        public bool VehiculoEnTaller
        {
            get => _vehiculoEnTaller;
            set
            {
                if (SetProperty(ref _vehiculoEnTaller, value))
                {
                    // Actualizar el estado del boton
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        public bool VehiculoEditable
        {
            get => _vehiculoEditable;
            set => SetProperty(ref _vehiculoEditable, value);
        }
        public bool ClienteEditable
        {
            get => _clienteEditable;
            set => SetProperty(ref _clienteEditable, value);
        }

        public string Anio
        {
            get => _anio;
            set => SetProperty(ref _anio, value);
        }

        public string DniCliente
        {
            get => _dniCliente;
            set
            {
                if(SetProperty(ref _dniCliente, value.ToUpperInvariant()))
                {
                    BuscarClienteEnBD();
                }
            }
        }

        public string TelefonoCliente
        {
            get => _telefonoCliente;
            set => SetProperty(ref _telefonoCliente, value);
        }
        public Boolean Asignar
        {
            get => _asignar;
            set => SetProperty(ref _asignar, value);
        }
        #endregion
        #region Formatos
        //Para el formato del dni/matricula este correcto 
        public string this[string propertyName]
        {
            get
            {
                string result = string.Empty;
                //Para el Dni
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
                } //Para la matricula
                else if (propertyName == nameof(MatriculaVehiculo))
                {
                    if (string.IsNullOrWhiteSpace(MatriculaVehiculo))
                    {
                        result = "La matrícula es obligatoria.";
                    }
                    else if (!MatriculaValida(MatriculaVehiculo))
                    {
                        result = "Formato de matrícula inválido. Ejemplo correcto: 1234BCD. Sin vocales ni Ñ.";
                    }
                } //Para el telefono
                else if (propertyName == nameof(TelefonoCliente))
                {
                    if (string.IsNullOrWhiteSpace(TelefonoCliente))
                    {
                        result = "El teléfono es obligatorio.";
                    }
                    else if (!Regex.IsMatch(TelefonoCliente, @"^\d{9}$"))
                    {
                        result = "El teléfono debe contener exactamente 9 dígitos numéricos.";
                    }
                }

                return result;
            }
        }

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
        public ICommand AgregarVehiculoClienteCommand { get; set; }
        #endregion
        //Constructor
        public RegistrarVehiculosViewModel()
        {
            // Inicializar repositorios
            _vehiculoRepository = new VehiculoRepository();
            _clienteRepository = new ClienteRepository();
            _CVRepository = new ClienteVehiculoRepository();
            //Inicializar campos
            _mensajeError = string.Empty;
            _vehiculoEditable = true;
            _clienteEditable = true;

            // Inicializar comandos
            AgregarVehiculoClienteCommand = new comandoViewModel(AgregarVehiculoCliente, PuedeAgregar);

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
        private void BuscarVehiculoEnBD()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MatriculaVehiculo))
                {
                    VehiculoEditable = true;
                    return;
                }

                if (MatriculaValida(MatriculaVehiculo))
                {
                    var vehiculo = _vehiculoRepository.BuscarPorMatricula(MatriculaVehiculo);
                    if (vehiculo != null)
                    {
                        MarcaVehiculo = vehiculo.Marca;
                        ModeloVehiculo = vehiculo.Modelo;
                        MotivoIngresoVehiculo = vehiculo.MotivoIngreso;
                        DescripcionVehiculo = vehiculo.Descripcion;
                        VehiculoEditable = false; 
                    }
                    else
                    {
                        VehiculoEditable = true;
                    }
                }
                else
                {
                    VehiculoEditable = true;
                }
            }
            catch (Exception ex)
            {
                VehiculoEnTaller = true;
                MensajeError = $"Error al verificar al vehículo: {ex.Message}";
            }
        }
        private void BuscarClienteEnBD()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DniCliente))
                {
                    ClienteEditable = true;
                    return;
                }

                if (string.IsNullOrEmpty(this[nameof(DniCliente)]))
                {
                    var cliente = _clienteRepository.ObtenerPorDni(DniCliente);
                    if (cliente != null)
                    {
                        NombreCliente = cliente.Nombre;
                        TelefonoCliente = cliente.Telefono;
                        ClienteEditable = false; 
                    }
                    else
                    {
                        ClienteEditable = true;
                    }
                }
                else
                {
                    ClienteEditable = true;
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al verificar al cliente: {ex.Message}";
            }
        }
        private void AgregarVehiculoCliente(object obj)
        {
            // Obtener el ID del mecánico desde el hilo actual
            var identity = Thread.CurrentPrincipal?.Identity as IdentidadMecanico;
            var idMecanico = identity?.Name;


            #region Comprobaciones
            // Validar DNI antes de continuar
            string errorDni = this[nameof(DniCliente)];

            if (!string.IsNullOrEmpty(errorDni))
            {
                MessageBox.Show(errorDni, "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            //Comprobar el id_mecanico
            if (string.IsNullOrEmpty(idMecanico))
            {
                MessageBox.Show("El ID del mecánico no está definido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Si ya tiene una factura no pagada no puede entrar al taller 
            if (_CVRepository.VehiculoTieneFacturaPendiente(MatriculaVehiculo))
            {
                MessageBox.Show("Este vehículo tiene facturas pendientes. No puede ingresar al taller hasta que estén pagadas.", "Acceso denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            #endregion
            try
            {
                var clienteA = new Cliente()
                {
                    Dni = DniCliente,
                    Nombre = NombreCliente,
                    Telefono = TelefonoCliente,
                };

                var vehiculoA = new Vehiculo()
                {
                    Matricula = MatriculaVehiculo,
                    Marca = MarcaVehiculo,
                    Modelo = ModeloVehiculo,
                    MotivoIngreso = MotivoIngresoVehiculo,
                    Descripcion = DescripcionVehiculo
                };
                _CVRepository.GuardarClienteVehiculoYAsignar(clienteA, vehiculoA, idMecanico, Asignar);
                MessageBox.Show("Cliente y vehiculo registrados/activados correctamente.");

                //Limpiar los campos
                MatriculaVehiculo = string.Empty;
                MarcaVehiculo = string.Empty;
                ModeloVehiculo = string.Empty;
                MotivoIngresoVehiculo = string.Empty;
                DescripcionVehiculo = string.Empty;
                NombreCliente = string.Empty;
                TelefonoCliente = string.Empty;
                DniCliente = string.Empty;
                Asignar = false;

                // Rehabilitar campos
                _vehiculoEditable = true;
                _clienteEditable = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        //Metodos de verificacion/validacion
        private bool PuedeAgregar(object? obj)
        {
            // Validar los campos que no esten vacios
            bool camposObligatorios = !string.IsNullOrWhiteSpace(MatriculaVehiculo) &&
                             !string.IsNullOrWhiteSpace(NombreCliente) &&
                             !string.IsNullOrWhiteSpace(ModeloVehiculo) &&
                             !string.IsNullOrEmpty(MarcaVehiculo) &&
                             !string.IsNullOrEmpty(MotivoIngresoVehiculo) &&
                             !string.IsNullOrEmpty(TelefonoCliente);
            // Validar el dni y la matricula
            bool dniValido = string.IsNullOrEmpty(this[nameof(DniCliente)]);
            bool matriculaValida = string.IsNullOrEmpty(this[nameof(MatriculaVehiculo)]);
            bool telefonoValido = string.IsNullOrEmpty(this[nameof(TelefonoCliente)]);

            return camposObligatorios && dniValido && matriculaValida && !VehiculoEnTaller;
        }
        private void VerificarVehiculoEnTaller()
        {
            if (MatriculaValida(MatriculaVehiculo))
            {
                try
                {
                    if (_CVRepository.BuscarVehiculoEnTaller(MatriculaVehiculo))
                    {
                        VehiculoEnTaller = true;
                        MensajeError = "*Este vehículo ya está en taller.";
                    }
                    else
                    {
                        VehiculoEnTaller = false;
                        MensajeError = string.Empty;
                    }

                    OnPropertyChanged(nameof(MensajeError));
                }
                catch (Exception ex)
                {
                    MensajeError = $"Error al verificar estado del vehículo: {ex.Message}";
                    VehiculoEnTaller = true;
                }
            }
            else
            {
                VehiculoEnTaller = false;
                MensajeError = string.Empty;
            }
        }

        #endregion

    }
}
