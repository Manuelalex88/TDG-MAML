using FontAwesome.Sharp;
using Npgsql;
using Prueba.model;
using Prueba.repository;
using Prueba.view;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Prueba.viewModel
{
    public class loginViewModel : BaseViewModel
    {
        #region Campos
        private string _username;
        private SecureString _password = new SecureString();
        private string _mensajeError;
        private bool _conexionBDActiva;
        #endregion

        #region Propiedades


        public bool ConexionBDActiva
        {
            get => _conexionBDActiva;
            set => SetProperty(ref _conexionBDActiva, value);
        }
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
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
        #endregion

        // Constructor
        public loginViewModel()
        {
            //Inicializar
            _username = string.Empty;
            _mensajeError = string.Empty;
            //Comandos
            LoginCommand = new comandoViewModel(ExecuteLoginCommand, CanExecuteLoginCommand);
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
            return !(string.IsNullOrWhiteSpace(Username) || Username.Length < 5 || Password == null || Password.Length < 3);
        }

        private void ExecuteLoginCommand(object obj)
        {
            IntPtr passwordBSTR = IntPtr.Zero;
            string plainPassword = string.Empty;

            try
            {
                passwordBSTR = Marshal.SecureStringToBSTR(Password);
                plainPassword = Marshal.PtrToStringBSTR(passwordBSTR);

                var repo = new MecanicoRepository();
                var mecanico = repo.Login(Username, plainPassword);

                if (mecanico != null)
                {
                    var identity = new IdentidadMecanico(mecanico.Id, mecanico.Nombre);
                    var roles = new[] { "mecanico", "admin" };
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

        #endregion
    }
}
