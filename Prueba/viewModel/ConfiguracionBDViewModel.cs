using Npgsql;
using Prueba.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Prueba.data;
using System.IO;
using System.Runtime.CompilerServices;

namespace Prueba.viewModel
{
    public class ConfiguracionBDViewModel : BaseViewModel
    {

        #region Campos
        private string _servidor;
        private string _puerto;
        private string _usuario;
        private string _contrasena;
        private string _baseDatos;
        private string _nombreTaller;
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

        public string NombreTaller
        {
            get => _nombreTaller;
            set => SetProperty(ref _nombreTaller, value);
        }
        #endregion
        #region Comandos
        public ICommand GuardarCommand { get; }
        #endregion

        public ConfiguracionBDViewModel()
        {
            //Instanciar
            _usuario =string.Empty;
            _contrasena =string.Empty;
            _puerto =string.Empty;
            _servidor =string.Empty;
            _baseDatos =string.Empty;
            _nombreTaller =string.Empty;

            //Para la carga de la cadena de conexion
            var config = GestorConfiguracion.CargarConfiguracion();
            if (!string.IsNullOrWhiteSpace(config?.CadenaConexion))
            {
                var csb = new NpgsqlConnectionStringBuilder(config.CadenaConexion);
                Servidor = csb.Host ?? string.Empty;
                Puerto = csb.Port.ToString(); 
                Usuario = csb.Username ?? string.Empty;
                Contrasena = csb.Password ?? string.Empty;
                BaseDatos = csb.Database ?? string.Empty; ;
            }
            //Recuperamos el nombre del taller
            NombreTaller = config?.NombreTaller ?? "Mi Taller";
            //Inicializar comandos
            GuardarCommand = new comandoViewModel(Guardar);
        }
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
        private void Guardar(object obj)
        {
            // Validar el nombre
            if (string.IsNullOrWhiteSpace(NombreTaller) &&
                string.IsNullOrWhiteSpace(BaseDatos) &&
                (string.IsNullOrWhiteSpace(Servidor) ||
                 string.IsNullOrWhiteSpace(Puerto) ||
                 string.IsNullOrWhiteSpace(Usuario) ||
                 string.IsNullOrWhiteSpace(Contrasena)))
            {
                MessageBox.Show("Debe ingresar al menos el Nombre del Taller o los datos mínimos de la base de datos.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string connectionString = string.Empty;

            // Solo montar la cadena si puede
            if (!string.IsNullOrWhiteSpace(Servidor) &&
                !string.IsNullOrWhiteSpace(Puerto) &&
                !string.IsNullOrWhiteSpace(Usuario) &&
                !string.IsNullOrWhiteSpace(Contrasena) &&
                !string.IsNullOrWhiteSpace(BaseDatos))
            {
                connectionString = $"Host={Servidor};Port={Puerto};Username={Usuario};Password={Contrasena};Database={BaseDatos}";
            }

            var nuevaConfig = new ConfiguracionApp
            {
                CadenaConexion = connectionString,
                NombreTaller = this.NombreTaller ?? string.Empty
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
