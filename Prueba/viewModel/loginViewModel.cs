using Npgsql;
using Prueba.model;
using Prueba.repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Prueba.viewModel
{
    public class loginViewModel : BaseViewModel
    {
        //Campos
        private string _username;
        private SecureString _password = new SecureString();
        private string _mensajeError;
        private bool _IsViewVisible = true;

        //Propiedades
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
        public bool IsViewVisible
        {
            get => _IsViewVisible;
            set => SetProperty(ref _IsViewVisible, value);
        }
        //Comandos
        public Action CerrarVentanaAction { get; set; } = () => { };

        public ICommand LoginCommand { get; set; }

        //Constructor
        public loginViewModel()
        {
            LoginCommand = new comandoViewModel(ExecuteLoginCommand, CanExecuteLoginCommand);
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
        //Si los datos son validos el boton se habilitara para hacer clic
        private bool CanExecuteLoginCommand(object? obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 5
                || Password == null || Password.Length < 3) validData = false;
            else validData = true;
            return validData;
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

                if (mecanico != null) //MIRAR Y APRENDER
                {
                    var identity = new IdentidadMecanico(mecanico.Id, mecanico.Nombre);
                    var roles = new[] { "mecanico" }; // si usas roles más adelante

                    Thread.CurrentPrincipal = new GenericPrincipal(identity, roles);

                    IsViewVisible = false;
                    CerrarVentanaAction?.Invoke();
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
