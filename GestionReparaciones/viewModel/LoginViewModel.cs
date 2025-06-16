using FontAwesome.Sharp;
using Npgsql;
using GestionReparaciones.data;
using GestionReparaciones.model;
using GestionReparaciones.repository;
using GestionReparaciones.view;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace GestionReparaciones.viewModel
{
    public class loginViewModel : BaseViewModel
    {
        #region Campos
        private string _username;
        private SecureString _password = new SecureString();
        private string _mensajeError;
        private string _nombreTaller;
        private bool _conexionBDActiva;
        public string MensajeEstadoConexion => ConexionBDActiva
        ? "✅ Base de datos conectada correctamente."
        : "❌ Error: No se pudo conectar a la base de datos.";
        #endregion

        #region Propiedades


        public bool ConexionBDActiva
        {
            get => _conexionBDActiva;
            set
            {
                if (SetProperty(ref _conexionBDActiva, value))
                    OnPropertyChanged(nameof(MensajeEstadoConexion));
            }
        }
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string NombreTaller
        {
            get => _nombreTaller;
            set => SetProperty(ref _nombreTaller, value);
        }
        public SecureString Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string MensajeError
        {
            get => _mensajeError;
            set => SetProperty(ref _mensajeError, value);
        }
        #endregion

        #region Comandos
        public ICommand LoginCommand { get; set; }
        public ICommand CloseCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand AbrirConfiguracionBDCommand { get; }
        public ICommand MostrarMensajeRecuperarAdminCommand { get; }
        #endregion

        // Constructor
        public loginViewModel()
        {
            //Inicializar
            _username = string.Empty;
            _mensajeError = string.Empty;
            _nombreTaller = string.Empty;
            //Nombre Taller
            NombreTaller = DatosConstantesEstaticos.NombreTaller;
            //Comandos
            LoginCommand = new comandoViewModel(ExecuteLoginCommand, CanExecuteLoginCommand);
            MostrarMensajeRecuperarAdminCommand = new comandoViewModel(MostrarMensajeRecuperarAdmin);
            AbrirConfiguracionBDCommand = new comandoViewModel(ExecuteAbrirConfiguracionBD);
            CloseCommand = new comandoViewModel(o => ((Window)o!).Close());
            MinimizeCommand = new comandoViewModel(o =>
            {
                if (o is Login view)
                {
                    view.MinimizarConAnimacion();
                }
            });
            //Metodos
            VerificarConexionBD();
        }

        #region Métodos

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            if (propertyName != null)
                OnPropertyChanged(propertyName);

            return true;
        }
        private void VerificarConexionBD()
        {
            try
            {
                var repo = new MecanicoRepository();
                ConexionBDActiva = repo.TestConexion(); 
            }
            catch
            {
                ConexionBDActiva = false;
            }
        }
        private bool CanExecuteLoginCommand(object? obj)
        {
            // Quita espacios al inicio y fin para la validación de longitud (Ejemplo (espacio)GestionReparaciones(espacio) ok || Prueb(espacio)a No)
            var trimmedUsername = Username?.Trim() ?? string.Empty;

            return !(string.IsNullOrWhiteSpace(trimmedUsername) || trimmedUsername.Length < 5 || Password == null || Password.Length < 3);
        }
        private void ExecuteAbrirConfiguracionBD(object obj)
        {
            try
            {
                var ventanaConfig = new ConfiguracionBD();

                ventanaConfig.ShowDialog();


                var config = GestorConfiguracion.CargarConfiguracion();
                NombreTaller = config?.NombreTaller ?? "Nombre por defecto";
                VerificarConexionBD();
            }catch (Exception ex) {
                MessageBox.Show($"Error:\n{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private void ExecuteLoginCommand(object obj)
        {

            if (!ConexionBDActiva)
            {
                MessageBox.Show(
                    "No está conectado a ninguna base de datos.\n\nHaga clic en el botón con el ícono de base de datos (abajo a la izquierda) para configurar la conexión.",
                    "Sin conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }
            // Pasamos de secureString a string normal para comparar
            IntPtr passwordBSTR = IntPtr.Zero;
            string plainPassword = string.Empty;
            string[] roles;

            try
            {
                passwordBSTR = Marshal.SecureStringToBSTR(Password);
                plainPassword = Marshal.PtrToStringBSTR(passwordBSTR);

                var repo = new MecanicoRepository();
                var mecanico = repo.Login(Username.Trim(), plainPassword);

                if (mecanico != null)
                {
                    // Creamos los roles
                    var identity = new IdentidadMecanico(mecanico.Id, mecanico.Nombre);
                    
                    if (mecanico.Nombre.Equals("Administrador", StringComparison.OrdinalIgnoreCase))
                    {
                        roles = new[] { "admin" };
                    }
                    else
                    {
                        roles = new[] { "mecanico" };
                    }
                    Thread.CurrentPrincipal = new GenericPrincipal(identity, roles);

                    var ventanaPrincipal = new VentanaPrincipal();
                    ventanaPrincipal.Show();

                    if (obj is Window window)
                        window.Close();
                }
                else
                {
                    MensajeError = "Usuario o contraseña incorrectos.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}\n\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (passwordBSTR != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(passwordBSTR);
            }
        }
        private void MostrarMensajeRecuperarAdmin(Object obj)
        {
            MessageBox.Show("Se ha enviado un correo a su cuenta para la recuperación para la contraseña");
        }
        #endregion
    }
}
