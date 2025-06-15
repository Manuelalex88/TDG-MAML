using Npgsql;
using GestionReparaciones.data;
using GestionReparaciones.model;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using System.Windows;
using System.IO;

namespace GestionReparaciones.viewModel
{
    public class ConfiguracionBDViewModel : BaseViewModel
    {
        #region Campos
        private string _servidor;
        private string _puerto;
        private string _usuario;
        private string _contrasena;
        private string _baseDatos;
        #endregion

        #region Propiedades
        public string Servidor
        {
            get => _servidor;
            set => SetProperty(ref _servidor, value);
        }
        public string Puerto
        {
            get => _puerto;
            set => SetProperty(ref _puerto, value);
        }

        public string Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        public string Contrasena
        {
            get => _contrasena;
            set => SetProperty(ref _contrasena, value);
        }

        public string BaseDatos
        {
            get => _baseDatos;
            set => SetProperty(ref _baseDatos, value);
        }

        #endregion

        #region Comandos
        public ICommand GuardarCommand { get; }
        #endregion

        public ConfiguracionBDViewModel()
        {
            _usuario = string.Empty;
            _contrasena = string.Empty;
            _puerto = string.Empty;
            _servidor = string.Empty;
            _baseDatos = string.Empty;

            var config = GestorConfiguracion.CargarConfiguracion();
            if (!string.IsNullOrWhiteSpace(config?.CadenaConexion))
            {
                var csb = new NpgsqlConnectionStringBuilder(config.CadenaConexion);
                Servidor = csb.Host ?? string.Empty;
                Puerto = csb.Port.ToString();
                Usuario = csb.Username ?? string.Empty;
                Contrasena = csb.Password ?? string.Empty;
                BaseDatos = csb.Database ?? string.Empty;
            }


            GuardarCommand = new comandoViewModel(Guardar);
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
        }

        private void Guardar(object obj)
        {
            // Validar solo datos mínimos de base de datos 
            if (string.IsNullOrWhiteSpace(BaseDatos) ||
                string.IsNullOrWhiteSpace(Servidor) ||
                string.IsNullOrWhiteSpace(Puerto) ||
                string.IsNullOrWhiteSpace(Usuario) ||
                string.IsNullOrWhiteSpace(Contrasena))
            {
                MessageBox.Show("Debe ingresar los datos mínimos de la base de datos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = $"Host={Servidor};Port={Puerto};Username={Usuario};Password={Contrasena};Database={BaseDatos}";

            var nuevaConfig = new ConfiguracionApp
            {
                CadenaConexion = connectionString,
            };

            string ruta = DatosConstantes.rutaConfiguracion;
            var opciones = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(ruta, JsonSerializer.Serialize(nuevaConfig, opciones));

            MessageBox.Show("Configuración guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            if (obj is Window ventana)
                ventana.Close();
        }
    }
}
